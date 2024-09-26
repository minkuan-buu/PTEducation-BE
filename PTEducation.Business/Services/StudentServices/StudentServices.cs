using Org.BouncyCastle.Bcpg;
using PTEducation.Business.ApplicationMiddleware;
using PTEducation.Business.Services.AttendanceServices;
using PTEducation.Data.DTO.ResponseModel;
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

        public async Task<DataResultModel<ScoreStudentResModel>> GetScoreByMonth(int Month, int Year, string Token)
        {
            var userId = Authentication.DecodeToken(Token, "userid");
            var User = await _userRepositories.GetSingle(x => x.Id.Equals(userId));
            var Score = await _scoreRepositories.GetList(x => x.TestDateAt.Month == Month && x.TestDateAt.Year == Year && x.Status.Equals(GeneralStatusEnums.Active.ToString()), includeProperties: "ScoreDetails.StudentClass");
            var ListScore = Score.ToList();
            var ScoreDetails = ListScore
                .Where(x => x.ScoreDetails.Any(sd => sd.StudentClass.StudentId.Equals(userId)))
                .ToList();
            List<ScoreStudentDetailResModel> ListScoreDetails = new();
            foreach (var score in ScoreDetails)
            {
                ScoreStudentDetailResModel ScoreDetail = new()
                {
                    TestDateAt = score.TestDateAt,
                    Score = score.ScoreDetails.FirstOrDefault().Score
                };
                ListScoreDetails.Add(ScoreDetail);
            }
            var Result = new ScoreStudentResModel()
            {
                Id = userId,
                Name = TextConvert.ConvertFromUnicodeEscape(User.Name),
                Scores = ListScoreDetails
            };
            return new DataResultModel<ScoreStudentResModel>()
            {
                Data = Result,
            };
        }

        public async Task<DataResultModel<AttendanceStudentResModel>> GetAttendanceByMonth(int Month, int Year, string Token)
        {
            var userId = Authentication.DecodeToken(Token, "userid");
            var User = await _userRepositories.GetSingle(x => x.Id.Equals(userId));
            var Attandance = await _attendanceRepositories.GetList(x => x.EndDate.Month == Month && x.EndDate.Year == Year && x.Status.Equals(GeneralStatusEnums.Active.ToString()), includeProperties: "AttendanceDetails.StudentClass");
            var ListAttandance = Attandance.ToList();
            var AttandanceDetails = ListAttandance
                .Where(x => x.AttendanceDetails.Any(sd => sd.StudentClass.StudentId.Equals(userId)))
                .ToList();
            List<AttendanceStudentDetailResModel> ListAttendanceDetails = new();
            foreach (var attendance in AttandanceDetails)
            {
                AttendanceStudentDetailResModel AttendanceDetail = new()
                {
                    StartDate = attendance.StartDate,
                    EndDate = attendance.EndDate,
                    isPresent = attendance.AttendanceDetails.FirstOrDefault().Status.Equals(GeneralStatusEnums.Active.ToString())
                };
                ListAttendanceDetails.Add(AttendanceDetail);
            }
            var Result = new AttendanceStudentResModel()
            {
                Id = userId,
                Name = TextConvert.ConvertFromUnicodeEscape(User.Name),
                Attendances = ListAttendanceDetails
            };
            return new DataResultModel<AttendanceStudentResModel>()
            {
                Data = Result,
            };
        }

        public async Task<ListDataResultModel<ScoreMonthResModel>> GetScoreMonth(string Token)
        {
            var userId = Authentication.DecodeToken(Token, "userid");
            var Class = await _studentClassRepositories.GetSingle(x => x.StudentId.Equals(userId));
            var Score = await _scoreRepositories.GetList(x => x.ClassId.Equals(Class.ClassId) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            var distinctMonths = Score
                .Select(test => new { test.TestDateAt.Year, test.TestDateAt.Month })  // Lấy ra năm và tháng của từng bài kiểm tra
                .Distinct()  // Lấy ra các tháng khác nhau
                .ToList();
            List<ScoreMonthResModel> ListRes = new();
            foreach(var item in distinctMonths)
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

        public async Task<ListDataResultModel<AttendanceMonthResModel>> GetAttendanceMonth(string Token)
        {
            var userId = Authentication.DecodeToken(Token, "userid");
            var Class = await _studentClassRepositories.GetSingle(x => x.StudentId.Equals(userId));
            var Score = await _attendanceRepositories.GetList(x => x.ClassId.Equals(Class.ClassId) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            var distinctMonths = Score
                .Select(attendance => new { attendance.EndDate.Year, attendance.EndDate.Month })  // Lấy ra năm và tháng của từng bài kiểm tra
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
