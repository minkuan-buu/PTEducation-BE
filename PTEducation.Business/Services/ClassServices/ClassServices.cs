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
using PTEducation.Data.Repositories.ScoreDetailRepositories;
using PTEducation.Data.Repositories.ScoreRepositories;
using PTEducation.Data.Repositories.StudentClassRepositories;
using PTEducation.Data.Repositories.UserRepositories;
using System;
using System.Collections.Generic;
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
        private readonly IEmail _email;
        private readonly IMapper _mapper;
        public ClassServices(IClassRepositories classRepositories, IMapper mapper, IUserRepositories userRepositories, IStudentClassRepositories studentClassRepositories, IEmail email, IAttendanceDetailRepositories attendanceDetailRepositories, IAttendanceRepositories attendanceRepositories, IScoreDetailRepositories scoreDetailRepositories, IScoreRepositories scoreRepositories)
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
            var ListClass = await ViewAllClasses(pageIndex, 10, searchModel);
            return new PagedListDataResultModel<ListClassResModel>()
            {
                Data = _mapper.Map<List<ListClassResModel>>(ListClass.Data),
                PageNumber = pageIndex ?? 1,
                PageSize = 10,
                TotalPages = ListClass.TotalPages
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

        public async Task<MessageResultModel> CreateClass(ClassCreateReqModel ClassReq, string token)
        {
            var userId = Authentication.DecodeToken(token, "userid");
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
                NewUser.IsNeedResetPassoword = true;
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
                includeProperties: "StudentClasses.Student,Scores.ScoreDetails,Attendances.AttendanceDetails"
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

                // 2. Xóa AttendanceDetails
                var attendanceDetails = CheckExist.Attendances
                    .SelectMany(a => a.AttendanceDetails)
                    .ToList();
                _attendanceDetailRepositories.DeleteRange(attendanceDetails);

                // 3. Xóa Attendance
                _attendanceRepositories.DeleteRange(CheckExist.Attendances.ToList());

                // 4. Xóa ScoreDetails
                var scoreDetails = CheckExist.Scores
                    .SelectMany(s => s.ScoreDetails)
                    .ToList();
                _scoreDetailRepositories.DeleteRange(scoreDetails);

                // 5. Xóa Scores
                _scoreRepositories.DeleteRange(CheckExist.Scores.ToList());

                // 6. Xóa StudentClasses
                _studentClassRepositories.DeleteRange(CheckExist.StudentClasses.ToList());

                // 7. Xóa Users (Student)
                _userRepositories.DeleteRange(usersToDelete);

                // 8. Xóa Class
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
                    var GeneratePassword = AddStudentsReq.DefaultPassword ?? "Sinhhocvui@123";
                    CreateHashPasswordModel HashedPassword = Authentication.CreateHashPassword(GeneratePassword);
                    NewUser.Password = HashedPassword.HashedPassword;
                    NewUser.Salt = HashedPassword.Salt;
                    NewUser.Role = RoleEnums.Student.ToString();
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
                    filter = filter.And(p => p.Name.ToLower().Contains(TextConvert.ConvertToUnicodeEscape(searchModel.Keyword).ToLower()));
                }
            }

            var allClass = await _classRepositories.GetPagedList(filter, orderBy, includeProperties: "StudentClasses.Student,CreatedByNavigation", pageIndex ?? 1, pageSize ?? 10);

            return allClass;
        }
    }
}
