using AutoMapper;
using PTEducation.Business.ApplicationMiddleware;
using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Entities;
using PTEducation.Data.Enums;
using PTEducation.Data.Repositories.AttendanceDetailRepositories;
using PTEducation.Data.Repositories.AttendanceRepositories;
using PTEducation.Data.Repositories.ClassRepositories;
using PTEducation.Data.Repositories.ScoreDetailRepositories;
using PTEducation.Data.Repositories.ScoreRepositories;
using PTEducation.Data.Repositories.UserRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.OverviewServices
{
    public class OverviewServices : IOverviewServices
    {
        private readonly IUserRepositories _userRepositories;
        private readonly IClassRepositories _classRepositories;
        private readonly IAttendanceRepositories _attendanceRepositories;
        private readonly IAttendanceDetailRepositories _attendanceDetailRepositories;
        private readonly IScoreRepositories _scoreRepositories;
        private readonly IScoreDetailRepositories _scoreDetailRepositories;
        private readonly IMapper _mapper;

        public OverviewServices(
            IUserRepositories userRepositories,
            IClassRepositories classRepositories,
            IAttendanceRepositories attendanceRepositories,
            IAttendanceDetailRepositories attendanceDetailRepositories,
            IScoreRepositories scoreRepositories,
            IScoreDetailRepositories scoreDetailRepositories, 
            IMapper mapper)
        {
            _userRepositories = userRepositories;
            _classRepositories = classRepositories;
            _attendanceRepositories = attendanceRepositories;
            _attendanceDetailRepositories = attendanceDetailRepositories;
            _scoreRepositories = scoreRepositories;
            _scoreDetailRepositories = scoreDetailRepositories;
            _mapper = mapper;
        }

        private async Task<(User Student, StudentClass ActiveClass)> ResolveStudentAndClass(string userId)
        {
            var user = await _userRepositories.GetSingle(
                x => x.Id.Equals(userId) && x.Status.Equals(GeneralStatusEnums.Active.ToString()),
                includeProperties: "StudentClasses,StudentGuardianGuardians"
            );

            if (user == null)
            {
                throw new CustomException("User not found!");
            }

            User student = user;
            if (user.Role.Equals(RoleEnums.Guardian.ToString()))
            {
                var relationship = user.StudentGuardianGuardians.FirstOrDefault();
                if (relationship == null)
                {
                    throw new CustomException("No students associated with this guardian.");
                }
                
                student = await _userRepositories.GetSingle(
                    x => x.Id.Equals(relationship.StudentId) && x.Status.Equals(GeneralStatusEnums.Active.ToString()),
                    includeProperties: "StudentClasses"
                );
                
                if (student == null)
                {
                    throw new CustomException("Associated student not found.");
                }
            }

            var activeClass = student.StudentClasses.FirstOrDefault(x => x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            if (activeClass == null)
            {
                throw new CustomException("Student is not assigned to any active class.");
            }

            return (student, activeClass);
        }


        public async Task<DataResultModel<StudentGuardianOverviewResModel>> GetOverviewForStudentOrGuardian(string userId)
        {
            var (student, activeStudentClass) = await ResolveStudentAndClass(userId);
            var classId = activeStudentClass.ClassId;
            var studentClassId = activeStudentClass.Id;

            // Fetch target class with schedules & attendances for NextSession logic
            var targetClass = await _classRepositories.GetSingle(
                c => c.Id == classId,
                includeProperties: "ClassSchedules,Attendances"
            );

            // 1. Class Name
            var className = TextConvert.ConvertFromUnicodeEscape(targetClass.Name);

            // 2. Average Score
            var studentScores = await _scoreDetailRepositories.GetList(
                sd => sd.StudentClassId == studentClassId && sd.Status == GeneralStatusEnums.Active.ToString() && sd.ScoreNavigation.Status == GeneralStatusEnums.Active.ToString(),
                includeProperties: "ScoreNavigation"
            );
            decimal averageScore = studentScores.Any() ? studentScores.Average(sd => sd.Score) : 0;

            // 3. Attendance Rate
            var closedAttendanceDetails = await _attendanceDetailRepositories.GetList(
                ad => ad.StudentClassId == studentClassId && ad.Attendance.Status == AttendanceStatusEnums.Closed.ToString(),
                includeProperties: "Attendance"
            );

            var ownClassDetails = closedAttendanceDetails
                .Where(ad => ad.Attendance.ClassId == classId)
                .ToList();

            var makeUpDetails = closedAttendanceDetails
                .Where(ad => ad.MakeUpSession != null)
                .ToList();

            var totalClosedSessions = ownClassDetails.Count;
            var presentOrLateSessions = 0;

            foreach (var ownAd in ownClassDetails)
            {
                if (ownAd.Status == AttendanceEnums.Present.ToString() ||
                    ownAd.Status == AttendanceEnums.Late.ToString())
                {
                    presentOrLateSessions++;
                }
                else
                {
                    var hasSuccessfulMakeup = makeUpDetails.Any(mu =>
                        mu.MakeUpSession == ownAd.AttendanceId &&
                        (mu.Status == AttendanceEnums.Present.ToString() ||
                         mu.Status == AttendanceEnums.Late.ToString())
                    );

                    if (hasSuccessfulMakeup)
                    {
                        presentOrLateSessions++;
                    }
                }
            }

            decimal attendanceRate = totalClosedSessions > 0
                ? ((decimal)presentOrLateSessions / totalClosedSessions) * 100
                : 0;

            // 4. Next Session
            var now = DateTime.Now;
            DateTime? nextSession = null;
            var currentSession = targetClass.Attendances
                .Where(att => IsWindowOpen(att, now))
                .OrderBy(att => att.Date.ToDateTime(att.StartTime))
                .FirstOrDefault();

            if (currentSession != null)
            {
                nextSession = currentSession.Date.ToDateTime(currentSession.StartTime);
            }
            else
            {
                var nextSessionCandidates = new List<(DateTime StartAt, DateTime EndAt)>();

                foreach (var schedule in targetClass.ClassSchedules.Where(cs => cs.Status.Equals(GeneralStatusEnums.Active.ToString())))
                {
                    var nextScheduledSession = GetNextSessionWindow(targetClass.StartAt, targetClass.EndAt, schedule.DayOfWeek, schedule.StartTime, schedule.EndTime);
                    if (nextScheduledSession.HasValue)
                    {
                        nextSessionCandidates.Add(nextScheduledSession.Value);
                    }
                }

                foreach (var attendance in targetClass.Attendances.Where(att => IsFutureWindow(att, now)))
                {
                    nextSessionCandidates.Add((attendance.Date.ToDateTime(attendance.StartTime), attendance.Date.ToDateTime(attendance.EndTime)));
                }

                if (nextSessionCandidates.Any())
                {
                    nextSession = nextSessionCandidates.OrderBy(candidate => candidate.StartAt).First().StartAt;
                }
            }

            // 5. Recent Attendances
            var studentAttendances = await _attendanceDetailRepositories.GetList(
                ad => ad.StudentClassId == studentClassId && ad.Attendance.ClassId == classId,
                includeProperties: "Attendance"
            );
            var recentAttendances = studentAttendances
                .OrderByDescending(ad => ad.Attendance.Date)
                .ThenByDescending(ad => ad.Attendance.StartTime)
                .Take(5)
                .Select(ad => new AttendanceSessionsResModel
                {
                    Date = ad.Attendance.Date.ToDateTime(TimeOnly.MinValue),
                    StartTime = ad.Attendance.Date.ToDateTime(ad.Attendance.StartTime),
                    EndTime = ad.Attendance.Date.ToDateTime(ad.Attendance.EndTime),
                    SessionType = ad.Attendance.SessionType,
                    Note = ad.Attendance.Note ?? "",
                    AttendanceStatus = ad.Status,
                    Status = ad.Attendance.Status
                })
                .ToList();

            // 6. Recent Scores
            var recentScores = studentScores
                .OrderByDescending(sd => sd.ScoreNavigation.TestDateAt)
                .Take(5)
                .Select(sd => new ScoreSessionResModel
                {
                    TestDateAt = sd.ScoreNavigation.TestDateAt,
                    Shift = sd.ScoreNavigation.Shift,
                    Score = sd.Score.ToString("F1", System.Globalization.CultureInfo.InvariantCulture),
                    Note = sd.Note ?? ""
                })
                .ToList();

            var result = new StudentGuardianOverviewResModel
            {
                StudentName = student.Name,
                ClassName = className,
                AverageScore = averageScore,
                AttendanceRate = attendanceRate,
                NextSession = nextSession,
                RecentAttendances = recentAttendances,
                RecentScores = recentScores
            };

            return new DataResultModel<StudentGuardianOverviewResModel>
            {
                Data = result
            };
        }

        public async Task<DataResultModel<AttendanceStudentGuardianOverviewResModel>>GetAttendanceOverviewForStudentOrGuardian(string userId)
        {
            var (student, activeStudentClass) = await ResolveStudentAndClass(userId);
            var classId = activeStudentClass.ClassId;
            var studentClassId = activeStudentClass.Id;

            // Fetch target class with schedules & attendances for NextSession logic
            var targetClass = await _classRepositories.GetSingle(
                c => c.Id == classId,
                includeProperties: "ClassSchedules,Attendances"
            );

            var closedAttendanceDetails = await _attendanceDetailRepositories.GetList(
                ad => ad.StudentClassId == studentClassId && ad.Attendance.Status == AttendanceStatusEnums.Closed.ToString(),
                includeProperties: "Attendance"
            );

            var ownClassDetails = closedAttendanceDetails
                .Where(ad => ad.Attendance.ClassId == classId)
                .ToList();

            var makeUpDetails = closedAttendanceDetails
                .Where(ad => ad.MakeUpSession != null)
                .ToList();

            var totalClosedSessions = ownClassDetails.Count;
            var presentOrLateSessions = 0;
            var absentSessions = 0;

            foreach (var ownAd in ownClassDetails)
            {
                if (ownAd.Status == AttendanceEnums.Present.ToString() ||
                    ownAd.Status == AttendanceEnums.Late.ToString())
                {
                    presentOrLateSessions++;
                }
                else
                {
                    var hasSuccessfulMakeup = makeUpDetails.Any(mu =>
                        mu.MakeUpSession == ownAd.AttendanceId &&
                        (mu.Status == AttendanceEnums.Present.ToString() ||
                         mu.Status == AttendanceEnums.Late.ToString())
                    );

                    if (hasSuccessfulMakeup)
                    {
                        presentOrLateSessions++;
                    }
                    else if (ownAd.Status == AttendanceEnums.Absent.ToString())
                    {
                        absentSessions++;
                    }
                }
            }

            decimal attendanceRate = totalClosedSessions > 0
                ? ((decimal)presentOrLateSessions / totalClosedSessions) * 100
                : 0;

            var Attendances = await _attendanceRepositories.GetList(
                x => (x.ClassId == classId || x.AttendanceDetailAttendances.Any(y => y.StudentClassId == studentClassId)) && 
                     !x.Status.Equals(GeneralStatusEnums.Inactive.ToString()),
                includeProperties: "AttendanceDetailAttendances"
            );
            var distinctMonths = Attendances
                .Select(attendance => new { attendance.Date.Year, attendance.Date.Month })
                .Distinct()
                .ToList();
            List<AttendanceMonthResModel> ListRes = new();
            foreach (var item in distinctMonths)
            {
                AttendanceMonthResModel AttendanceMonth = new()
                {
                    Id = $"{item.Month.ToString()}/{item.Year.ToString()}",
                    Month = item.Month,
                    Year = item.Year
                };
                ListRes.Add(AttendanceMonth);
            }

            var weeklySchedules = _mapper.Map<List<ClassScheduleResModel>>(
                targetClass.ClassSchedules.Where(cs => cs.Status.Equals(GeneralStatusEnums.Active.ToString())).ToList()
            );

            DateTime now = DateTime.Now;
            int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = now.AddDays(-1 * diff).Date;
            DateTime endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);

            var extraAttendancesThisWeek = Attendances.Where(a =>
                (a.ClassScheduleId == null || a.ClassId != classId) &&
                a.Date.ToDateTime(TimeOnly.MinValue) >= startOfWeek &&
                a.Date.ToDateTime(TimeOnly.MinValue) <= endOfWeek
            ).ToList();

            foreach (var attendance in extraAttendancesThisWeek)
            {
                weeklySchedules.Add(new ClassScheduleResModel
                {
                    DayOfWeek = (byte)attendance.Date.DayOfWeek,
                    StartTime = attendance.StartTime,
                    EndTime = attendance.EndTime
                });
            }

            return new DataResultModel<AttendanceStudentGuardianOverviewResModel>
            {
                Data = new AttendanceStudentGuardianOverviewResModel
                {
                    ClassId = activeStudentClass.ClassId,
                    ClassName = activeStudentClass.Class.Name,
                    StudentName = student.Name,
                    AttendanceRate = attendanceRate,
                    PresentAttendance = presentOrLateSessions,
                    AbsentAttendance = absentSessions,
                    TotalSession = totalClosedSessions,
                    Months = ListRes,
                    WeeklySchedules = weeklySchedules,
                }
            };
        }

        private static bool IsWindowOpen(Attendance attendance, DateTime now)
        {
            if (attendance == null) return false;
            var opensAt = attendance.Date.ToDateTime(attendance.StartTime);
            var closesAt = attendance.Date.ToDateTime(attendance.EndTime);
            return !attendance.Status.Equals(AttendanceStatusEnums.Closed.ToString()) && now >= opensAt && now <= closesAt;
        }

        private static bool IsFutureWindow(Attendance attendance, DateTime now)
        {
            if (attendance == null) return false;
            var opensAt = attendance.Date.ToDateTime(attendance.StartTime);
            return !attendance.Status.Equals(AttendanceStatusEnums.Closed.ToString()) && opensAt > now;
        }

        private static (DateTime StartAt, DateTime EndAt)? GetNextSessionWindow(DateTime startAt, DateTime endAt, byte dayOfWeek, TimeOnly startTime, TimeOnly endTime)
        {
            var now = DateTime.Now;
            var nextSession = now.Date;

            while (nextSession.DayOfWeek != (DayOfWeek)dayOfWeek)
            {
                nextSession = nextSession.AddDays(1);
            }

            nextSession = new DateTime(nextSession.Year, nextSession.Month, nextSession.Day, startTime.Hour, startTime.Minute, 0);
            var nextSessionEndAt = new DateTime(nextSession.Year, nextSession.Month, nextSession.Day, endTime.Hour, endTime.Minute, 0);

            if (nextSession <= now)
            {
                nextSession = nextSession.AddDays(7);
                nextSessionEndAt = nextSessionEndAt.AddDays(7);
            }

            if (nextSession < startAt || nextSession > endAt)
            {
                return null;
            }

            return (nextSession, nextSessionEndAt);
        }
    }
}