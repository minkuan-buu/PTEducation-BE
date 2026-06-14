using PTEducation.Data.DTO.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.StudentServices
{
    public interface IStudentServices
    {
        Task<DataResultModel<ScoreStudentResModel>> GetScoreByMonth(int Month, int Year, string userId);
        Task<DataResultModel<AttendanceStudentResModel>> GetAttendanceByMonth(int Month, int Year, string Token);
        Task<ListDataResultModel<ScoreMonthResModel>> GetScoreMonth(string userId);
        Task<ListDataResultModel<AttendanceMonthResModel>> GetAttendanceMonth(string userId);
    }
}
