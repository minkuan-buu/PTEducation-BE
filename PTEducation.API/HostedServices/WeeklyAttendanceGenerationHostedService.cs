using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using PTEducation.Business.Services.AttendanceServices;
using PTEducation.Data.Entities;
using PTEducation.Data.Enums;
using PTEducation.Data.Repositories.AttendanceRepositories;
using PTEducation.Data.Repositories.ClassRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PTEducation.API.HostedServices
{
    public class WeeklyAttendanceGenerationHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WeeklyAttendanceGenerationHostedService> _logger;

        public WeeklyAttendanceGenerationHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<WeeklyAttendanceGenerationHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WeeklyAttendanceGenerationHostedService started.");

            // Run immediately on startup
            await GenerateAllAttendancesAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = GetTimeToNextSunday8Pm(DateTime.Now);
                _logger.LogInformation("Next weekly attendance generation scheduled in {Delay}.", delay);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                await GenerateAllAttendancesAsync(stoppingToken);
            }
        }

        private TimeSpan GetTimeToNextSunday8Pm(DateTime now)
        {
            // Find the next Sunday (if today is Sunday, and it is after 8 PM, this returns next Sunday)
            var nextSunday = now.Date.AddDays(((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7);
            var target = nextSunday.AddHours(20); // 8:00 PM is 20:00

            if (target <= now)
            {
                target = target.AddDays(7);
            }

            return target - now;
        }

        private static DateTime GetMondayOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.Date.AddDays(-1 * diff);
        }

        private static string ResolveAttendanceStatus(DateOnly date, TimeOnly startTime, TimeOnly endTime, DateTime now)
        {
            var opensAt = DateTime.SpecifyKind(date.ToDateTime(startTime), DateTimeKind.Local);
            var closesAt = DateTime.SpecifyKind(date.ToDateTime(endTime), DateTimeKind.Local);

            if (now >= closesAt)
            {
                return AttendanceStatusEnums.Closed.ToString();
            }

            if (now >= opensAt)
            {
                return AttendanceStatusEnums.Opening.ToString();
            }

            return AttendanceStatusEnums.Pending.ToString();
        }

        private async Task GenerateAllAttendancesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting weekly attendance generation scan.");
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var classRepositories = scope.ServiceProvider.GetRequiredService<IClassRepositories>();
                var attendanceRepositories = scope.ServiceProvider.GetRequiredService<IAttendanceRepositories>();
                var attendanceScheduler = scope.ServiceProvider.GetRequiredService<IAttendanceScheduler>();

                var now = DateTime.Now;
                var currentWeekMonday = GetMondayOfWeek(now);
                var startRangeDate = DateOnly.FromDateTime(currentWeekMonday);
                var endRangeDate = DateOnly.FromDateTime(currentWeekMonday.AddDays(13)); // Current week + Next week

                var targetDates = Enumerable.Range(0, 14)
                    .Select(i => DateOnly.FromDateTime(currentWeekMonday.AddDays(i)))
                    .ToList();

                var activeClasses = await classRepositories.GetList(
                    x => x.Status == GeneralStatusEnums.Active.ToString(),
                    includeProperties: "ClassSchedules"
                );

                foreach (var classEntity in activeClasses)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    var activeSchedules = classEntity.ClassSchedules
                        .Where(cs => cs.Status == GeneralStatusEnums.Active.ToString())
                        .ToList();

                    if (!activeSchedules.Any()) continue;

                    // Load existing attendances for this class in our target range
                    var existingAttendances = await attendanceRepositories.GetList(
                        x => x.ClassId == classEntity.Id &&
                             x.Date >= startRangeDate &&
                             x.Date <= endRangeDate &&
                             x.Status != GeneralStatusEnums.Inactive.ToString()
                    );

                    var existingSet = existingAttendances
                        .Select(a => (a.Date, a.StartTime, a.EndTime))
                        .ToHashSet();

                    var newAttendances = new List<Attendance>();

                    foreach (var date in targetDates)
                    {
                        var classStartDateOnly = DateOnly.FromDateTime(classEntity.StartAt);
                        var classEndDateOnly = DateOnly.FromDateTime(classEntity.EndAt);
                        if (date < classStartDateOnly || date > classEndDateOnly)
                        {
                            continue;
                        }

                        var targetDayOfWeek = (byte)date.DayOfWeek;
                        var schedulesForDay = activeSchedules.Where(cs => cs.DayOfWeek == targetDayOfWeek).ToList();

                        foreach (var schedule in schedulesForDay)
                        {
                            if (existingSet.Contains((date, schedule.StartTime, schedule.EndTime)))
                            {
                                continue;
                            }

                            var attendanceId = Guid.NewGuid();
                            var attendance = new Attendance
                            {
                                Id = attendanceId,
                                ClassId = classEntity.Id,
                                ClassScheduleId = schedule.Id,
                                SessionType = "Fixed",
                                Date = date,
                                StartTime = schedule.StartTime,
                                EndTime = schedule.EndTime,
                                Status = ResolveAttendanceStatus(date, schedule.StartTime, schedule.EndTime, now),
                                Note = "Automatically generated by weekly background service"
                            };

                            newAttendances.Add(attendance);
                        }
                    }

                    if (newAttendances.Any())
                    {
                        _logger.LogInformation("Generating {Count} attendances for class '{ClassName}' ({ClassId})", newAttendances.Count, classEntity.Name, classEntity.Id);

                        await using var transaction = await attendanceRepositories.BeginTransactionAsync();
                        try
                        {
                            await attendanceRepositories.InsertRange(newAttendances, false);
                            await attendanceRepositories.SaveChangesAsync();
                            await attendanceRepositories.CommitTransactionAsync();
                        }
                        catch (Exception ex)
                        {
                            await attendanceRepositories.RollbackTransactionAsync();
                            _logger.LogError(ex, "Failed to insert generated attendances for class '{ClassId}'", classEntity.Id);
                            continue;
                        }

                        // Schedule the dynamic Quartz open/close jobs for the newly created attendances
                        foreach (var att in newAttendances)
                        {
                            try
                            {
                                if (attendanceScheduler != null)
                                {
                                    await attendanceScheduler.ScheduleAttendanceJobsAsync(att);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to schedule jobs for generated attendance {AttendanceId}", att.Id);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during weekly attendance generation.");
            }
        }
    }
}
