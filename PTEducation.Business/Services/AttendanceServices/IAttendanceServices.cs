using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.AttendanceServices
{
    public interface IAttendanceServices
    {
        Task<DataResultModel<AttendanceDetailResModel>> GetAttendanceDetail(Guid Id);
        // Task<ListDataResultModel<AttendanceListResModel>> GetListAttendance(int? pageIndex, AttendanceFilter filter);
        Task<MessageResultModel> CreateAttendance(AttendanceCreateReqModel attendanceReq, string token);
        Task<MessageResultModel> UpdateAttendance(AttendanceUpdateReqModel attendanceReq);
        Task<MessageResultModel> SoftDeleteAttendance(Guid Id);
        Task<MessageResultModel> RestoreAttendance(Guid Id);
    }
}
