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
            foreach (var attendance in AttendanceReq.AttendanceReqList)
            {
                if (attendance.AttendanceStatus.Equals(AttendanceEnums.Vắng_mặt.ToString()))
                {
                    var AttendanceUpdate = AttendanceDetail.FirstOrDefault(x => x.StudentClassId.Equals(attendance.StudentClassId));
                    if (AttendanceUpdate != null)
                    {
                        AttendanceDetail.Remove(AttendanceUpdate);
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
                            Status = GeneralStatusEnums.Active.ToString()
                        };
                        AttendanceDetail.Add(NewAttendance);
                    }
                }
            }
            CheckExist.AttendanceDetails = AttendanceDetail;
            await _attendanceRepositories.Update(CheckExist);
            return new MessageResultModel()
            {
                Message = "Ok"
            };
        }
    }
}
