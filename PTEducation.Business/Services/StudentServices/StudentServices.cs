using Org.BouncyCastle.Bcpg;
using PTEducation.Business.ApplicationMiddleware;
using PTEducation.Business.Services.AttendanceServices;
using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Entities;
using PTEducation.Data.Enums;
using PTEducation.Data.Repositories.AttendanceRepositories;
using PTEducation.Data.Repositories.ScoreRepositories;
using PTEducation.Data.Repositories.StudentClassRepositories;
using PTEducation.Data.Repositories.UserRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.StudentServices
{
    public class StudentServices : IStudentServices
    {
        private readonly IScoreRepositories _scoreRepositories;
        private readonly IAttendanceRepositories _attendanceRepositories;
        private readonly IUserRepositories _userRepositories;
        private readonly IStudentClassRepositories _studentClassRepositories;
        public StudentServices(IScoreRepositories scoreRepositories, IAttendanceRepositories attendanceRepositories, IUserRepositories userRepositories, IStudentClassRepositories studentClassRepositories)
        {
            _attendanceRepositories = attendanceRepositories;
            _studentClassRepositories = studentClassRepositories;
            _userRepositories = userRepositories;
            _scoreRepositories = scoreRepositories;
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

        public async Task<DataResultModel<ScoreStudentResModel>> GetScoreByMonth(int Month, int Year, string userId)
        {
            var (student, userClass) = await ResolveStudentAndClass(userId);
            var Score = await _scoreRepositories.GetList(x => x.ClassId == userClass.ClassId && x.TestDateAt.Month == Month && x.TestDateAt.Year == Year && x.Status.Equals(GeneralStatusEnums.Active.ToString()), includeProperties: "ScoreDetails.StudentClass");
            var ListScore = Score.ToList();
            List<ScoreStudentDetailResModel> ListScoreDetails = new();
            foreach (var score in ListScore)
            {
                var getStudentScore = score.ScoreDetails.FirstOrDefault(x => x.StudentClass.StudentId.Equals(student.Id));
                ScoreStudentDetailResModel ScoreDetail = new()
                {
                    TestDateAt = score.TestDateAt,
                    Shift = score.Shift != null ? TextConvert.ConvertFromUnicodeEscape(score.Shift) : null,
                    Score = getStudentScore != null ? getStudentScore.Score : 0,
                    Note = getStudentScore != null && getStudentScore.Note != null ? TextConvert.ConvertFromUnicodeEscape(getStudentScore.Note) : null
                };
                ListScoreDetails.Add(ScoreDetail);
            }
            var Result = new ScoreStudentResModel()
            {
                Id = student.Id,
                Name = TextConvert.ConvertFromUnicodeEscape(student.Name),
                Scores = ListScoreDetails.OrderByDescending(x => x.TestDateAt).ToList()
            };
            return new DataResultModel<ScoreStudentResModel>()
            {
                Data = Result,
            };
        }

        public async Task<DataResultModel<AttendanceStudentResModel>> GetAttendanceByMonth(int Month, int Year, string userId)
        {
            var (student, userClass) = await ResolveStudentAndClass(userId);
            var Attandance = await _attendanceRepositories.GetList(
                x => (x.ClassId == userClass.ClassId || x.AttendanceDetailAttendances.Any(y => y.StudentClass.StudentId == student.Id)) && 
                     x.Date.Month == Month && 
                     x.Date.Year == Year && 
                     x.Status.Equals(AttendanceStatusEnums.Closed.ToString()), 
                includeProperties: "AttendanceDetailAttendances.StudentClass, AttendanceDetailAttendances.MakeUpSessionNavigation"
            );
            var ListAttandance = Attandance.ToList();
            List<AttendanceStudentDetailResModel> ListAttendanceDetails = new();
            foreach (var attendance in ListAttandance)
            {
                var getStudentAttandance = attendance.AttendanceDetailAttendances.FirstOrDefault(x => x.StudentClass.StudentId.Equals(student.Id));
                AttendanceStudentDetailResModel AttendanceDetail = new()
                {
                    Date = attendance.Date,
                    StartTime = attendance.StartTime,
                    EndTime = attendance.EndTime,
                    AttendanceStatus = getStudentAttandance != null ? getStudentAttandance.Status : "Pending",
                    MakeUpAttendance = getStudentAttandance != null && getStudentAttandance.MakeUpSessionNavigation != null ? new MakeUpAttendanceDetail
                    {
                        Date = getStudentAttandance.MakeUpSessionNavigation.Date,
                        StartTime = getStudentAttandance.MakeUpSessionNavigation.StartTime,
                        EndTime = getStudentAttandance.MakeUpSessionNavigation.EndTime
                    } : null
                };
                ListAttendanceDetails.Add(AttendanceDetail);
            }
            var Result = new AttendanceStudentResModel()
            {
                Id = student.Id,
                Name = student.Name,
                Attendances = ListAttendanceDetails.OrderByDescending(x => x.Date).ThenByDescending(x => x.StartTime).ToList()
            };
            return new DataResultModel<AttendanceStudentResModel>()
            {
                Data = Result,
            };
        }

        public async Task<ListDataResultModel<ScoreMonthResModel>> GetScoreMonth(string userId)
        {
            var (student, userClass) = await ResolveStudentAndClass(userId);
            var Score = await _scoreRepositories.GetList(x => x.ClassId.Equals(userClass.ClassId) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            var distinctMonths = Score
                .Select(test => new { test.TestDateAt.Year, test.TestDateAt.Month })  // Lấy ra năm và tháng của từng bài kiểm tra
                .Distinct()  // Lấy ra các tháng khác nhau
                .ToList();
            List<ScoreMonthResModel> ListRes = new();
            foreach (var item in distinctMonths)
            {
                ScoreMonthResModel ScoreMonth = new()
                {
                    Id = $"{item.Month.ToString()}/{item.Year.ToString()}",
                    Month = item.Month,
                    Year = item.Year
                };
                ListRes.Add(ScoreMonth);
            }
            return new ListDataResultModel<ScoreMonthResModel>()
            {
                Data = ListRes
            };
        }

        public async Task<ListDataResultModel<AttendanceMonthResModel>> GetAttendanceMonth(string userId)
        {
            var (student, userClass) = await ResolveStudentAndClass(userId);
            var Score = await _attendanceRepositories.GetList(
                x => (x.ClassId == userClass.ClassId || x.AttendanceDetailAttendances.Any(y => y.StudentClass.StudentId == student.Id)) && 
                     !x.Status.Equals(GeneralStatusEnums.Inactive.ToString()),
                includeProperties: "AttendanceDetailAttendances.StudentClass"
            );
            var distinctMonths = Score
                .Select(attendance => new { attendance.Date.Year, attendance.Date.Month })  // Lấy ra năm và tháng của từng bài kiểm tra
                .Distinct()  // Lấy ra các tháng khác nhau
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
            return new ListDataResultModel<AttendanceMonthResModel>()
            {
                Data = ListRes
            };
        }
    }
}
