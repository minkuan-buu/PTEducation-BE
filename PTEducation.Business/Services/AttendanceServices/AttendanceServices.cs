using AutoMapper;
using PTEducation.Business.ApplicationMiddleware;
using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Enums;
using PTEducation.Data.Repositories.AttendanceRepositories;
using PTEducation.Data.Repositories.ClassRepositories;
using PTEducation.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PTEducation.Data.Repositories.StudentClassRepositories;
using PTEducation.Data.Repositories.AttendanceDetailRepositories;
using PTEducation.Business.Ultilities.FilterCombine;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Globalization;

namespace PTEducation.Business.Services.AttendanceServices
{
    public class AttendanceServices : IAttendanceServices
    {
        private readonly IAttendanceRepositories _attendanceRepositories;
        private readonly IAttendanceDetailRepositories _attendanceDetailRepositories;
        private readonly IClassRepositories _classRepositories;
        private readonly IStudentClassRepositories _studentClassRepositories;
        private readonly IMapper _mapper;
        public AttendanceServices(IAttendanceRepositories attendanceRepositories, IStudentClassRepositories studentClassRepositories, IClassRepositories classRepositories, IMapper mapper, IAttendanceDetailRepositories attendanceDetailRepositories)
        {
            _attendanceDetailRepositories = attendanceDetailRepositories;
            _classRepositories = classRepositories;
            _attendanceRepositories = attendanceRepositories;
            _studentClassRepositories = studentClassRepositories;
            _mapper = mapper;
        }

        public async Task<MessageResultModel> CreateAttendance(AttendanceCreateReqModel attendanceReq, Guid classId)
        {
            var attendanceDate = DateOnly.FromDateTime(attendanceReq.Date);
            TimeOnly startTime = attendanceReq.StartTime;
            TimeOnly endTime = attendanceReq.EndTime;
            var CheckExistAttendance = await _attendanceRepositories.GetSingle(x =>
                x.Date.Equals(attendanceDate) &&
                x.ClassId.Equals(classId) &&
                x.StartTime == startTime &&
                x.EndTime == endTime);
            if (CheckExistAttendance != null)
            {
                throw new CustomException("Attendance already exists");
            }
            var CheckExistClass = await _classRepositories.GetSingle(x => x.Id.Equals(classId) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            if (CheckExistClass == null)
            {
                throw new CustomException("Class not found or not active");
            }
            var NewAttendanceId = Guid.NewGuid();
            var NewAttendance = _mapper.Map<Attendance>(attendanceReq);
            NewAttendance.SessionType = "Adhoc";
            NewAttendance.ClassId = classId;
            NewAttendance.Date = attendanceDate;
            NewAttendance.StartTime = startTime;
            NewAttendance.EndTime = endTime;
            NewAttendance.Id = NewAttendanceId;
            await _attendanceRepositories.Insert(NewAttendance);
            return new MessageResultModel()
            {
                Message = "Ok"
            };
        }

        public async Task<MessageResultModel> SoftDeleteAttendance(Guid Id)
        {
            var CheckExist = await _attendanceRepositories.GetSingle(x => x.Id.Equals(Id), includeProperties: "AttendanceDetails");
            if (CheckExist == null)
            {
                throw new CustomException("Attendance not found");
            }
            if (CheckExist.Status.Equals(GeneralStatusEnums.Inactive.ToString()))
            {
                throw new CustomException("Attendance is inactive!");
            }
            CheckExist.Status = GeneralStatusEnums.Inactive.ToString();
            foreach (var item in CheckExist.AttendanceDetails)
            {
                item.Status = GeneralStatusEnums.Inactive.ToString();
            }
            await _attendanceRepositories.Update(CheckExist);
            return new MessageResultModel()
            {
                Message = "Ok"
            };
        }

        public async Task<ListDataResultModel<AttendanceSessionResModel>> GetAttendanceSessions(Guid classId, DateOnly date)
        {
            var attendances = await _attendanceRepositories.GetList(x => x.ClassId.Equals(classId) && x.Date.Equals(date));
            var result = _mapper.Map<List<AttendanceSessionResModel>>(attendances);
            return new ListDataResultModel<AttendanceSessionResModel>()
            {
                Data = result
            };
        }

        // public async Task<ListDataResultModel<AttendanceListResModel>> GetListAttendance(int? pageIndex, AttendanceFilter filter)
        // {
        //     var allAttendance = await ViewAllAttendance(pageIndex, filter);
        //     var StudentInClass = await _studentClassRepositories.GetList(x => x.ClassId.Equals(filter.ClassId) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
        //     var ListStudentInClassId = StudentInClass.Select(x => x.Id).ToList();
        //     var Result = _mapper.Map<List<AttendanceListResModel>>(allAttendance);
        //     foreach (var Attendance in Result)
        //     {
        //         var AttendanceRaw = allAttendance.FirstOrDefault(x => x.Id.Equals(Attendance.Id));
        //         if (AttendanceRaw == null)
        //         {
        //             continue;
        //         }
        //         var ListStudentNotHaveAttend = ListStudentInClassId.Except(AttendanceRaw.AttendanceDetails.Select(x => x.StudentClassId)).ToList();
        //         Attendance.TotalPresent = AttendanceRaw.AttendanceDetails.Count();
        //         Attendance.TotalAbsent = ListStudentNotHaveAttend.Count;
        //     }
        //     return new ListDataResultModel<AttendanceListResModel>()
        //     {
        //         Data = Result
        //     };
        // }

        public async Task<MessageResultModel> UpdateAttendance(AttendanceUpdateReqModel attendanceReq)
        {
            var CheckExist = await _attendanceRepositories.GetSingle(x => x.Id.Equals(attendanceReq.Id));
            if(CheckExist == null)
            {
                throw new CustomException("Attendance not found");
            }
            CheckExist.Date = DateOnly.FromDateTime(attendanceReq.Date);
            CheckExist.StartTime = attendanceReq.StartTime;
            CheckExist.EndTime = attendanceReq.EndTime;
            CheckExist.ClassScheduleId = attendanceReq.ClassScheduleId;
            CheckExist.SessionType = attendanceReq.SessionType;
            CheckExist.Note = attendanceReq.Note;
            await _attendanceRepositories.Update(CheckExist);
            return new MessageResultModel()
            {
                Message = "Ok"
            };
        }

        // private async Task<List<Attendance>> ViewAllAttendance(int? pageIndex, AttendanceFilter searchModel)
        // {
        //     Func<IQueryable<Attendance>, IOrderedQueryable<Attendance>> orderBy = o => o.OrderByDescending(p => p.Date).ThenByDescending(p => p.StartTime);
        //     Expression<Func<Attendance, bool>> filter = p => p.ClassId.Equals(searchModel.ClassId);

        //     if (searchModel != null)
        //     {
        //         if (searchModel.FromDate.HasValue)
        //         {
        //             filter = filter.And(t => t.Date >= DateOnly.FromDateTime(searchModel.FromDate.Value));
        //         }
        //         if (searchModel.ToDate.HasValue)
        //         {
        //             filter = filter.And(t => t.Date <= DateOnly.FromDateTime(searchModel.ToDate.Value));
        //         }
        //     }

        //     var allAttendance = await _attendanceRepositories.GetList(filter, orderBy, includeProperties: "Class,ClassSchedule,AttendanceDetails.StudentClass.Student", pageIndex ?? 1);

        //     return allAttendance.ToList();
        // }

        public async Task<MessageResultModel> RestoreAttendance(Guid Id)
        {
            var CheckExist = await _attendanceRepositories.GetSingle(x => x.Id.Equals(Id), includeProperties: "AttendanceDetails");
            if (CheckExist == null)
            {
                throw new CustomException("Attendance not found");
            }
            if (CheckExist.Status.Equals(GeneralStatusEnums.Active.ToString()))
            {
                throw new CustomException("Attendance is active!");
            }
            CheckExist.Status = GeneralStatusEnums.Active.ToString();
            foreach (var item in CheckExist.AttendanceDetails)
            {
                item.Status = GeneralStatusEnums.Active.ToString();
            }
            await _attendanceRepositories.Update(CheckExist);
            return new MessageResultModel {
                Message = "Ok",
            };
        }
    }
}
