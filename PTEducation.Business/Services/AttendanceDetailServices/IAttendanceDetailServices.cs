using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.AttendanceDetailServices
{
    public interface IAttendanceDetailServices
    {
        Task<MessageResultModel> UpdateAttendance(AttendanceDetailUpdateReqModel AttendanceReq);
    }
}
