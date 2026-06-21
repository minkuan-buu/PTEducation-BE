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
        Task<ListDataResultModel<AttendanceSessionResModel>> GetAttendanceSessions(Guid classId, DateOnly date);
        Task<DataResultModel<AttendanceDetailResModel>> GetAttendanceDetail(Guid Id, Guid? classId);
        // Task<ListDataResultModel<AttendanceListResModel>> GetListAttendance(int? pageIndex, AttendanceFilter filter);
        Task<MessageResultModel> CheckAttendance(Guid AttendanceId, Guid StudentClassId);
        Task<AttendanceMutationResModel> CreateAttendance(AttendanceCreateReqModel attendanceReq, Guid classId);
        Task<AttendanceMutationResModel> UpdateAttendance(AttendanceUpdateReqModel attendanceReq);
        Task<AttendanceMutationResModel> CloseAttendance(Guid Id);
        Task<AttendanceMutationResModel> SoftDeleteAttendance(Guid Id);
        Task<MessageResultModel> UpdateAttendanceV2(Guid AttendanceId, List<AttendanceDetailStudentReqModel> AttendanceReqList);
        Task<AttendanceMutationResModel> RestoreAttendance(Guid Id);
        Task<DataResultModel<List<GeneralDropdownResModel>>> GetStudentAbsentSessions(Guid classId, Guid studentClassId);
    }
}
