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

        public OverviewServices(
            IUserRepositories userRepositories,
            IClassRepositories classRepositories,
            IAttendanceRepositories attendanceRepositories,
            IAttendanceDetailRepositories attendanceDetailRepositories,
            IScoreRepositories scoreRepositories,
            IScoreDetailRepositories scoreDetailRepositories)
        {
            _userRepositories = userRepositories;
            _classRepositories = classRepositories;
            _attendanceRepositories = attendanceRepositories;
            _attendanceDetailRepositories = attendanceDetailRepositories;
            _scoreRepositories = scoreRepositories;
            _scoreDetailRepositories = scoreDetailRepositories;
        }

        public async Task<DataResultModel<StudentGuardianOverviewResModel>> GetOverviewForStudentOrGuardian(string userId)
        {
            // Retrieve user with necessary relations
            var user = await _userRepositories.GetSingle(
                x => x.Id.Equals(userId),
                includeProperties: "StudentClasses.Class,StudentGuardianGuardians.Student.StudentClasses.Class"
            );

            if (user == null)
            {
                throw new CustomException("User not found!");
            }

            User? studentUser = null;
            if (user.Role.Equals(RoleEnums.Guardian.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var guardianRelation = user.StudentGuardianGuardians.FirstOrDefault();
                if (guardianRelation != null)
                {
                    studentUser = guardianRelation.Student;
                }
            }
            else if (user.Role.Equals(RoleEnums.Student.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                studentUser = user;
            }

            if (studentUser == null)
            {
                throw new CustomException("No associated student found!");
            }

            var activeStudentClass = studentUser.StudentClasses.FirstOrDefault(sc => sc.Status.Equals(GeneralStatusEnums.Active.ToString()));
            if (activeStudentClass == null || activeStudentClass.Class == null)
            {
                throw new CustomException("Student is not enrolled in any class!");
            }

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
            var totalClosedSessions = closedAttendanceDetails.Count();
            var presentOrLateSessions = closedAttendanceDetails.Count(ad =>
                ad.Status == AttendanceEnums.Present.ToString() ||
                ad.Status == AttendanceEnums.Late.ToString()
            );
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