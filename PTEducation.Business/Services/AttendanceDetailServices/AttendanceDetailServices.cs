using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Enums;
using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.AttendanceDetailRepositories;
using PTEducation.Data.Repositories.AttendanceRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.AttendanceDetailServices
{
    public class AttendanceDetailServices : IAttendanceDetailServices
    {
        private readonly IAttendanceDetailRepositories _attendanceDetailRepositories;
        private readonly IAttendanceRepositories _attendanceRepositories;
        public AttendanceDetailServices(IAttendanceDetailRepositories attendanceDetailRepositories, IAttendanceRepositories attendanceRepositories)
        {
            _attendanceDetailRepositories = attendanceDetailRepositories;
            _attendanceRepositories = attendanceRepositories;
        }

        public async Task<MessageResultModel> UpdateAttendance(AttendanceDetailUpdateReqModel AttendanceReq)
        {
            var CheckExist = await _attendanceRepositories.GetSingle(x => x.Id.Equals(AttendanceReq.Id), includeProperties: "AttendanceDetails");
            if (CheckExist == null)
            {
                return new MessageResultModel()
                {
                    Message = "Not Found"
                };
            }
            var AttendanceDetail = CheckExist.AttendanceDetails.ToList();
            List<AttendanceDetail> ListRemoveDetail = new();
            List<AttendanceDetail> ListAddDetail = new();
            foreach (var attendance in AttendanceReq.AttendanceReqList)
            {
                if (attendance.AttendanceStatus.Equals(AttendanceEnums.Absent.ToString()))
                {
                    var AttendanceUpdate = AttendanceDetail.FirstOrDefault(x => x.StudentClassId.Equals(attendance.StudentClassId));
                    if (AttendanceUpdate != null)
                    {
                        ListRemoveDetail.Add(AttendanceUpdate);
                    }
                }
                else
                {
                    var AttendanceUpdate = AttendanceDetail.FirstOrDefault(x => x.StudentClassId.Equals(attendance.StudentClassId));
                    if (AttendanceUpdate == null)
                    {

                        var NewAttendance = new AttendanceDetail()
                        {
                            Id = Guid.NewGuid(),
                            AttendanceId = AttendanceReq.Id,
                            StudentClassId = attendance.StudentClassId,
                            Status = GeneralStatusEnums.Active.ToString(),
                            CreatedAt = DateTime.Now
                        };
                        ListAddDetail.Add(NewAttendance);
                    }
                }
            }
            await _attendanceDetailRepositories.InsertRange(ListAddDetail);
            await _attendanceDetailRepositories.DeleteRange(ListRemoveDetail);
            return new MessageResultModel()
            {
                Message = "Ok"
            };
        }
    }
}
