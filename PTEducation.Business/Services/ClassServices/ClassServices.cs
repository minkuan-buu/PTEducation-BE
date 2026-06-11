using AutoMapper;
using Microsoft.Extensions.Logging;
using PTEducation.Business.ApplicationMiddleware;
using PTEducation.Business.Ultilities.Email;
using PTEducation.Business.Ultilities.FilterCombine;
using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Entities;
using PTEducation.Data.Enums;
using PTEducation.Data.Repositories.AttendanceDetailRepositories;
using PTEducation.Data.Repositories.AttendanceRepositories;
using PTEducation.Data.Repositories.ClassRepositories;
using PTEducation.Data.Repositories.OTPRepositories;
using PTEducation.Data.Repositories.ScoreDetailRepositories;
using PTEducation.Data.Repositories.ScoreRepositories;
using PTEducation.Data.Repositories.StudentClassRepositories;
using PTEducation.Data.Repositories.UserRepositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.ClassServices
{
    public class ClassServices : IClassServices
    {
        private readonly IClassRepositories _classRepositories;
        private readonly IAttendanceDetailRepositories _attendanceDetailRepositories;
        private readonly IAttendanceRepositories _attendanceRepositories;
        private readonly IScoreDetailRepositories _scoreDetailRepositories;
        private readonly IScoreRepositories _scoreRepositories;
        private readonly IUserRepositories _userRepositories;
        private readonly IStudentClassRepositories _studentClassRepositories;
        private readonly IOTPRepositories _otpRepositories;
        private readonly IEmail _email;
        private readonly IMapper _mapper;
        public ClassServices(IClassRepositories classRepositories, IMapper mapper, IUserRepositories userRepositories, IStudentClassRepositories studentClassRepositories, IEmail email, IAttendanceDetailRepositories attendanceDetailRepositories, IAttendanceRepositories attendanceRepositories, IScoreDetailRepositories scoreDetailRepositories, IScoreRepositories scoreRepositories, IOTPRepositories otpRepositories)
        {
            _classRepositories = classRepositories;
            _userRepositories = userRepositories;
            _studentClassRepositories = studentClassRepositories;
            _email = email;
            _mapper = mapper;
            _attendanceDetailRepositories = attendanceDetailRepositories;
            _attendanceRepositories = attendanceRepositories;
            _scoreDetailRepositories = scoreDetailRepositories;
            _scoreRepositories = scoreRepositories;
            _otpRepositories = otpRepositories;
        }

        public async Task<DataResultModel<ClassDetailResModel>> GetClassDetail(Guid Id)
        {
            var Class = await _classRepositories.GetSingle(x => x.Id == Id, includeProperties: "StudentClasses.Student,CreatedByNavigation");
            if (Class == null)
            {
                throw new CustomException("Class not found!");
            }
            var ClassRes = _mapper.Map<ClassDetailResModel>(Class);
            return new DataResultModel<ClassDetailResModel>()
            {
                Data = ClassRes
            };
        }

        public async Task<PagedListDataResultModel<ListClassResModel>> GetClassList(int? pageIndex, ClassFilter searchModel)
        {
            var ListClass = await ViewAllClasses(pageIndex, 5, searchModel);
            return new PagedListDataResultModel<ListClassResModel>()
            {
                Data = _mapper.Map<List<ListClassResModel>>(ListClass.Data),
                PageNumber = pageIndex ?? 1,
                PageSize = 5,
                TotalPages = ListClass.TotalPages
            };
        }

        public async Task<DataResultModel<List<ListClassResModel>>> GetClassList()
        {
            var ListClass = await _classRepositories.GetList(x => x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            return new DataResultModel<List<ListClassResModel>>()
            {
                Data = _mapper.Map<List<ListClassResModel>>(ListClass)
            };
        }

        public async Task<ListDataResultModel<ClassListSelectResModel>> GetClassSelectList()
        {
            var ListClass = await _classRepositories.GetList(x => x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            return new ListDataResultModel<ClassListSelectResModel>()
            {
                Data = _mapper.Map<List<ClassListSelectResModel>>(ListClass)
            };
        }

        public async Task<MessageResultModel> CreateClass(ClassCreateReqModel ClassReq, string userId)
        {
            var CheckExist = await _classRepositories.GetSingle(x => x.Name.Equals(TextConvert.ConvertToUnicodeEscape(ClassReq.Name)) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            if (CheckExist != null)
            {
                throw new CustomException("Class name is existed!");
            }
            var ClassId = Guid.NewGuid();
            var NewClass = _mapper.Map<Class>(ClassReq);
            NewClass.Id = ClassId;
            NewClass.CreatedBy = userId;
            if (ClassReq.Students == null)
            {
                return new MessageResultModel()
                {
                    Message = "Ok"
                };
            }
            List<User> ListNewUser = new();
            List<StudentClass> ListNewStudentClass = new();
            List<EmailReqModel> ListSendEmail = new();
            foreach (var item in ClassReq.Students)
            {
                string FilePath = "../PTEducation.Business/TemplateEmail/FirstInformation.html";
                string Html = File.ReadAllText(FilePath);
                var NewUser = _mapper.Map<User>(item);
                var GeneratePassword = ClassReq.DefaultPassword ?? Environment.GetEnvironmentVariable("STUDENT_DEFAULT_PASSWORD") ?? throw new CustomException("Default student password is not configured in the system. Please contact the administrator.");
                CreateHashPasswordModel HashedPassword = Authentication.CreateHashPassword(GeneratePassword);
                NewUser.Password = HashedPassword.HashedPassword;
                NewUser.Salt = HashedPassword.Salt;
                NewUser.Role = RoleEnums.Student.ToString();
                NewUser.IsNeedResetPassword = true;
                ListNewUser.Add(NewUser);
                StudentClass NewStudentClass = new()
                {
                    Id = Guid.NewGuid(),
                    ClassId = ClassId,
                    StudentId = item.Id,
                    Status = GeneralStatusEnums.Active.ToString(),
                };
                Html = Html.Replace("{{ID}}", item.Id);
                Html = Html.Replace("{{Password}}", GeneratePassword);
                Html = Html.Replace("{{Email}}", item.Email);
                var EmailReq = new EmailReqModel
                {
                    Email = item.Email,
                    HtmlContent = Html
                };
                ListSendEmail.Add(EmailReq);
                ListNewStudentClass.Add(NewStudentClass);
            }

            // await _userRepositories.InsertRange(ListNewUser);
            // await _classRepositories.Insert(NewClass);
            // await _studentClassRepositories.InsertRange(ListNewStudentClass);
            // await _email.SendEmail("[Thông tin đăng nhập]", ListSendEmail);
            var transaction = await _classRepositories.BeginTransactionAsync();
            try
            {
                await _classRepositories.Insert(NewClass, false);
                await _userRepositories.InsertRange(ListNewUser, false);
                await _studentClassRepositories.InsertRange(ListNewStudentClass, false);

                await _classRepositories.SaveChangesAsync(); // commit dữ liệu vào DB
                await _classRepositories.CommitTransactionAsync();

                await _email.SendEmail("[Thông tin đăng nhập]", ListSendEmail);

                return new MessageResultModel()
                {
                    Message = "Ok"
                };
            }
            catch (Exception ex)
            {
                await _classRepositories.RollbackTransactionAsync();
                if (ex.Message.Contains("cannot be tracked because another instance with the key value"))
                {
                    // Tìm Id bị trùng trong message
                    var match = System.Text.RegularExpressions.Regex.Match(
                        ex.Message, @"\{Id:\s*(\d+)\}"
                    );

                    var duplicatedId = match.Success ? match.Groups[1].Value : "unknown";

                    throw new CustomException($"Phát hiện sinh viên với Id {duplicatedId} đã tồn tại trong hệ thống hoặc trong danh sách đang thêm vào. Vui lòng kiểm tra lại danh sách sinh viên.");
                }
                else if (ex.InnerException.Message.Contains("duplicate key") && ex.InnerException.Message.Contains("dbo.User"))
                {
                    throw new CustomException("Phát hiện trùng lặp ID sinh viên. Vui lòng kiểm tra lại danh sách sinh viên.");
                }
                else
                {
                    throw new CustomException("Đã xảy ra lỗi trong quá trình tạo lớp học. Vui lòng thử lại sau.");
                }
                throw;
            }
        }

        public async Task<MessageResultModel> CreateClassV2(ClassCreateReqModelV2 ClassReq, string userId)
        {
            var CheckExist = await _classRepositories.GetSingle(x => x.Name.Equals(TextConvert.ConvertToUnicodeEscape(ClassReq.Name)) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            if (CheckExist != null)
            {
                throw new CustomException("Tên lớp đã tồn tại");
            }
            var ClassId = Guid.NewGuid();
            var NewClass = _mapper.Map<Class>(ClassReq);
            NewClass.Id = ClassId;
            NewClass.CreatedBy = userId;
            foreach (var schedule in ClassReq.Schedules)
            {
                ClassSchedule NewSchedule = new()
                {
                    Id = Guid.NewGuid(),
                    ClassId = ClassId,
                    DayOfWeek = schedule.DayOfWeek,
                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime,
                    Status = GeneralStatusEnums.Active.ToString()
                };
                NewClass.ClassSchedules.Add(NewSchedule);
            }
            await _classRepositories.Insert(NewClass);
            return new MessageResultModel()
            {
                Message = "Ok"
            };
        }

        public async Task<MessageResultModel> UpdateClass(ClassUpdateReqModel ClassReq)
        {
            var CheckExist = await _classRepositories.GetSingle(x => x.Id == ClassReq.Id);
            if (CheckExist == null)
            {
                throw new CustomException("Class not found!");
            }
            var CheckExistName = await _classRepositories.GetSingle(x => x.Name.Equals(TextConvert.ConvertToUnicodeEscape(ClassReq.Name)) && x.Status.Equals(GeneralStatusEnums.Active.ToString()) && x.Id != ClassReq.Id);
            if (CheckExistName != null)
            {
                throw new CustomException("Class name is existed!");
            }
            CheckExist.Name = TextConvert.ConvertToUnicodeEscape(ClassReq.Name);
            CheckExist.StartAt = ClassReq.StartAt;
            CheckExist.EndAt = ClassReq.EndAt;
            await _classRepositories.Update(CheckExist);
            return new MessageResultModel()
            {
                Message = "Ok"
            };
        }

        public async Task<MessageResultModel> SoftDeleteClass(Guid Id)
        {
            var CheckExist = await _classRepositories.GetSingle(x => x.Id == Id, includeProperties: "StudentClasses,Scores.ScoreDetails,Attendances.AttendanceDetails");
            if (CheckExist == null)
            {
                throw new CustomException("Class not found!");
            }
            if (CheckExist.Status.Equals(GeneralStatusEnums.Inactive.ToString()))
            {
                throw new CustomException("Class is deleted!");
            }
            CheckExist.Status = GeneralStatusEnums.Inactive.ToString();
            foreach (var item in CheckExist.StudentClasses)
            {
                item.Status = GeneralStatusEnums.Inactive.ToString();
                foreach (var score in CheckExist.Scores)
                {
                    score.Status = GeneralStatusEnums.Inactive.ToString();
                    foreach (var scoreDetail in score.ScoreDetails)
                    {
                        scoreDetail.Status = GeneralStatusEnums.Inactive.ToString();
                    }
                }
                foreach (var attendance in CheckExist.Attendances)
                {
                    attendance.Status = GeneralStatusEnums.Inactive.ToString();
                    foreach (var attendanceDetail in attendance.AttendanceDetails)
                    {
                        attendanceDetail.Status = GeneralStatusEnums.Inactive.ToString();
                    }
                }
            }
            await _classRepositories.Update(CheckExist);
            return new MessageResultModel()
            {
                Message = "Ok"
            };
        }

        public async Task<MessageResultModel> RestoreClass(Guid Id)
        {
            var CheckExist = await _classRepositories.GetSingle(x => x.Id == Id, includeProperties: "StudentClasses,Scores.ScoreDetails,Attendances.AttendanceDetails");
            if (CheckExist == null)
            {
                throw new CustomException("Class not found!");
            }
            if (CheckExist.Status.Equals(GeneralStatusEnums.Active.ToString()))
            {
                throw new CustomException("Class is active!");
            }
            CheckExist.Status = GeneralStatusEnums.Active.ToString();
            foreach (var item in CheckExist.StudentClasses)
            {
                item.Status = GeneralStatusEnums.Active.ToString();
                foreach (var score in CheckExist.Scores)
                {
                    score.Status = GeneralStatusEnums.Active.ToString();
                    foreach (var scoreDetail in score.ScoreDetails)
                    {
                        scoreDetail.Status = GeneralStatusEnums.Active.ToString();
                    }
                }
                foreach (var attendance in CheckExist.Attendances)
                {
                    attendance.Status = GeneralStatusEnums.Active.ToString();
                    foreach (var attendanceDetail in attendance.AttendanceDetails)
                    {
                        attendanceDetail.Status = GeneralStatusEnums.Active.ToString();
                    }
                }
            }
            await _classRepositories.Update(CheckExist);
            return new MessageResultModel()
            {
                Message = "Ok"
            };
        }

        public async Task<MessageResultModel> HardDeleteClass(Guid Id)
        {
            var CheckExist = await _classRepositories.GetSingle(
                x => x.Id == Id,
                includeProperties: "StudentClasses.Student.Otps,Scores.ScoreDetails,Attendances.AttendanceDetails"
            );

            if (CheckExist == null)
            {
                throw new CustomException("Class not found!");
            }

            var transaction = await _classRepositories.BeginTransactionAsync();
            try
            {
                // 1. Lấy danh sách user từ StudentClasses
                var usersToDelete = CheckExist.StudentClasses
                    .Select(sc => sc.Student)
                    .Where(u => u != null)
                    .Distinct()
                    .ToList();

                var userOTP = usersToDelete
                    .SelectMany(u => u.Otps)
                    .ToList();

                // 2. Xóa AttendanceDetails
                var attendanceDetails = CheckExist.Attendances
                    .SelectMany(a => a.AttendanceDetails)
                    .ToList();
                await _attendanceDetailRepositories.DeleteRange(attendanceDetails);

                // 3. Xóa Attendance
                await _attendanceRepositories.DeleteRange(CheckExist.Attendances.ToList());

                // 4. Xóa ScoreDetails
                var scoreDetails = CheckExist.Scores
                    .SelectMany(s => s.ScoreDetails)
                    .ToList();
                await _scoreDetailRepositories.DeleteRange(scoreDetails);

                // 5. Xóa Scores
                await _scoreRepositories.DeleteRange(CheckExist.Scores.ToList());

                // 6. Xóa StudentClasses
                await _studentClassRepositories.DeleteRange(CheckExist.StudentClasses.ToList());

                // 7. Xóa Otps của Users
                await _otpRepositories.DeleteRange(userOTP);

                // 8. Xóa Users (Student)
                await _userRepositories.DeleteRange(usersToDelete);

                // 9. Xóa Class
                await _classRepositories.Delete(CheckExist);

                await _classRepositories.SaveChangesAsync();
                await _classRepositories.CommitTransactionAsync();

                return new MessageResultModel()
                {
                    Message = "Ok"
                };
            }
            catch (Exception ex)
            {
                await _classRepositories.RollbackTransactionAsync();
                throw new CustomException("Error occurred while deleting class!");
            }
        }

        public async Task<MessageResultModel> ManualAddStudent(ManualAddStudentClassModel AddStudentsReq)
        {
            var WarningMessage = "Cảnh báo: ";
            var CheckExist = await _classRepositories.GetSingle(x => x.Id.Equals(AddStudentsReq.Id) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            if (CheckExist == null)
            {
                throw new CustomException("Class not found!");
            }
            List<User> ListNewUser = new();
            List<StudentClass> ListNewStudentClass = new();
            List<EmailReqModel> ListSendEmail = new();
            string FilePath = "../PTEducation.Business/TemplateEmail/FirstInformation.html";
            string Html = File.ReadAllText(FilePath);
            foreach (var item in AddStudentsReq.Students)
            {
                var CheckExistStudent = await _userRepositories.GetSingle(x => x.Id.Equals(item.Id));
                if (CheckExistStudent == null)
                {
                    var NewUser = _mapper.Map<User>(item);
                    var GeneratePassword = AddStudentsReq.DefaultPassword ?? Environment.GetEnvironmentVariable("STUDENT_DEFAULT_PASSWORD") ?? throw new CustomException("Default student password is not configured in the system. Please contact the administrator.");
                    CreateHashPasswordModel HashedPassword = Authentication.CreateHashPassword(GeneratePassword);
                    NewUser.Password = HashedPassword.HashedPassword;
                    NewUser.Salt = HashedPassword.Salt;
                    NewUser.Role = RoleEnums.Student.ToString();
                    NewUser.IsNeedResetPassword = true;
                    ListNewUser.Add(NewUser);
                    Html = Html.Replace("{{ID}}", item.Id);
                    Html = Html.Replace("{{Password}}", GeneratePassword);
                    Html = Html.Replace("{{Email}}", item.Email);
                    var EmailReq = new EmailReqModel
                    {
                        Email = item.Email,
                        HtmlContent = Html
                    };
                    ListSendEmail.Add(EmailReq);
                }
                var CheckExistStudentClass = await _studentClassRepositories.GetList(x => !x.ClassId.Equals(AddStudentsReq.Id) && x.StudentId.Equals(item.Id) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
                if (CheckExistStudentClass == null || !CheckExistStudentClass.Any())
                {
                    StudentClass NewStudentClass = new()
                    {
                        Id = Guid.NewGuid(),
                        ClassId = AddStudentsReq.Id,
                        StudentId = item.Id,
                        Status = GeneralStatusEnums.Active.ToString(),
                    };
                    ListNewStudentClass.Add(NewStudentClass);
                }
                else
                {
                    WarningMessage += $"Học viên {item.Name} ({item.Id}) đã có trong lớp khác. Vui lòng kiểm tra lại danh sách sinh viên.\n";
                }

            }
            var transaction = await _classRepositories.BeginTransactionAsync();
            try
            {
                await _userRepositories.InsertRange(ListNewUser, false);
                await _studentClassRepositories.InsertRange(ListNewStudentClass, false);

                await _classRepositories.SaveChangesAsync(); // commit dữ liệu vào DB
                await _classRepositories.CommitTransactionAsync();
                await _email.SendEmail("[Thông tin đăng nhập]", ListSendEmail);
                return new MessageResultModel()
                {
                    Message = "Thêm học viên thành công!" + WarningMessage
                };
            }
            catch (Exception ex)
            {
                await _classRepositories.RollbackTransactionAsync();
                if (ex.InnerException.Message.Contains("duplicate key") && ex.InnerException.Message.Contains("dbo.User"))
                {
                    throw new CustomException("Phát hiện trùng lặp ID sinh viên. Vui lòng kiểm tra lại danh sách sinh viên.");
                }
                else
                {
                    throw new CustomException("Đã xảy ra lỗi trong quá trình tạo lớp học. Vui lòng thử lại sau.");
                }
                throw;
            }
        }

        public async Task<MessageResultModel> MoveOutStudent(MoveOutStudentClassModel MoveOutReq)
        {
            var CheckExistStudent = await _userRepositories.GetSingle(x => x.StudentClasses.Any(sc => sc.Id.Equals(MoveOutReq.StudentId)), includeProperties: "StudentClasses");
            if (CheckExistStudent == null)
            {
                throw new CustomException("Học viên không tồn tại!");
            }
            var CheckExistClass = await _classRepositories.GetSingle(x => x.Id.Equals(MoveOutReq.TargetClassId) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            if (CheckExistClass == null)
            {
                throw new CustomException("Lớp học không tồn tại hoặc đã bị xóa!");
            }
            var CheckExistStudentClass = await _studentClassRepositories.GetSingle(x => x.Id.Equals(MoveOutReq.StudentId) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            if (CheckExistStudentClass.ClassId == MoveOutReq.TargetClassId)
            {
                throw new CustomException("Học viên đã có trong lớp học này!");
            }
            CheckExistStudentClass.ClassId = MoveOutReq.TargetClassId;
            await _studentClassRepositories.Update(CheckExistStudentClass);
            return new MessageResultModel()
            {
                Message = "Ok"
            };
        }

        private async Task<PagedListDataResultModel<Class>> ViewAllClasses(int? pageIndex, int? pageSize, ClassFilter searchModel)
        {
            Func<IQueryable<Class>, IOrderedQueryable<Class>> orderBy = o => o.OrderBy(p => p.Name);
            Expression<Func<Class, bool>> filter = p => true;

            if (searchModel != null)
            {
                if (searchModel.OrderCreatedAt.HasValue)
                {
                    if (searchModel.OrderCreatedAt.Value)
                    {
                        orderBy = orderBy.AndThen(q => q.OrderByDescending(p => p.CreatedAt));
                    }
                    else
                    {
                        orderBy = orderBy.AndThen(q => q.OrderBy(p => p.CreatedAt));
                    }
                }

                if (searchModel.StartAt != null && searchModel.EndAt != null)
                {
                    filter = filter.And(p => p.StartAt >= searchModel.StartAt && p.EndAt <= searchModel.EndAt);
                }

                if (searchModel.StartAt != null)
                {
                    filter = filter.And(p => p.StartAt >= searchModel.StartAt);
                }

                if (searchModel.EndAt != null)
                {
                    filter = filter.And(p => p.EndAt <= searchModel.EndAt);
                }

                if (!string.IsNullOrEmpty(searchModel.Keyword))
                {
                    filter = filter.And(p => p.Name.ToLower().Contains(searchModel.Keyword.ToLower()));
                }
            }

            var allClass = await _classRepositories.GetPagedList(filter, orderBy, includeProperties: "StudentClasses.Student,CreatedByNavigation", pageIndex ?? 1, pageSize ?? 10);

            return allClass;
        }

        public async Task<DataResultModel<Guid>> GetClassIdByName(string ClassName)
        {
            var Class = await _classRepositories.GetSingle(x => x.Name.Equals(TextConvert.ConvertToUnicodeEscape(ClassName)) && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            if (Class == null)
            {
                throw new CustomException("Class not found!");
            }
            return new DataResultModel<Guid>()
            {
                Data = Class.Id
            };
        }

        public async Task<DataResultModel<ClassDetailMetaData>> GetClassMetadata(Guid ClassId)
        {
            var Class = await _classRepositories.GetSingle(x => x.Id == ClassId, includeProperties: "ClassSchedules");
            if (Class == null)
            {
                throw new CustomException("Class not found!");
            }

            var studentClasses = await _studentClassRepositories.GetList(
                x => x.ClassId == ClassId,
                includeProperties: "Student"
            );
            var scores = await _scoreRepositories.GetList(
                x => x.ClassId == ClassId,
                includeProperties: "ScoreDetails"
            );
            var attendances = await _attendanceRepositories.GetList(
                x => x.ClassId == ClassId,
                includeProperties: "AttendanceDetails"
            );

            Class.StudentClasses = studentClasses.ToList();
            Class.Scores = scores.ToList();
            Class.Attendances = attendances.ToList();

            var TotalStudent = Class.StudentClasses.Count(sc => sc.Status.Equals(GeneralStatusEnums.Active.ToString()) && sc.Student.Status.Equals(AccountStatusEnums.Active.ToString()));
            var TotalPendingStudent = Class.StudentClasses.Count(sc => sc.Student.Status.Equals(AccountStatusEnums.PendingApproved.ToString()));
            var TotalScore = Class.Scores.Count(s => s.Status.Equals(GeneralStatusEnums.Active.ToString()));

            // Tách riêng danh sách buổi học đã hoàn tất
            var closedAttendances = Class.Attendances.Where(a => a.Status.Equals(AttendanceStatusEnums.Closed.ToString())).ToList();
            var TotalAttendance = closedAttendances.Count;
            var TotalAttendanceDetails = closedAttendances.Sum(att => att.AttendanceDetails.Count);

            var activeScores = Class.Scores.Where(s => s.Status.Equals(GeneralStatusEnums.Active.ToString())).ToList();
            var averageScore = (TotalStudent > 0 && activeScores.Any())
                ? activeScores.Average(s => s.ScoreDetails.Any() ? s.ScoreDetails.Average(sd => sd.Score) : 0)
                : 0;

            var now = DateTime.Now;

            DateTime? nextSession = null;
            DateTime? nextSessionEndAt = null;
            string? nextSessionKind = null;

            var currentSession = Class.Attendances
                .Where(att => IsWindowOpen(att, now))
                .OrderBy(att => att.Date.ToDateTime(att.StartTime))
                .FirstOrDefault();

            if (currentSession != null)
            {
                nextSession = currentSession.Date.ToDateTime(currentSession.StartTime);
                nextSessionEndAt = currentSession.Date.ToDateTime(currentSession.EndTime);
                nextSessionKind = "Current";
            }
            else
            {
                var nextSessionCandidates = new List<(DateTime StartAt, DateTime EndAt)>();

                foreach (var schedule in Class.ClassSchedules.Where(cs => cs.Status.Equals(GeneralStatusEnums.Active.ToString())))
                {
                    var nextScheduledSession = GetNextSessionWindow(Class.StartAt, Class.EndAt, schedule.DayOfWeek, schedule.StartTime, schedule.EndTime);
                    if (nextScheduledSession.HasValue)
                    {
                        nextSessionCandidates.Add(nextScheduledSession.Value);
                    }
                }

                foreach (var attendance in Class.Attendances.Where(att => IsFutureWindow(att, now)))
                {
                    nextSessionCandidates.Add((attendance.Date.ToDateTime(attendance.StartTime), attendance.Date.ToDateTime(attendance.EndTime)));
                }

                if (nextSessionCandidates.Any())
                {
                    var nextSessionCandidate = nextSessionCandidates.OrderBy(candidate => candidate.StartAt).First();
                    nextSession = nextSessionCandidate.StartAt;
                    nextSessionEndAt = nextSessionCandidate.EndAt;
                    nextSessionKind = "Upcoming";
                }
            }

            var Metadata = new ClassDetailMetaData()
            {
                WeeklySchedules = _mapper.Map<List<ClassScheduleResModel>>(Class.ClassSchedules.Where(cs => cs.Status.Equals(GeneralStatusEnums.Active.ToString())).ToList()),
                TotalStudent = TotalStudent,
                TotalPendingStudent = TotalPendingStudent,
                AverageScore = averageScore,
                Name = Class.Name,

                // FIX: Tính tỉ lệ chuyên cần chính xác
                AttendanceRate = TotalAttendanceDetails > 0
                    ? ((decimal)closedAttendances.Sum(att => att.AttendanceDetails.Count(ad => ad.Status == AttendanceEnums.Present.ToString())) /
                    TotalAttendanceDetails) * 100
                    : 0,

                CompletedSessions = TotalAttendance,
                TotalSessions = Class.ClassSchedules
                    .Where(cs => cs.Status.Equals(GeneralStatusEnums.Active.ToString()))
                    .Sum(cs => CountWeekdayOccurrences(Class.StartAt.Date, Class.EndAt.Date, (DayOfWeek)cs.DayOfWeek))
                    + Class.Attendances.Count(att => IsAdditionalSessionType(att.SessionType)),
                StartAt = Class.StartAt,
                EndAt = Class.EndAt,
                NextSession = nextSession,
                NextSessionEndAt = nextSessionEndAt,
                NextSessionKind = nextSessionKind
            };

            return new DataResultModel<ClassDetailMetaData>()
            {
                Data = Metadata
            };
        }
        
        private static bool IsWindowOpen(Attendance attendance, DateTime now)
        {
            if (attendance == null)
            {
                return false;
            }

            var opensAt = attendance.Date.ToDateTime(attendance.StartTime);
            var closesAt = attendance.Date.ToDateTime(attendance.EndTime);

            return !attendance.Status.Equals(AttendanceStatusEnums.Closed.ToString()) &&
                   now >= opensAt && now <= closesAt;
        }

        private static bool IsFutureWindow(Attendance attendance, DateTime now)
        {
            if (attendance == null)
            {
                return false;
            }

            var opensAt = attendance.Date.ToDateTime(attendance.StartTime);
            return !attendance.Status.Equals(AttendanceStatusEnums.Closed.ToString()) && opensAt > now;
        }

        private (DateTime StartAt, DateTime EndAt)? GetNextSessionWindow(DateTime StartAt, DateTime EndAt, byte DayOfWeek, TimeOnly StartTime, TimeOnly EndTime)
        {
            var now = DateTime.Now;
            var nextSession = now.Date;

            while (nextSession.DayOfWeek != (DayOfWeek)DayOfWeek)
            {
                nextSession = nextSession.AddDays(1);
            }

            nextSession = new DateTime(nextSession.Year, nextSession.Month, nextSession.Day, StartTime.Hour, StartTime.Minute, 0);
            var nextSessionEndAt = new DateTime(nextSession.Year, nextSession.Month, nextSession.Day, EndTime.Hour, EndTime.Minute, 0);

            if (nextSession <= now)
            {
                nextSession = nextSession.AddDays(7);
                nextSessionEndAt = nextSessionEndAt.AddDays(7);
            }

            if (nextSession < StartAt || nextSession > EndAt)
            {
                return null;
            }

            return (nextSession, nextSessionEndAt);
        }

        private int CountWeekdayOccurrences(DateTime startDate, DateTime endDate, DayOfWeek targetDay)
        {
            if (startDate > endDate) return 0;

            // Tìm ngày đầu tiên >= startDate có DayOfWeek == targetDay
            int daysToAdd = ((int)targetDay - (int)startDate.DayOfWeek + 7) % 7;
            DateTime first = startDate.AddDays(daysToAdd);

            if (first > endDate) return 0;

            // Số buổi = 1 + số tuần chẵn giữa first và endDate
            return 1 + (int)((endDate.Date - first.Date).TotalDays / 7);
        }

        private bool IsAdditionalSessionType(string sessionType)
        {
            var normalizedSessionType = sessionType?.Replace(" ", string.Empty).Trim();
            return string.Equals(normalizedSessionType, "Adhoc", StringComparison.OrdinalIgnoreCase) || string.Equals(normalizedSessionType, "Makeup", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<ClassScoreStudentExport> GetStudentScoreByClassIdAndRangeDate(Guid ClassId, DateTime? FromDate, DateTime? ToDate)
        {
            // --- [LOGIC MỚI] XỬ LÝ KHOẢNG THỜI GIAN ---
            // 1. Nếu FromDate chưa nhập (là giá trị mặc định 0001-01-01) -> Lấy từ MinValue (Lấy tất cả quá khứ)
            // Ngược lại: Giữ nguyên FromDate
            var filterFrom = (FromDate == default(DateTime)) ? DateTime.MinValue : FromDate;

            // 2. Nếu ToDate chưa nhập (là giá trị mặc định 0001-01-01) -> Lấy đến MaxValue (Lấy tất cả tương lai)
            // Ngược lại: Giữ nguyên ToDate
            var filterTo = (ToDate == default(DateTime)) ? DateTime.MaxValue : ToDate;


            // --- 1. Lấy danh sách các BUỔI KIỂM TRA (Score) ---
            var scoreSessions = await _scoreRepositories.GetList(
                filter: s => s.ClassId == ClassId &&
                                s.TestDateAt >= filterFrom &&  // Dùng biến đã xử lý
                                s.TestDateAt <= filterTo,      // Dùng biến đã xử lý
                                                               // Include thêm Class để lấy tên lớp
                includeProperties: "ScoreDetails.StudentClass.Student,Class"
            );

            // --- 2. Xử lý danh sách học sinh (Giữ nguyên) ---
            var studentData = scoreSessions
                .SelectMany(s => s.ScoreDetails)
                .Where(sd => sd.StudentClass != null && sd.StudentClass.Student != null)
                .GroupBy(sd => sd.StudentClass.Student)
                .Select(g => new ScoreStudentResModel
                {
                    Id = g.Key.Id.ToString(),
                    Name = g.Key.Name ?? "No Name",
                    Scores = g.Select(sd => new ScoreStudentDetailResModel
                    {
                        TestDateAt = sd.ScoreNavigation.TestDateAt,
                        Shift = sd.ScoreNavigation.Shift,
                        Score = sd.Score,
                        Note = sd.Note
                    })
                    .OrderBy(x => x.TestDateAt)
                    .ToList()
                })
                .ToList();

            // --- 3. Lấy tên lớp (Fallback) ---
            string className = scoreSessions.FirstOrDefault()?.Class?.Name;

            if (string.IsNullOrEmpty(className))
            {
                var classObj = await _classRepositories.GetSingle(c => c.Id == ClassId);
                className = classObj?.Name ?? "Unknown Class";
            }

            // --- 4. Trả về ---
            return new ClassScoreStudentExport
            {
                Name = className,
                StudentData = studentData
            };
        }
        
        public async Task<PagedListDataResultModel<StudentInClassResModel>> GetStudentByClassId(Guid ClassId, int? pageIndex, UserFilter searchModel, bool isPending)
        {
            Func<IQueryable<StudentClass>, IOrderedQueryable<StudentClass>> orderBy = o => o.OrderBy(p => p.Student.Name);
            Expression<Func<StudentClass, bool>> filter = sc => sc.ClassId == ClassId;

            if (searchModel != null)
            {
                if (!string.IsNullOrEmpty(searchModel.Keyword))
                {
                    filter = filter.And(sc => sc.Student.Name.ToLower().Contains(searchModel.Keyword.ToLower()) || sc.Student.Email.ToLower().Contains(searchModel.Keyword.ToLower()) || sc.Student.Phone.ToLower().Contains(searchModel.Keyword.ToLower()) || sc.Student.StudentGuardianStudents.Any(sgs => sgs.Guardian.Name.ToLower().Contains(searchModel.Keyword.ToLower()) || sgs.Guardian.Email.ToLower().Contains(searchModel.Keyword.ToLower()) || sgs.Guardian.Phone.ToLower().Contains(searchModel.Keyword.ToLower())));
                }
            }

            if (isPending)
            {
                filter = filter.And(sc => sc.Student.Status.Equals(AccountStatusEnums.PendingApproved.ToString()));
            } else
            {
                filter = filter.And(sc => sc.Student.Status.Equals(GeneralStatusEnums.Active.ToString()));
            }

            var studentInClass = await _studentClassRepositories.GetPagedList(filter, orderBy, includeProperties: "Student.StudentGuardianStudents.Guardian", pageIndex ?? 1, 10);

            return new PagedListDataResultModel<StudentInClassResModel>()
            {
                Data = _mapper.Map<List<StudentInClassResModel>>(studentInClass.Data),
                PageNumber = pageIndex ?? 1,
                PageSize = 10,
                TotalPages = studentInClass.TotalPages
            };
        }

        
        public async Task<List<string>> GetCalendarIndicators(Guid classId, AttendanceFilter filter)
        {
            if (filter == null)
            {
                throw new CustomException("Filter is required");
            }

            if (classId == Guid.Empty)
            {
                throw new CustomException("ClassId is required");
            }

            var classEntity = await _classRepositories.GetSingle(
                x => x.Id.Equals(classId) && x.Status.Equals(GeneralStatusEnums.Active.ToString()),
                includeProperties: "ClassSchedules");

            if (classEntity == null)
            {
                throw new CustomException("Class not found or not active");
            }

            var classStart = DateOnly.FromDateTime(classEntity.StartAt);
            var classEnd = DateOnly.FromDateTime(classEntity.EndAt);

            var rangeStart = filter.FromDate.HasValue
                ? DateOnly.FromDateTime(filter.FromDate.Value)
                : classStart;

            var rangeEnd = filter.ToDate.HasValue
                ? DateOnly.FromDateTime(filter.ToDate.Value)
                : classEnd;

            var scheduleStart = rangeStart < classStart ? classStart : rangeStart;
            var scheduleEnd = rangeEnd > classEnd ? classEnd : rangeEnd;

            Expression<Func<Attendance, bool>> dateFilter = p =>
                p.ClassId.Equals(classId) &&
                p.ClassScheduleId == null;

            if (filter.FromDate.HasValue)
            {
                dateFilter = dateFilter.And(t => t.Date >= DateOnly.FromDateTime(filter.FromDate.Value));
            }

            if (filter.ToDate.HasValue)
            {
                dateFilter = dateFilter.And(t => t.Date <= DateOnly.FromDateTime(filter.ToDate.Value));
            }

            var attendanceList = await _attendanceRepositories.GetList(dateFilter, o => o.OrderBy(p => p.Date));
            var additionalAttendances = attendanceList
                .Where(att => IsAdditionalSessionType(att.SessionType));
            var calendarDates = new HashSet<DateOnly>();

            foreach (var attendance in additionalAttendances)
            {
                calendarDates.Add(attendance.Date);
            }

            if (scheduleStart <= scheduleEnd)
            {
                foreach (var schedule in classEntity.ClassSchedules.Where(cs => cs.Status.Equals(GeneralStatusEnums.Active.ToString())))
                {
                    var targetDay = (DayOfWeek)schedule.DayOfWeek;
                    var daysToAdd = ((int)targetDay - (int)scheduleStart.DayOfWeek + 7) % 7;
                    var current = scheduleStart.AddDays(daysToAdd);

                    while (current <= scheduleEnd)
                    {
                        calendarDates.Add(current);
                        current = current.AddDays(7);
                    }
                }
            }

            return calendarDates
                .OrderBy(date => date)
                .Select(date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                .ToList();
        }
    }
}
