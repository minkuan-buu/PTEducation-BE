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
        private static string ResolveAttendanceStatus(DateOnly date, TimeOnly startTime, TimeOnly endTime)
        {
            var now = DateTime.Now;
            var opensAt = date.ToDateTime(startTime);
            var closesAt = date.ToDateTime(endTime);

            if (now >= closesAt)
            {
                return AttendanceStatusEnums.Closed.ToString();
            }

            if (now >= opensAt)
            {
                return AttendanceStatusEnums.Opening.ToString();
            }

            return AttendanceStatusEnums.Pending.ToString();
        }

        private readonly IAttendanceRepositories _attendanceRepositories;
        private readonly IAttendanceDetailRepositories _attendanceDetailRepositories;
        private readonly IClassRepositories _classRepositories;
        private readonly IStudentClassRepositories _studentClassRepositories;
        private readonly IMapper _mapper;
        private readonly IAttendanceScheduler _attendanceScheduler;
        public AttendanceServices(IAttendanceRepositories attendanceRepositories, IStudentClassRepositories studentClassRepositories, IClassRepositories classRepositories, IMapper mapper, IAttendanceDetailRepositories attendanceDetailRepositories, IAttendanceScheduler attendanceScheduler)
        {
            _attendanceDetailRepositories = attendanceDetailRepositories;
            _classRepositories = classRepositories;
            _attendanceRepositories = attendanceRepositories;
            _studentClassRepositories = studentClassRepositories;
            _mapper = mapper;
            _attendanceScheduler = attendanceScheduler;
        }

        public async Task<AttendanceMutationResModel> CreateAttendance(AttendanceCreateReqModel attendanceReq, Guid classId)
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
            NewAttendance.SessionType = attendanceReq.SessionType;
            NewAttendance.ClassId = classId;
            NewAttendance.Date = attendanceDate;
            NewAttendance.StartTime = startTime;
            NewAttendance.EndTime = endTime;
            NewAttendance.Status = ResolveAttendanceStatus(attendanceDate, startTime, endTime);
            NewAttendance.Id = NewAttendanceId;
            await _attendanceRepositories.Insert(NewAttendance);
            // schedule open/close jobs if scheduler available
            try
            {
                if (_attendanceScheduler != null)
                {
                    await _attendanceScheduler.ScheduleAttendanceJobsAsync(NewAttendance);
                }
            }
            catch { }
            return new AttendanceMutationResModel
            {
                AttendanceId = NewAttendanceId,
                ClassId = classId,
                Date = NewAttendance.Date,
                StartTime = NewAttendance.StartTime,
                EndTime = NewAttendance.EndTime,
                SessionType = NewAttendance.SessionType,
                Status = NewAttendance.Status
            };
        }

        public async Task<DataResultModel<AttendanceDetailResModel>> GetAttendanceDetail(Guid Id, Guid? classId)
        {
            var CheckExist = await _attendanceRepositories.GetSingle(x => x.Id.Equals(Id), includeProperties: "AttendanceDetailAttendances.StudentClass.Student.StudentGuardianStudents.Guardian,Class,ClassSchedule");
            if (CheckExist == null)
            {
                throw new CustomException("Attendance not found");
            }
            var StudentInClass = await _studentClassRepositories.GetList(x => x.ClassId.Equals(classId != null ? classId : CheckExist.ClassId) && x.Status.Equals(GeneralStatusEnums.Active.ToString()), includeProperties: "Student.StudentGuardianStudents.Guardian");
            var ListStudentInClassId = StudentInClass.Select(x => x.Id).ToList();
            var ListStudentNotHaveAttend = ListStudentInClassId.Except(CheckExist.AttendanceDetailAttendances.Select(x => x.StudentClassId)).ToList();
            var Result = _mapper.Map<AttendanceDetailResModel>(CheckExist);
            if (classId != null)
            {
                var allowedStudentClassIds = CheckExist.AttendanceDetailAttendances
                    .Where(x => x.StudentClass.ClassId.Equals(classId.Value))
                    .Select(x => x.StudentClassId)
                    .ToHashSet();

                Result.AttendanceDetails = Result.AttendanceDetails
                    .Where(x => allowedStudentClassIds.Contains(x.StudentClassId))
                    .ToList();
            }
            foreach (var item in ListStudentNotHaveAttend)
            {
                var Student = StudentInClass.FirstOrDefault(x => x.Id.Equals(item));
                if (Student == null)
                {
                    continue;
                }
                AttendanceDetailStudentResModel attendanceDetailStudent = new()
                {
                    StudentClassId = item,
                    StudentId = Student.StudentId,
                    StudentName = TextConvert.ConvertFromUnicodeEscape(Student.Student.Name),
                    AttendanceStatus = AttendanceEnums.NotYet.ToString(),
                    Guardians = Student.Student.StudentGuardianStudents.Select(x => new UserGuardianListResModel
                    {
                        Id = x.Guardian.Id,
                        Name = x.Guardian.Name,
                        Email = x.Guardian.Email,
                        Phone = x.Guardian.Phone,
                        Relationship = x.Relationship,
                        IsPrimary = x.IsPrimary
                    }).ToList()
                };
                Result.AttendanceDetails.Add(attendanceDetailStudent);
            }
            Result.AttendanceDetails = Result.AttendanceDetails.OrderBy(x => x.StudentName).ToList();
            return new DataResultModel<AttendanceDetailResModel>()
            {
                Data = Result
            };
        }

        public async Task<AttendanceMutationResModel> SoftDeleteAttendance(Guid Id)
        {
            var CheckExist = await _attendanceRepositories.GetSingle(x => x.Id.Equals(Id), includeProperties: "AttendanceDetailAttendances");
            if (CheckExist == null)
            {
                throw new CustomException("Attendance not found");
            }
            if (CheckExist.Status.Equals(GeneralStatusEnums.Inactive.ToString()))
            {
                throw new CustomException("Attendance is inactive!");
            }
            CheckExist.Status = GeneralStatusEnums.Inactive.ToString();
            foreach (var item in CheckExist.AttendanceDetailAttendances)
            {
                item.Status = GeneralStatusEnums.Inactive.ToString();
            }
            await _attendanceRepositories.Update(CheckExist);
            return new AttendanceMutationResModel
            {
                AttendanceId = CheckExist.Id,
                ClassId = CheckExist.ClassId,
                Date = CheckExist.Date,
                StartTime = CheckExist.StartTime,
                EndTime = CheckExist.EndTime,
                SessionType = CheckExist.SessionType,
                Status = CheckExist.Status
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
        //         var ListStudentNotHaveAttend = ListStudentInClassId.Except(AttendanceRaw.AttendanceDetailAttendances.Select(x => x.StudentClassId)).ToList();
        //         Attendance.TotalPresent = AttendanceRaw.AttendanceDetailAttendances.Count();
        //         Attendance.TotalAbsent = ListStudentNotHaveAttend.Count;
        //     }
        //     return new ListDataResultModel<AttendanceListResModel>()
        //     {
        //         Data = Result
        //     };
        // }

        public async Task<MessageResultModel> CheckAttendance(Guid AttendanceId, CheckAttendanceReqModel checkAttendanceReq)
        {
            var CheckExist = await _attendanceRepositories.GetSingle(x => x.Id.Equals(AttendanceId) && x.Status.Equals(AttendanceStatusEnums.Opening.ToString()), includeProperties: "AttendanceDetailAttendances");
            if (CheckExist == null)
            {
                return new MessageResultModel()
                {
                    Message = "Not Found or Attendance is not opening"
                };
            }
            var CheckExistStudentClass = await _studentClassRepositories.GetSingle(x => x.Id.Equals(checkAttendanceReq.StudentClassId) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            if (CheckExistStudentClass == null)
            {
                return new MessageResultModel()
                {
                    Message = "StudentClass not found or not active"
                };
            }
            var CheckExistAttendanceDetail = CheckExist.AttendanceDetailAttendances.FirstOrDefault(x => x.StudentClassId.Equals(checkAttendanceReq.StudentClassId));
            if (CheckExistAttendanceDetail == null)
            {
                AttendanceDetail newAttendanceDetail = new AttendanceDetail()
                {
                    Id = Guid.NewGuid(),
                    AttendanceId = AttendanceId,
                    StudentClassId = checkAttendanceReq.StudentClassId,
                    Status = AttendanceEnums.Present.ToString(),
                    CreatedAt = DateTime.Now,
                    MakeUpSession = checkAttendanceReq.MakeUpSessionId
                };

                await _attendanceDetailRepositories.Insert(newAttendanceDetail);
            }

            return new MessageResultModel()
            {
                Message = "Ok"
            };
        }
        //.StudentClass.Student.StudentGuardianStudents.Guardian

        public async Task<AttendanceMutationResModel> UpdateAttendance(AttendanceUpdateReqModel attendanceReq)
        {
            var CheckExist = await _attendanceRepositories.GetSingle(x => x.Id.Equals(attendanceReq.Id) && x.Status.Equals(AttendanceStatusEnums.Pending.ToString()));
            if (CheckExist == null)
            {
                throw new CustomException("Attendance not found");
            }
            CheckExist.Date = DateOnly.FromDateTime(attendanceReq.Date);
            CheckExist.StartTime = attendanceReq.StartTime;
            CheckExist.EndTime = attendanceReq.EndTime;
            CheckExist.ClassScheduleId = attendanceReq.ClassScheduleId;
            CheckExist.SessionType = attendanceReq.SessionType;
            CheckExist.Note = attendanceReq.Note;
            CheckExist.Status = ResolveAttendanceStatus(CheckExist.Date, CheckExist.StartTime, CheckExist.EndTime);
            await _attendanceRepositories.Update(CheckExist);

            // reschedule jobs if scheduler available
            try
            {
                if (_attendanceScheduler != null)
                {
                    await _attendanceScheduler.RemoveAttendanceJobsAsync(CheckExist.Id);
                    await _attendanceScheduler.ScheduleAttendanceJobsAsync(CheckExist);
                }
            }
            catch { }
            return new AttendanceMutationResModel
            {
                AttendanceId = CheckExist.Id,
                ClassId = CheckExist.ClassId,
                Date = CheckExist.Date,
                StartTime = CheckExist.StartTime,
                EndTime = CheckExist.EndTime,
                SessionType = CheckExist.SessionType,
                Status = CheckExist.Status
            };
        }

        public async Task<AttendanceMutationResModel> CloseAttendance(Guid Id)
        {
            var CheckExist = await _attendanceRepositories.GetSingle(x => x.Id.Equals(Id), includeProperties: "AttendanceDetailAttendances");
            if (CheckExist == null)
            {
                throw new CustomException("Attendance not found");
            }

            var StudentInClass = await _studentClassRepositories.GetList(
                x => x.ClassId.Equals(CheckExist.ClassId) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));

            var ExistingStudentClassIds = CheckExist.AttendanceDetailAttendances.Select(x => x.StudentClassId).ToHashSet();
            var MissingAttendanceDetails = StudentInClass
                .Where(x => !ExistingStudentClassIds.Contains(x.Id))
                .Select(x => new AttendanceDetail
                {
                    Id = Guid.NewGuid(),
                    AttendanceId = CheckExist.Id,
                    StudentClassId = x.Id,
                    Status = AttendanceEnums.Absent.ToString(),
                    CreatedAt = DateTime.Now
                })
                .ToList();

            await using var transaction = await _attendanceRepositories.BeginTransactionAsync();
            try
            {
                if (MissingAttendanceDetails.Count > 0)
                {
                    await _attendanceDetailRepositories.InsertRange(MissingAttendanceDetails, false);
                }

                if (!CheckExist.Status.Equals(AttendanceStatusEnums.Closed.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    CheckExist.Status = AttendanceStatusEnums.Closed.ToString();
                    await _attendanceRepositories.Update(CheckExist, false);
                }

                await _attendanceRepositories.SaveChangesAsync();
                await _attendanceRepositories.CommitTransactionAsync();
            }
            catch
            {
                await _attendanceRepositories.RollbackTransactionAsync();
                throw;
            }

            return new AttendanceMutationResModel
            {
                AttendanceId = CheckExist.Id,
                ClassId = CheckExist.ClassId,
                Date = CheckExist.Date,
                StartTime = CheckExist.StartTime,
                EndTime = CheckExist.EndTime,
                SessionType = CheckExist.SessionType,
                Status = CheckExist.Status
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

        //     var allAttendance = await _attendanceRepositories.GetList(filter, orderBy, includeProperties: "Class,ClassSchedule,AttendanceDetailAttendances.StudentClass.Student", pageIndex ?? 1);

        //     return allAttendance.ToList();
        // }

        public async Task<AttendanceMutationResModel> RestoreAttendance(Guid Id)
        {
            var CheckExist = await _attendanceRepositories.GetSingle(x => x.Id.Equals(Id), includeProperties: "AttendanceDetailAttendances");
            if (CheckExist == null)
            {
                throw new CustomException("Attendance not found");
            }
            if (CheckExist.Status.Equals(GeneralStatusEnums.Active.ToString()))
            {
                throw new CustomException("Attendance is active!");
            }
            CheckExist.Status = GeneralStatusEnums.Active.ToString();
            foreach (var item in CheckExist.AttendanceDetailAttendances)
            {
                item.Status = GeneralStatusEnums.Active.ToString();
            }
            await _attendanceRepositories.Update(CheckExist);
            return new AttendanceMutationResModel
            {
                AttendanceId = CheckExist.Id,
                ClassId = CheckExist.ClassId,
                Date = CheckExist.Date,
                StartTime = CheckExist.StartTime,
                EndTime = CheckExist.EndTime,
                SessionType = CheckExist.SessionType,
                Status = CheckExist.Status
            };
        }

        public async Task<MessageResultModel> UpdateAttendanceV2(Guid AttendanceId, List<AttendanceDetailStudentReqModel> AttendanceReqList)
        {
            var CheckExist = await _attendanceRepositories.GetSingle(x => x.Id.Equals(AttendanceId), includeProperties: "AttendanceDetailAttendances");
            if (CheckExist == null)
            {
                return new MessageResultModel()
                {
                    Message = "Not Found"
                };
            }
            await ApplyAttendanceChanges(AttendanceId, CheckExist.AttendanceDetailAttendances.ToList(), AttendanceReqList);
            return new MessageResultModel()
            {
                Message = "Ok"
            };
        }

        private async Task ApplyAttendanceChanges(Guid attendanceId, List<AttendanceDetail> existingDetails, List<AttendanceDetailStudentReqModel> attendanceReqList)
        {
            var detailsByStudentClassId = existingDetails.ToDictionary(x => x.StudentClassId);
            var distinctRequests = attendanceReqList
                .GroupBy(x => x.StudentClassId)
                .Select(x => x.Last())
                .ToList();

            List<AttendanceDetail> listAddDetail = new();

            foreach (var attendance in distinctRequests)
            {
                if (detailsByStudentClassId.TryGetValue(attendance.StudentClassId, out var existingDetail))
                {
                    existingDetail.Status = attendance.AttendanceStatus;
                }
                else
                {
                    listAddDetail.Add(new AttendanceDetail()
                    {
                        Id = Guid.NewGuid(),
                        AttendanceId = attendanceId,
                        StudentClassId = attendance.StudentClassId,
                        Status = attendance.AttendanceStatus,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            if (listAddDetail.Count > 0)
            {
                await _attendanceDetailRepositories.InsertRange(listAddDetail, false);
            }

            await _attendanceDetailRepositories.SaveChangesAsync();
        }

        public async Task<DataResultModel<List<GeneralDropdownResModel>>> GetStudentAbsentSessions(Guid classId, Guid studentClassId)
        {
            var CheckExist = await _attendanceRepositories.GetList(x => 
                x.ClassId.Equals(classId) && 
                x.Status.Equals(AttendanceStatusEnums.Closed.ToString()) && 
                x.AttendanceDetailAttendances.Any(y => 
                    y.StudentClassId.Equals(studentClassId) && 
                    y.Status.Equals(AttendanceEnums.Absent.ToString()) &&
                    !y.StudentClass.AttendanceDetails.Any(m => 
                        m.MakeUpSession == x.Id && 
                        (m.Status.Equals(AttendanceEnums.Present.ToString()) || m.Status.Equals(AttendanceEnums.Late.ToString()))
                    )
                )
            );
            if (CheckExist.ToList().Count == 0)
            {
                return new DataResultModel<List<GeneralDropdownResModel>>
                {
                    Data = new List<GeneralDropdownResModel>(),
                };
            }
            var ListAbsentSessions = CheckExist.OrderByDescending(x => x.Date).ThenBy(x => x.StartTime).Select(x => new GeneralDropdownResModel
            {
                Id = x.Id,
                Name = x.StartTime.ToString("HH:mm") + " - " + x.EndTime.ToString("HH:mm") + ", " + x.Date.ToString("dd/MM/yyyy")
            }).ToList();
            return new DataResultModel<List<GeneralDropdownResModel>>
            {
                Data = ListAbsentSessions,
            };
        }
    }
}
