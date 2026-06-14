using PTEducation.Business.Services.AttendanceServices;
using PTEducation.Data.Entities;
using Quartz;
using PTEducation.API.Jobs;

namespace PTEducation.API.Scheduling
{
    public class AttendanceScheduler : IAttendanceScheduler
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public AttendanceScheduler(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        public async Task ScheduleAttendanceJobsAsync(Attendance attendance)
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var opensAt = DateTime.SpecifyKind(attendance.Date.ToDateTime(attendance.StartTime), DateTimeKind.Local);
            var closesAt = DateTime.SpecifyKind(attendance.Date.ToDateTime(attendance.EndTime), DateTimeKind.Local);
            var now = DateTime.Now;

            if (opensAt > now)
            {
                var openJob = JobBuilder.Create<AttendanceWindowJob>()
                    .WithIdentity($"attendance-open-{attendance.Id}")
                    .UsingJobData("AttendanceId", attendance.Id.ToString())
                    .UsingJobData("Action", "open")
                    .Build();

                var openTrigger = TriggerBuilder.Create()
                    .WithIdentity($"attendance-open-trigger-{attendance.Id}")
                    .StartAt(new DateTimeOffset(opensAt))
                    .Build();

                await scheduler.DeleteJob(new JobKey($"attendance-open-{attendance.Id}"));
                await scheduler.ScheduleJob(openJob, openTrigger);
            }

            if (closesAt > now)
            {
                var closeJob = JobBuilder.Create<AttendanceWindowJob>()
                    .WithIdentity($"attendance-close-{attendance.Id}")
                    .UsingJobData("AttendanceId", attendance.Id.ToString())
                    .UsingJobData("Action", "close")
                    .Build();

                var closeTrigger = TriggerBuilder.Create()
                    .WithIdentity($"attendance-close-trigger-{attendance.Id}")
                    .StartAt(new DateTimeOffset(closesAt))
                    .Build();

                await scheduler.DeleteJob(new JobKey($"attendance-close-{attendance.Id}"));
                await scheduler.ScheduleJob(closeJob, closeTrigger);
            }
        }

        public async Task RemoveAttendanceJobsAsync(Guid attendanceId)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.DeleteJob(new JobKey($"attendance-open-{attendanceId}"));
            await scheduler.DeleteJob(new JobKey($"attendance-close-{attendanceId}"));
        }
    }
}
