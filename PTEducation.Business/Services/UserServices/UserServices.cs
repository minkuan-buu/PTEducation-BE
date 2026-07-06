using AutoMapper;
using PTEducation.Business.ApplicationMiddleware;
using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Repositories.UserRepositories;
using PTEducation.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using PTEducation.Data.Enums;
using PTEducation.Business.Ultilities.Email;
using PTEducation.Data.Repositories.StudentClassRepositories;
using PTEducation.Business.Ultilities.FilterCombine;
using Org.BouncyCastle.Ocsp;
using PTEducation.Data.Repositories.AttendanceDetailRepositories;
using PTEducation.Data.Repositories.ScoreDetailRepositories;
using PTEducation.Data.Repositories.OTPRepositories;
using PTEducation.Data.Repositories.ClassRepositories;
using PTEducation.Data.Repositories.StudentGuardianRepositories;
using PTEducation.Business.Services.StorageServices;

namespace PTEducation.Business.Services.UserServices
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepositories _userRepositories;
        private readonly IStudentGuardianRepositories _studentGuardianRepositories;
        private readonly IClassRepositories _classRepositories;
        private readonly IStudentClassRepositories _studentClassRepositories;
        private readonly IOTPRepositories _otpRepositories;
        private readonly IAttendanceDetailRepositories _attendanceDetailRepositories;
        private readonly IScoreDetailRepositories _scoreDetailRepositories;
        private readonly IStorageServices _storageServices;
        private readonly IEmail _email;
        private readonly IMapper _mapper;
        private readonly string _domainFE = Environment.GetEnvironmentVariable("DOMAIN_FE") ?? throw new InvalidOperationException("DOMAIN_FE environment variable is not set.");
        public UserServices(IUserRepositories userRepositories, IMapper mapper, IEmail email, IStudentClassRepositories studentClassRepositories, IAttendanceDetailRepositories attendanceDetailRepositories, IScoreDetailRepositories scoreDetailRepositories, IOTPRepositories otpRepositories, IClassRepositories classRepositories, IStudentGuardianRepositories studentGuardianRepositories, IStorageServices storageServices)
        {
            _studentClassRepositories = studentClassRepositories;
            _studentGuardianRepositories = studentGuardianRepositories;
            _userRepositories = userRepositories;
            _email = email;
            _mapper = mapper;
            _classRepositories = classRepositories;
            _attendanceDetailRepositories = attendanceDetailRepositories;
            _scoreDetailRepositories = scoreDetailRepositories;
            _otpRepositories = otpRepositories;
            _storageServices = storageServices;
        }

        public async Task<DataResultModel<RawUserLoginResModel>> Login(string Username, string Password)
        {
            var CheckExist = await _userRepositories.GetSingle(x => (x.Email.Equals(Username) || x.Id.Equals(Username)) && x.Status.Equals(AccountStatusEnums.Active.ToString()));
            if (CheckExist == null)
            {
                throw new CustomException("Không tìm thấy tài khoản!");
            }
            if (!string.IsNullOrWhiteSpace(CheckExist.PasswordBcrypt))
            {
                var Auth = Authentication.VerifyPasswordBCrypt(Password, CheckExist.PasswordBcrypt);
                if (!Auth)
                {
                    throw new CustomException("Mật khẩu không chính xác!");
                }
            }
            // else
            // {
            //     var Auth = Authentication.VerifyPasswordHashed(Password, CheckExist.Salt, CheckExist.Password);
            //     if (!Auth)
            //     {
            //         throw new CustomException("Mật khẩu không chính xác!");
            //     }
            // }
            var User = _mapper.Map<RawUserLoginResModel>(CheckExist);
            User.Token = Authentication.GenerateJWT(CheckExist);
            User.EncryptedToken = SelfCrypto.Encrypt(User.Token);
            User.IsNeedChangePassword = CheckExist.IsNeedResetPassword;
            return new DataResultModel<RawUserLoginResModel>()
            {
                Data = User
            };
            // throw new NotImplementedException();
        }

        public async Task InitAdminIfNeeded()
        {
            var existingAdmin = await _userRepositories.GetSingle(x => x.Id.StartsWith("Admin-"));
            if (existingAdmin != null)
            {
                return;
            }

            var defaultPassword = Environment.GetEnvironmentVariable("ADMIN_DEFAULT_PASSWORD");
            if (string.IsNullOrWhiteSpace(defaultPassword))
            {
                throw new CustomException("Admin default password is not configured.");
            }

            var adminId = await GenerateUniqueAdminId();
            var admin = new User
            {
                Id = adminId,
                Name = Environment.GetEnvironmentVariable("ADMIN_NAME") ?? "Administrator",
                Email = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@pteducation.edu.vn",
                Phone = Environment.GetEnvironmentVariable("ADMIN_PHONE") ?? "0000000000",
                Role = RoleEnums.Admin.ToString(),
                Status = AccountStatusEnums.Active.ToString(),
                PasswordBcrypt = Authentication.CreateHashPasswordBCrypt(defaultPassword),
                IsNeedResetPassword = true
            };

            await _userRepositories.Insert(admin);
        }

        private async Task<string> GenerateUniqueAdminId()
        {
            var random = new Random();
            string adminId;
            do
            {
                adminId = $"Admin-{random.Next(100000, 999999)}";
            } while (await _userRepositories.GetSingle(x => x.Id == adminId) != null);

            return adminId;
        }

        public async Task<MessageResultModel> Register(UserRegisterReqModel ReqModel)
        {
            // var CheckExist = await _userRepositories.GetSingle(x => x.Email == ReqModel.Email || x.Id == ReqModel.Id);
            // if (CheckExist != null)
            // {
            //     throw new CustomException("Tài khoản với Email hoặc Id này đã tồn tại!");
            // }
            // var NewUser = _mapper.Map<User>(ReqModel);
            // if (ReqModel.Id == null)
            // {
            //     Random rnd = new Random();
            //     NewUser.Id = $"{ReqModel.Role}-{rnd.Next(100000, 999999)}";
            // }
            // var GeneratePassword = Authentication.GenerateRandomPassword();
            // CreateHashPasswordModel HashedPassword = Authentication.CreateHashPassword(GeneratePassword);
            // NewUser.Status = AccountStatusEnums.Active.ToString();
            // NewUser.Password = HashedPassword.HashedPassword;
            // NewUser.Salt = HashedPassword.Salt;
            // await _userRepositories.Insert(NewUser);
            // string FilePath = "../PTEducation.Business/TemplateEmail/FirstInformation.html";
            // string Html = File.ReadAllText(FilePath);
            // Html = Html.Replace("{{Password}}", GeneratePassword);
            // Html = Html.Replace("{{Email}}", ReqModel.Email);
            // var listEmail = new List<EmailReqModel>
            // {
            //     new EmailReqModel
            //     {
            //         Email = ReqModel.Email,
            //         HtmlContent = Html
            //     }
            // };
            // await _email.SendEmail("[Thông tin đăng nhập]", listEmail);
            // return new MessageResultModel
            // {
            //     Message = "Ok"
            // };
            throw new NotImplementedException();
        }

        public async Task<MessageResultModel> Register(UserRegisterWithGuardianInfo ReqModel)
        {
            var CheckExist = await _userRepositories.GetSingle(x => x.Email == ReqModel.Email);
            var GetClass = await _classRepositories.GetSingle(x => x.Id == ReqModel.ClassId);
            if (CheckExist != null)
            {
                throw new CustomException("Tài khoản với Email này đã tồn tại!");
            }
            if (GetClass == null)
            {
                throw new CustomException("Lớp học không tồn tại!");
            }
            var className = TextConvert.ConvertFromUnicodeEscape(GetClass.Name).Trim();
            var classBlockMatch = System.Text.RegularExpressions.Regex.Match(className, @"^\d+");
            var classBlock = classBlockMatch.Success ? classBlockMatch.Value : className;
            if (!int.TryParse(classBlock, out var classBlockNumber))
            {
                throw new CustomException("Không thể tạo mã định danh vì tên lớp không hợp lệ.");
            }
            var classBlockCode = classBlockNumber.ToString("D2");

            var usersInClassBlock = await _userRepositories.GetList(x =>
                x.Id.StartsWith($"1{classBlockCode}") || x.Id.StartsWith($"2{classBlockCode}"));

            var maxStudentSequence = usersInClassBlock
                .Where(x => x.Id.StartsWith($"1{classBlockCode}") && x.Id.Length == 6)
                .Select(x => int.TryParse(x.Id.Substring(3, 3), out var seq) ? seq : 0)
                .DefaultIfEmpty(0)
                .Max();
            var nextStudentSequence = maxStudentSequence + 1;
            if (nextStudentSequence > 999)
            {
                throw new CustomException($"Đã vượt quá giới hạn mã học sinh cho khối {classBlockCode}.");
            }

            var maxGuardianSequence = usersInClassBlock
                .Where(x => x.Id.StartsWith($"2{classBlockCode}") && x.Id.Length == 7)
                .Select(x => int.TryParse(x.Id.Substring(3, 4), out var seq) ? seq : 0)
                .DefaultIfEmpty(0)
                .Max();
            var nextGuardianSequence = maxGuardianSequence + 1;

            List<User> ListAddUser = new List<User>();

            var NewStudent = new User
            {
                Id = $"1{classBlockCode}{nextStudentSequence:000}",
                Name = ReqModel.Name,
                Email = ReqModel.Email,
                Phone = ReqModel.Phone ?? "",
                Role = RoleEnums.Student.ToString(),
                SchoolInfo = ReqModel.School,
                IsNeedResetPassword = true,
            };

            StudentClass NewStudentClass = new()
            {
                Id = Guid.NewGuid(),
                ClassId = ReqModel.ClassId,
                StudentId = NewStudent.Id,
                Status = AccountStatusEnums.PendingApproved.ToString(),
            };

            var GeneratePassword = Authentication.GenerateRandomPassword();
            string HashedPassword = Authentication.CreateHashPasswordBCrypt(GeneratePassword);
            NewStudent.PasswordBcrypt = HashedPassword;
            NewStudent.Status = AccountStatusEnums.PendingApproved.ToString();
            ListAddUser.Add(NewStudent);
            List<StudentGuardian> ListStudentGuardian = new List<StudentGuardian>();
            var listEmail = new List<EmailReqModel>();
            string FileGuardianPath = "../PTEducation.Business/TemplateEmail/FirstInformationNewGuardian.html";
            foreach (var guardian in ReqModel.Guardians)
            {
                if (nextGuardianSequence > 9999)
                {
                    throw new CustomException($"Đã vượt quá giới hạn mã giám hộ cho khối {classBlockCode}.");
                }
                var NewGeneratePassword = Authentication.GenerateRandomPassword();

                var NewGuardian = new User
                {
                    Id = $"2{classBlockCode}{nextGuardianSequence:0000}",
                    Name = guardian.Name,
                    Email = guardian.Email,
                    Phone = guardian.Phone ?? "",
                    Role = RoleEnums.Guardian.ToString(),
                    Status = AccountStatusEnums.PendingApproved.ToString(),
                    IsNeedResetPassword = true,
                    PasswordBcrypt = Authentication.CreateHashPasswordBCrypt(NewGeneratePassword)
                };
                ListAddUser.Add(NewGuardian);
                nextGuardianSequence++;
                string GuardHtml = File.ReadAllText(FileGuardianPath);
                GuardHtml = GuardHtml.Replace("{{PASSWORD}}", NewGeneratePassword);
                GuardHtml = GuardHtml.Replace("{{CLASSNAME}}", className);
                GuardHtml = GuardHtml.Replace("{{STUDENTNAME}}", ReqModel.Name);
                GuardHtml = GuardHtml.Replace("{{GUARDIANNAME}}", guardian.Name);
                GuardHtml = GuardHtml.Replace("{{USERNAME}}", guardian.Email);
                GuardHtml = GuardHtml.Replace("{{DOMAIN_FE}}", _domainFE);
                listEmail.Add(
                    new EmailReqModel
                    {
                        Email = guardian.Email,
                        HtmlContent = GuardHtml
                    }
                );

                var NewStudentGuardian = new StudentGuardian
                {
                    Id = Guid.NewGuid(),
                    StudentId = NewStudent.Id,
                    GuardianId = NewGuardian.Id,
                    Guardian = NewGuardian,
                    IsPrimary = guardian.IsPrimary,
                    Relationship = guardian.Relationship
                };
                ListStudentGuardian.Add(NewStudentGuardian);
            }
            await _userRepositories.InsertRange(ListAddUser);
            await _studentGuardianRepositories.InsertRange(ListStudentGuardian);
            await _studentClassRepositories.Insert(NewStudentClass);
            string FilePath = "../PTEducation.Business/TemplateEmail/FirstInformationNew.html";
            string Html = File.ReadAllText(FilePath);
            Html = Html.Replace("{{CLASSNAME}}", className);
            Html = Html.Replace("{{PASSWORD}}", GeneratePassword);
            Html = Html.Replace("{{STUDENTNAME}}", ReqModel.Name);
            Html = Html.Replace("{{USERNAME}}", ReqModel.Email);
            Html = Html.Replace("{{DOMAIN_FE}}", _domainFE);
            listEmail.Add(
                new EmailReqModel
                {
                    Email = ReqModel.Email,
                    HtmlContent = Html
                }
            );
            await _email.SendEmail("[Thông tin đăng nhập]", listEmail);
            return new MessageResultModel
            {
                Message = "Ok"
            };
        }

        public async Task<MessageResultModel> ChangePassword(UserChangePasswordReqModel ReqModel, string userId)
        {
            var user = await _userRepositories.GetSingle(x => x.Id == userId);
            if (user == null)
            {
                throw new CustomException("Không tìm thấy người dùng!");
            }
            if (user.PasswordBcrypt == null)
            {
                throw new CustomException("Đã có lỗi xảy ra, vui lòng liên hệ quản trị viên!");
            }
            if (ReqModel.NewPassword != ReqModel.ConfirmPassword)
            {
                throw new CustomException("Mật khẩu mới và xác nhận mật khẩu không khớp!");
            }
            if (ReqModel.OldPassword == ReqModel.NewPassword)
            {
                throw new CustomException("Mật khẩu mới giống với mật khẩu cũ!");
            }
            if (ReqModel.NewPassword.Length < 6)
            {
                throw new CustomException("Mật khẩu phải có ít nhất 6 ký tự!");
            }
            var Auth = Authentication.VerifyPasswordBCrypt(ReqModel.OldPassword, user.PasswordBcrypt);
            if (!Auth)
            {
                throw new CustomException("Mật khẩu cũ không chính xác!");
            }
            var NewPassword = Authentication.CreateHashPasswordBCrypt(ReqModel.NewPassword);
            user.PasswordBcrypt = NewPassword;
            user.IsNeedResetPassword = false;
            await _userRepositories.Update(user);
            return new MessageResultModel
            {
                Message = "Ok"
            };
        }

        public async Task<DataResultModel<UserProfileResModel>> GetMyProfile(string userId)
        {
            var user = await _userRepositories.GetSingle(x => x.Id == userId && x.Status.Equals(GeneralStatusEnums.Active.ToString()), includeProperties: "StudentClasses.Class");
            if (user == null)
            {
                throw new CustomException("Không tìm thấy người dùng!");
            }
            
            var Result = _mapper.Map<UserProfileResModel>(user);
            Result.Role = user.Role;
            Result.SchoolInfo = user.SchoolInfo;
            Result.Status = user.Status;

            if (user.Role.Equals(RoleEnums.Student.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var studentGuardians = await _studentGuardianRepositories.GetList(
                    x => x.StudentId == userId,
                    includeProperties: "Guardian"
                );
                Result.Guardians = studentGuardians.Select(sg => new UserGuardianListResModel
                {
                    Id = sg.Guardian.Id,
                    Name = TextConvert.ConvertFromUnicodeEscape(sg.Guardian.Name),
                    Email = sg.Guardian.Email,
                    Phone = sg.Guardian.Phone,
                    Relationship = TextConvert.ConvertFromUnicodeEscape(sg.Relationship),
                    IsPrimary = sg.IsPrimary
                }).ToList();
            }
            else if (user.Role.Equals(RoleEnums.Guardian.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var relationships = await _studentGuardianRepositories.GetList(
                    x => x.GuardianId == userId,
                    includeProperties: "Student"
                );

                Result.GuardianProfile = new GuardianProfileDto();
                foreach (var rel in relationships)
                {
                    Result.GuardianProfile.ManagedStudents.Add(new GuardianStudentDto
                    {
                        Id = rel.Student.Id,
                        Name = TextConvert.ConvertFromUnicodeEscape(rel.Student.Name),
                        Email = rel.Student.Email,
                        Phone = rel.Student.Phone,
                        AvatarUrl = rel.Student.AvatarUrl,
                        SchoolInfo = rel.Student.SchoolInfo,
                        Relationship = TextConvert.ConvertFromUnicodeEscape(rel.Relationship),
                        IsPrimary = rel.IsPrimary
                    });
                }
            }
            else if (user.Role.Equals(RoleEnums.Admin.ToString(), StringComparison.OrdinalIgnoreCase) || 
                     user.Role.Equals(RoleEnums.Manager.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var students = await _userRepositories.GetList(x => x.Role == RoleEnums.Student.ToString() && x.Status == AccountStatusEnums.Active.ToString());
                var guardians = await _userRepositories.GetList(x => x.Role == RoleEnums.Guardian.ToString() && x.Status == AccountStatusEnums.Active.ToString());
                var managers = await _userRepositories.GetList(x => (x.Role == RoleEnums.Manager.ToString() || x.Role == RoleEnums.Admin.ToString()) && x.Status == AccountStatusEnums.Active.ToString());
                var classes = await _classRepositories.GetList(x => x.Status == GeneralStatusEnums.Active.ToString());

                Result.AdminProfile = new AdminProfileDto
                {
                    TotalStudentsCount = students.Count(),
                    TotalGuardiansCount = guardians.Count(),
                    TotalManagersCount = managers.Count(),
                    TotalClassesCount = classes.Count(),
                    ActiveClassesCount = classes.Count()
                };
            }

            return new DataResultModel<UserProfileResModel>
            {
                Data = Result
            };
        }



        public async Task<MessageResultModel> ResetPassword(UserResetPasswordReqModel ReqModel, string token)
        {
            try
            {
                var email = Authentication.DecodeToken(token, "email");
                var user = await _userRepositories.GetSingle(x => x.Email.Equals(email));
                if (user == null)
                {
                    throw new CustomException("Không tìm thấy người dùng!");
                }
                if (ReqModel.NewPassword != ReqModel.ConfirmPassword)
                {
                    throw new CustomException("Mật khẩu mới và xác nhận mật khẩu không khớp!");
                }
                if (ReqModel.NewPassword.Length < 6)
                {
                    throw new CustomException("Mật khẩu phải có ít nhất 6 ký tự!");
                }
                var NewPassword = Authentication.CreateHashPasswordBCrypt(ReqModel.NewPassword);
                user.PasswordBcrypt = NewPassword;
                user.IsNeedResetPassword = false;
                user.Status = GeneralStatusEnums.Active.ToString();
                await _userRepositories.Update(user);
                return new MessageResultModel
                {
                    Message = "Ok"
                };
            }
            catch (Exception ex)
            {
                throw new CustomException("Error: " + ex.Message);
            }
        }
        //public async Task<bool> SendMail()
        //{
        //    var check = await _email.SendEmail();
        //    return check;
        //}

        public async Task<MessageResultModel> Register(List<ManagerRegisterReqModel> ReqModel)
        {
            var listEmail = new List<EmailReqModel>();
            var listUser = new List<User>();
            foreach (var item in ReqModel)
            {
                var CheckExist = await _userRepositories.GetSingle(x => x.Email == item.Email);
                if (CheckExist != null)
                {
                    throw new CustomException("Tài khoản với Email này đã tồn tại!");
                }
                var NewUser = _mapper.Map<User>(item);
                Random rnd = new Random();
                NewUser.Id = $"Manager-{rnd.Next(100000, 999999)}";
                var GeneratePassword = Environment.GetEnvironmentVariable("MANAGER_DEFAULT_PASSWORD") ?? throw new CustomException("Default manager password is not configured in the system. Please contact the administrator.");
                var NewPassword = Authentication.CreateHashPasswordBCrypt(GeneratePassword);
                NewUser.Status = AccountStatusEnums.Active.ToString();
                NewUser.PasswordBcrypt = NewPassword;
                listUser.Add(NewUser);
                string FilePath = "../PTEducation.Business/TemplateEmail/ManagerInformationNew.html";
                string Html = File.ReadAllText(FilePath);
                Html = Html.Replace("{{MANAGERNAME}}", item.Name);
                Html = Html.Replace("{{PASSWORD}}", GeneratePassword);
                Html = Html.Replace("{{USERNAME}}", item.Email);
                Html = Html.Replace("{{DOMAIN_FE}}", _domainFE);
                listEmail.Add(new EmailReqModel
                {
                    Email = item.Email,
                    HtmlContent = Html
                });
            }
            await _userRepositories.InsertRange(listUser);

            await _email.SendEmail("[Thông tin đăng nhập]", listEmail);

            return new MessageResultModel
            {
                Message = "Ok"
            };
        }

        public async Task<PagedListDataResultModel<ManagerResModel>> GetManagers(int? pageIndex, UserFilter searchModel, string userId)
        {
            var ListManager = await ViewAllManagers(pageIndex, 10, searchModel, userId);
            return new PagedListDataResultModel<ManagerResModel>()
            {
                Data = _mapper.Map<List<ManagerResModel>>(ListManager.Data),
                PageNumber = pageIndex ?? 1,
                PageSize = 10,
                TotalPages = ListManager.TotalPages
            };
        }

        private async Task<PagedListDataResultModel<User>> ViewAllManagers(int? pageIndex, int? pageSize, UserFilter searchModel, string userId)
        {
            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = o => o.OrderBy(p => p.Name);
            Expression<Func<User, bool>> filter = p => true;

            if (searchModel != null)
            {
                if (!string.IsNullOrEmpty(searchModel.Keyword))
                {
                    var keyword = searchModel.Keyword.ToLower();
                    var unicodeKeyword = TextConvert.ConvertToUnicodeEscape(searchModel.Keyword).ToLower();
                    filter = filter.And(p =>
                        p.Name.ToLower().Contains(unicodeKeyword) ||
                        p.Email.ToLower().StartsWith(keyword)
                    );
                }
            }

            filter = filter.And(p => p.Role.Equals("Manager")).And(p => p.Id != userId);

            var allManagers = await _userRepositories.GetPagedList(filter, orderBy, string.Empty, pageIndex ?? 1, pageSize ?? 10);

            return allManagers;
        }

        public async Task<MessageResultModel> Deactivate(string userId)
        {
            var user = await _userRepositories.GetSingle(x => x.Id == userId && x.Status.Equals(GeneralStatusEnums.Active.ToString()));
            if (user == null)
            {
                throw new CustomException("Không tìm thấy người dùng!");
            }
            user.Status = GeneralStatusEnums.Inactive.ToString();
            await _userRepositories.Update(user);
            return new MessageResultModel
            {
                Message = "Ok"
            };
        }

        public async Task<MessageResultModel> ReActivate(string userId)
        {
            var user = await _userRepositories.GetSingle(x => x.Id == userId && x.Status.Equals(GeneralStatusEnums.Inactive.ToString()));
            if (user == null)
            {
                throw new CustomException("Không tìm thấy người dùng!");
            }
            user.Status = GeneralStatusEnums.Active.ToString();
            await _userRepositories.Update(user);
            return new MessageResultModel
            {
                Message = "Ok"
            };
        }

        public async Task<MessageResultModel> UpdateStudentInfo(StudentUpdateReqModel ReqModel, Guid StudentClassId)
        {
            var StudentClass = await _studentClassRepositories.GetSingle(x => x.Id == StudentClassId, includeProperties: "Student");
            if (StudentClass == null || StudentClass.Student == null)
            {
                throw new CustomException("Không tìm thấy học sinh!");
            }

            var CheckEmailExist = await _userRepositories.GetOtherUserByEmail(ReqModel.Email, StudentClass.Student.Id);
            if (CheckEmailExist != null)
            {
                throw new CustomException("Email đã tồn tại!");
            }

            StudentClass.Student.Name = ReqModel.Name;
            StudentClass.Student.Email = ReqModel.Email;
            StudentClass.Student.Phone = ReqModel.Phone;
            var GeneratePassword = ReqModel.DefaultPassword ?? Environment.GetEnvironmentVariable("STUDENT_DEFAULT_PASSWORD") ?? throw new CustomException("Default student password is not configured in the system. Please contact the administrator.");
            var HashedPassword = Authentication.CreateHashPasswordBCrypt(GeneratePassword);
            StudentClass.Student.PasswordBcrypt = HashedPassword;
            StudentClass.Student.IsNeedResetPassword = true;

            await _studentClassRepositories.Update(StudentClass);
            if (ReqModel.IsResendInfo)
            {
                string FilePath = "../PTEducation.Business/TemplateEmail/FirstInformation.html";
                string Html = File.ReadAllText(FilePath);
                Html = Html.Replace("{{ID}}", StudentClass.Student.Id);
                Html = Html.Replace("{{Password}}", ReqModel.DefaultPassword ?? Environment.GetEnvironmentVariable("STUDENT_DEFAULT_PASSWORD"));
                Html = Html.Replace("{{Email}}", ReqModel.Email);
                var listEmail = new List<EmailReqModel>
                {
                    new EmailReqModel
                    {
                        Email = ReqModel.Email,
                        HtmlContent = Html
                    }
                };
                await _email.SendEmail("[Thông tin đăng nhập]", listEmail);
            }
            return new MessageResultModel
            {
                Message = "Ok"
            };
        }

        public async Task<MessageResultModel> DeleteStudent(Guid StudentClassId)
        {
            var StudentClass = await _studentClassRepositories.GetSingle(x => x.Id == StudentClassId, includeProperties: "AttendanceDetails,ScoreDetails,Student.Otps");
            if (StudentClass == null)
            {
                throw new CustomException("Không tìm thấy học sinh!");
            }
            using var transaction = await _userRepositories.BeginTransactionAsync();
            try
            {
                await _attendanceDetailRepositories.DeleteRange(StudentClass.AttendanceDetails.ToList());
                await _scoreDetailRepositories.DeleteRange(StudentClass.ScoreDetails.ToList());
                await _studentClassRepositories.Delete(StudentClass);
                await _otpRepositories.DeleteRange(StudentClass.Student.Otps.ToList());
                await _userRepositories.Delete(StudentClass.Student);

                await _userRepositories.SaveChangesAsync(); // commit dữ liệu vào DB
                await _userRepositories.CommitTransactionAsync();

                return new MessageResultModel
                {
                    Message = "Ok"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<MessageResultModel> ConvertNameFromUnicodeEscapeToUnicode()
        {
            try
            {
                var users = await _userRepositories.GetList();
                foreach (var u in users)
                {
                    u.Name = TextConvert.ConvertFromUnicodeEscape(u.Name);
                }
                await _userRepositories.UpdateRange(users.ToList());
                return new MessageResultModel
                {
                    Message = "Ok"
                };
            }
            catch
            {
                throw new CustomException("Error");
            }

        }

        public async Task<PagedListDataResultModel<UserListResModel>> GetAllStudents(int? pageIndex, UserFilter searchModel)
        {
            var ListStudent = await ViewAllStudents(pageIndex, 20, searchModel);
            return new PagedListDataResultModel<UserListResModel>()
            {
                Data = _mapper.Map<List<UserListResModel>>(ListStudent.Data),
                PageNumber = pageIndex ?? 1,
                PageSize = 20,
                TotalPages = ListStudent.TotalPages
            };
        }

        private async Task<PagedListDataResultModel<User>> ViewAllStudents(int? pageIndex, int? pageSize, UserFilter searchModel)
        {
            Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = o => o.OrderBy(p => p.Name);
            Expression<Func<User, bool>> filter = p => true;

            if (searchModel != null)
            {
                if (!string.IsNullOrEmpty(searchModel.Keyword))
                {
                    var keyword = searchModel.Keyword.ToLower();
                    var unicodeKeyword = TextConvert.ConvertToUnicodeEscape(searchModel.Keyword).ToLower();
                    filter = filter.And(p =>
                        p.Name.ToLower().Contains(unicodeKeyword) ||
                        p.Email.ToLower().StartsWith(keyword)
                    );
                }
            }

            filter = filter.And(p => p.Role.Equals(RoleEnums.Student.ToString()));

            var allStudents = await _userRepositories.GetPagedList(filter, orderBy, "StudentGuardianStudents.Guardian,StudentClasses.Class", pageIndex ?? 1, pageSize ?? 10);

            return allStudents;
        }

        public async Task<MessageResultModel> UpdateStudentAccess(string userId, AccessReqModel reqModel)
        {
            var user = await _userRepositories.GetSingle(x => x.Id == userId && x.Role.Equals(RoleEnums.Student.ToString()));
            if (user == null)
            {
                throw new CustomException("Không tìm thấy học sinh!");
            }
            if (reqModel.AccessStatus != "Approved" && reqModel.AccessStatus != "Rejected")
            {
                throw new CustomException("Trạng thái truy cập không hợp lệ!");
            }
            var studentClasses = (await _studentClassRepositories.GetList(
                x => x.StudentId == userId,
                includeProperties: "AttendanceDetails,ScoreDetails")).ToList();

            var studentGuardians = (await _studentGuardianRepositories.GetList(
                x => x.StudentId == userId,
                includeProperties: "Guardian")).ToList();

            var guardians = studentGuardians
                .Where(x => x.Guardian != null)
                .Select(x => x.Guardian)
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToList();

            using var transaction = await _userRepositories.BeginTransactionAsync();
            try
            {
                if (reqModel.AccessStatus == "Approved")
                {
                    user.Status = AccountStatusEnums.Active.ToString();

                    foreach (var studentClass in studentClasses)
                    {
                        studentClass.Status = AccountStatusEnums.Active.ToString();
                    }

                    foreach (var guardian in guardians)
                    {
                        guardian.Status = AccountStatusEnums.Active.ToString();
                    }

                    await _userRepositories.Update(user, saveChanges: false);

                    if (studentClasses.Count > 0)
                    {
                        await _studentClassRepositories.UpdateRange(studentClasses, saveChanges: false);
                    }

                    if (guardians.Count > 0)
                    {
                        await _userRepositories.UpdateRange(guardians, saveChanges: false);
                    }

                    await _userRepositories.SaveChangesAsync();
                    await _userRepositories.CommitTransactionAsync();

                    return new MessageResultModel
                    {
                        Message = "Ok"
                    };
                }

                var attendanceDetails = studentClasses.SelectMany(x => x.AttendanceDetails).ToList();
                var scoreDetails = studentClasses.SelectMany(x => x.ScoreDetails).ToList();

                if (attendanceDetails.Count > 0)
                {
                    await _attendanceDetailRepositories.DeleteRange(attendanceDetails, saveChanges: false);
                }

                if (scoreDetails.Count > 0)
                {
                    await _scoreDetailRepositories.DeleteRange(scoreDetails, saveChanges: false);
                }

                if (studentGuardians.Count > 0)
                {
                    await _studentGuardianRepositories.DeleteRange(studentGuardians, saveChanges: false);
                }

                var studentOtps = (await _otpRepositories.GetList(x => x.UserId == userId)).ToList();
                if (studentOtps.Count > 0)
                {
                    await _otpRepositories.DeleteRange(studentOtps, saveChanges: false);
                }

                if (guardians.Count > 0)
                {
                    var guardianIds = guardians.Select(x => x.Id).ToList();
                    var guardianOtps = (await _otpRepositories.GetList(x => guardianIds.Contains(x.UserId))).ToList();
                    if (guardianOtps.Count > 0)
                    {
                        await _otpRepositories.DeleteRange(guardianOtps, saveChanges: false);
                    }
                }

                if (studentClasses.Count > 0)
                {
                    await _studentClassRepositories.DeleteRange(studentClasses, saveChanges: false);
                }

                if (guardians.Count > 0)
                {
                    await _userRepositories.DeleteRange(guardians, saveChanges: false);
                }

                await _userRepositories.Delete(user, saveChanges: false);
                await _userRepositories.SaveChangesAsync();
                await _userRepositories.CommitTransactionAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return new MessageResultModel
            {
                Message = "Ok"
            };
        }

        public async Task<MessageResultModel> DeleteStudent(string userId)
        {
            var user = await _userRepositories.GetSingle(x => x.Id == userId && x.Role.Equals(RoleEnums.Student.ToString()));
            if (user == null)
            {
                throw new CustomException("Không tìm thấy học sinh!");
            }

            var studentClasses = (await _studentClassRepositories.GetList(
                x => x.StudentId == userId,
                includeProperties: "AttendanceDetails,ScoreDetails")).ToList();

            var studentGuardians = (await _studentGuardianRepositories.GetList(
                x => x.StudentId == userId,
                includeProperties: "Guardian")).ToList();

            var guardians = studentGuardians
                .Where(x => x.Guardian != null)
                .Select(x => x.Guardian)
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToList();

            using var transaction = await _userRepositories.BeginTransactionAsync();
            try
            {
                var attendanceDetails = studentClasses.SelectMany(x => x.AttendanceDetails).ToList();
                var scoreDetails = studentClasses.SelectMany(x => x.ScoreDetails).ToList();

                if (attendanceDetails.Count > 0)
                {
                    await _attendanceDetailRepositories.DeleteRange(attendanceDetails, saveChanges: false);
                }

                if (scoreDetails.Count > 0)
                {
                    await _scoreDetailRepositories.DeleteRange(scoreDetails, saveChanges: false);
                }

                if (studentGuardians.Count > 0)
                {
                    await _studentGuardianRepositories.DeleteRange(studentGuardians, saveChanges: false);
                }

                var studentOtps = (await _otpRepositories.GetList(x => x.UserId == userId)).ToList();
                if (studentOtps.Count > 0)
                {
                    await _otpRepositories.DeleteRange(studentOtps, saveChanges: false);
                }

                if (guardians.Count > 0)
                {
                    var guardianIds = guardians.Select(x => x.Id).ToList();
                    var guardianOtps = (await _otpRepositories.GetList(x => guardianIds.Contains(x.UserId))).ToList();
                    if (guardianOtps.Count > 0)
                    {
                        await _otpRepositories.DeleteRange(guardianOtps, saveChanges: false);
                    }
                }

                if (studentClasses.Count > 0)
                {
                    await _studentClassRepositories.DeleteRange(studentClasses, saveChanges: false);
                }

                if (guardians.Count > 0)
                {
                    await _userRepositories.DeleteRange(guardians, saveChanges: false);
                }
                await _userRepositories.Delete(user, saveChanges: false);
                await _userRepositories.SaveChangesAsync();
                await _userRepositories.CommitTransactionAsync();

                return new MessageResultModel
                {
                    Message = "Ok"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<DataResultModel<UserEditResModel>> GetUserDetail(string userId)
        {
            var GetUser = await _userRepositories.GetSingle(x => x.Id == userId, includeProperties: "StudentGuardianStudents.Guardian");
            if (GetUser == null)
            {
                throw new CustomException("Không tìm thấy thông tin người dùng!");
            }
            var result = new UserEditResModel
            {
                Name = TextConvert.ConvertFromUnicodeEscape(GetUser.Name),
                Email = GetUser.Email,
                Phone = GetUser.Phone,
                SchoolInfo = GetUser.SchoolInfo ?? string.Empty,
                AvatarUrl = GetUser.AvatarUrl,
                Role = GetUser.Role,
                Guardians = GetUser.StudentGuardianStudents.Select(x => new UserGuardianListResModel
                {
                    Id = x.GuardianId,
                    Name = TextConvert.ConvertFromUnicodeEscape(x.Guardian.Name),
                    Email = x.Guardian.Email,
                    Phone = x.Guardian.Phone,
                    IsPrimary = x.IsPrimary,
                    Relationship = x.Relationship
                }).ToList()
            };
            return new DataResultModel<UserEditResModel>
            {
                Data = result
            };
        }

        public async Task<MessageResultModel> UploadAvatar(string userId, AttachmentReqModel reqModel)
        {
            try
            {
                if (reqModel.File == null || reqModel.File.Length == 0)
                    throw new CustomException("Không có file đính kèm!");
                var user = await _userRepositories.GetSingle(x => x.Id == userId);
                if (user == null)
                {
                    throw new CustomException("Không tìm thấy thông tin người dùng!");
                }
                var Url = await _storageServices.UploadFileAsync(reqModel.File.OpenReadStream(), Guid.NewGuid().ToString(), reqModel.File.ContentType, "users/avatars");
                
                if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
                {
                    try
                    {
                        string oldKey = user.AvatarUrl;
                        if (oldKey.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                            oldKey.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            var uri = new Uri(oldKey);
                            oldKey = uri.AbsolutePath.TrimStart('/');
                        }
                        await _storageServices.DeleteFileAsync(oldKey);
                    }
                    catch (Exception ex)
                    {
                        // Don't block upload if old file deletion fails
                        Console.WriteLine($"Error deleting old avatar file: {ex.Message}");
                    }
                }

                user.AvatarUrl = Url;
                await _userRepositories.Update(user, saveChanges: false);
                await _userRepositories.SaveChangesAsync();
                return new MessageResultModel
                {
                    Message = "Ok"
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public async Task<MessageResultModel> ResetPassword(string userId, UserResetPassword reqModel)
        {
            try
            {
                var user = await _userRepositories.GetSingle(x => x.Id == userId);
                if (user == null)
                {
                    throw new CustomException("Không tìm thấy thông tin người dùng!");
                }
                var GeneratePassword = reqModel.Password ?? Authentication.GenerateRandomPassword();
                string HashedPassword = Authentication.CreateHashPasswordBCrypt(GeneratePassword);
                user.PasswordBcrypt = HashedPassword;
                user.IsNeedResetPassword = true;
                await _userRepositories.Update(user, saveChanges: false);
                await _userRepositories.SaveChangesAsync();

                string fileResetPath = "../PTEducation.Business/TemplateEmail/PasswordResetAdminNotification.html";
                if (File.Exists(fileResetPath))
                {
                    string htmlReset = File.ReadAllText(fileResetPath);
                    htmlReset = htmlReset.Replace("{{NAME}}", user.Name);
                    htmlReset = htmlReset.Replace("{{USERNAME}}", user.Email);
                    htmlReset = htmlReset.Replace("{{PASSWORD}}", GeneratePassword);
                    htmlReset = htmlReset.Replace("{{DOMAIN_FE}}", _domainFE);

                    var listEmail = new List<EmailReqModel>
                    {
                        new EmailReqModel { Email = user.Email, HtmlContent = htmlReset }
                    };
                    await _email.SendEmail("Mật khẩu của bạn đã được đặt lại", listEmail);
                }
                
                return new MessageResultModel
                {
                    Message = "Đặt lại mật khẩu thành công"
                };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message);
            }
        }

        public async Task<MessageResultModel> UpdateUserDetail(string userId, UserEditResModel payload)
        {
            var user = await _userRepositories.GetSingle(x => x.Id == userId);
            if (user == null)
            {
                throw new CustomException("Không tìm thấy người dùng!");
            }

            if (!user.Role.Equals(RoleEnums.Student.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                using var tx = await _userRepositories.BeginTransactionAsync();
                try
                {
                    var oldEmail = user.Email;
                    var newEmail = payload.Email.Trim();
                    if (!string.Equals(oldEmail, newEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        var existingUser = await _userRepositories.GetOtherUserByEmail(newEmail, userId);
                        if (existingUser != null)
                        {
                            throw new CustomException("Email đã tồn tại!");
                        }
                        user.Email = newEmail;
                    }

                    user.Name = payload.Name;
                    user.Phone = payload.Phone ?? "";
                    user.SchoolInfo = payload.SchoolInfo;
                    user.AvatarUrl = payload.AvatarUrl ?? "";

                    await _userRepositories.Update(user, saveChanges: false);
                    await _userRepositories.SaveChangesAsync();
                    await _userRepositories.CommitTransactionAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }

                return new MessageResultModel
                {
                    Message = "Ok"
                };
            }

            var student = user;

            var studentClass = await _studentClassRepositories.GetSingle(x => x.StudentId == userId, includeProperties: "Class");
            if (studentClass == null || studentClass.Class == null)
            {
                throw new CustomException("Học sinh chưa được gán vào lớp học!");
            }

            var className = TextConvert.ConvertFromUnicodeEscape(studentClass.Class.Name).Trim();
            var classBlockMatch = System.Text.RegularExpressions.Regex.Match(className, @"^\d+");
            var classBlock = classBlockMatch.Success ? classBlockMatch.Value : className;
            if (!int.TryParse(classBlock, out var classBlockNumber))
            {
                throw new CustomException("Không thể tạo mã định danh phụ huynh vì tên lớp không hợp lệ.");
            }
            var classBlockCode = classBlockNumber.ToString("D2");

            var listEmailToSend = new List<EmailReqModel>();

            using var transaction = await _userRepositories.BeginTransactionAsync();
            try
            {
                // Check if student email is changing
                var oldStudentEmail = student.Email;
                var newStudentEmail = payload.Email.Trim();
                bool isStudentEmailChanged = !string.Equals(oldStudentEmail, newStudentEmail, StringComparison.OrdinalIgnoreCase);

                if (isStudentEmailChanged)
                {
                    var existingUser = await _userRepositories.GetOtherUserByEmail(newStudentEmail, userId);
                    if (existingUser != null)
                    {
                        throw new CustomException("Email của học sinh đã tồn tại!");
                    }

                    student.Email = newStudentEmail;

                    // Send notification to old email
                    string fileOldPath = "../PTEducation.Business/TemplateEmail/EmailChangedOld.html";
                    if (File.Exists(fileOldPath))
                    {
                        string htmlOld = File.ReadAllText(fileOldPath);
                        htmlOld = htmlOld.Replace("{{NAME}}", student.Name);
                        htmlOld = htmlOld.Replace("{{OLD_EMAIL}}", oldStudentEmail);
                        htmlOld = htmlOld.Replace("{{NEW_EMAIL}}", newStudentEmail);
                        listEmailToSend.Add(new EmailReqModel
                        {
                            Email = oldStudentEmail,
                            HtmlContent = htmlOld
                        });
                    }

                    // Send notification to new email
                    string fileNewPath = "../PTEducation.Business/TemplateEmail/EmailChangedNew.html";
                    if (File.Exists(fileNewPath))
                    {
                        string htmlNew = File.ReadAllText(fileNewPath);
                        htmlNew = htmlNew.Replace("{{NAME}}", student.Name);
                        htmlNew = htmlNew.Replace("{{NEW_EMAIL}}", newStudentEmail);
                        htmlNew = htmlNew.Replace("{{DOMAIN_FE}}", _domainFE);
                        listEmailToSend.Add(new EmailReqModel
                        {
                            Email = newStudentEmail,
                            HtmlContent = htmlNew
                        });
                    }
                }

                student.Name = payload.Name;
                student.Phone = payload.Phone ?? "";
                student.SchoolInfo = payload.SchoolInfo;
                student.AvatarUrl = payload.AvatarUrl ?? "";

                await _userRepositories.Update(student, saveChanges: false);

                // Process Guardians
                var existingStudentGuardians = (await _studentGuardianRepositories.GetList(
                    x => x.StudentId == userId,
                    includeProperties: "Guardian"
                )).ToList();

                var payloadGuardians = payload.Guardians ?? new List<UserGuardianListResModel>();

                // Detect deletions
                var payloadGuardianIds = payloadGuardians
                    .Select(g => g.Id)
                    .Where(id => !string.IsNullOrEmpty(id) && !id.StartsWith("g-"))
                    .ToList();

                var guardiansToDelete = existingStudentGuardians
                    .Where(sg => !payloadGuardianIds.Contains(sg.GuardianId))
                    .ToList();

                if (guardiansToDelete.Count > 0)
                {
                    await _studentGuardianRepositories.DeleteRange(guardiansToDelete, saveChanges: false);
                }

                // Detect updates & additions
                int? nextGuardianSeq = null;

                foreach (var payloadG in payloadGuardians)
                {
                    var cleanEmail = payloadG.Email.Trim();

                    if (string.IsNullOrEmpty(payloadG.Id) || payloadG.Id.StartsWith("g-"))
                    {
                        // Add new guardian
                        var existingUser = await _userRepositories.GetOtherUserByEmail(cleanEmail, "");
                        if (existingUser != null)
                        {
                            throw new CustomException($"Email phụ huynh {payloadG.Name} ({cleanEmail}) đã tồn tại!");
                        }

                        if (nextGuardianSeq == null)
                        {
                            var usersInClassBlock = await _userRepositories.GetList(x =>
                                x.Id.StartsWith($"1{classBlockCode}") || x.Id.StartsWith($"2{classBlockCode}"));

                            var maxGuardianSequence = usersInClassBlock
                                .Where(x => x.Id.StartsWith($"2{classBlockCode}") && x.Id.Length == 7)
                                .Select(x => int.TryParse(x.Id.Substring(3, 4), out var seq) ? seq : 0)
                                .DefaultIfEmpty(0)
                                .Max();
                            nextGuardianSeq = maxGuardianSequence + 1;
                        }

                        if (nextGuardianSeq > 9999)
                        {
                            throw new CustomException("Đã vượt quá giới hạn mã giám hộ cho khối.");
                        }

                        var newGuardianId = $"2{classBlockCode}{nextGuardianSeq:0000}";
                        nextGuardianSeq++;

                        var newPassword = Authentication.GenerateRandomPassword();

                        var newGuardian = new User
                        {
                            Id = newGuardianId,
                            Name = payloadG.Name,
                            Email = cleanEmail,
                            Phone = payloadG.Phone ?? "",
                            Role = RoleEnums.Guardian.ToString(),
                            Status = student.Status, // Inherit status from student
                            IsNeedResetPassword = true,
                            PasswordBcrypt = Authentication.CreateHashPasswordBCrypt(newPassword)
                        };

                        await _userRepositories.Insert(newGuardian, saveChanges: false);

                        var newStudentGuardian = new StudentGuardian
                        {
                            Id = Guid.NewGuid(),
                            StudentId = userId,
                            GuardianId = newGuardianId,
                            IsPrimary = payloadG.IsPrimary,
                            Relationship = payloadG.Relationship
                        };

                        await _studentGuardianRepositories.Insert(newStudentGuardian, saveChanges: false);

                        string fileGuardianPath = "../PTEducation.Business/TemplateEmail/FirstInformationNewGuardian.html";
                        if (File.Exists(fileGuardianPath))
                        {
                            string guardHtml = File.ReadAllText(fileGuardianPath);
                            guardHtml = guardHtml.Replace("{{PASSWORD}}", newPassword);
                            guardHtml = guardHtml.Replace("{{CLASSNAME}}", className);
                            guardHtml = guardHtml.Replace("{{STUDENTNAME}}", payload.Name);
                            guardHtml = guardHtml.Replace("{{GUARDIANNAME}}", payloadG.Name);
                            guardHtml = guardHtml.Replace("{{USERNAME}}", cleanEmail);
                            guardHtml = guardHtml.Replace("{{DOMAIN_FE}}", _domainFE);
                            listEmailToSend.Add(new EmailReqModel
                            {
                                Email = cleanEmail,
                                HtmlContent = guardHtml
                            });
                        }
                    }
                    else
                    {
                        // Update existing guardian
                        var existingSG = existingStudentGuardians.FirstOrDefault(sg => sg.GuardianId == payloadG.Id);
                        if (existingSG != null && existingSG.Guardian != null)
                        {
                            var guardian = existingSG.Guardian;
                            var oldGuardianEmail = guardian.Email;
                            bool isGuardianEmailChanged = !string.Equals(oldGuardianEmail, cleanEmail, StringComparison.OrdinalIgnoreCase);

                            if (isGuardianEmailChanged)
                            {
                                var existingUser = await _userRepositories.GetOtherUserByEmail(cleanEmail, guardian.Id);
                                if (existingUser != null)
                                {
                                    throw new CustomException($"Email phụ huynh {payloadG.Name} ({cleanEmail}) đã tồn tại!");
                                }

                                guardian.Email = cleanEmail;

                                // Send notification to old email
                                string fileOldPath = "../PTEducation.Business/TemplateEmail/EmailChangedOld.html";
                                if (File.Exists(fileOldPath))
                                {
                                    string htmlOld = File.ReadAllText(fileOldPath);
                                    htmlOld = htmlOld.Replace("{{NAME}}", guardian.Name);
                                    htmlOld = htmlOld.Replace("{{OLD_EMAIL}}", oldGuardianEmail);
                                    htmlOld = htmlOld.Replace("{{NEW_EMAIL}}", cleanEmail);
                                    listEmailToSend.Add(new EmailReqModel
                                    {
                                        Email = oldGuardianEmail,
                                        HtmlContent = htmlOld
                                    });
                                }

                                // Send notification to new email
                                string fileNewPath = "../PTEducation.Business/TemplateEmail/EmailChangedNew.html";
                                if (File.Exists(fileNewPath))
                                {
                                    string htmlNew = File.ReadAllText(fileNewPath);
                                    htmlNew = htmlNew.Replace("{{NAME}}", guardian.Name);
                                    htmlNew = htmlNew.Replace("{{NEW_EMAIL}}", cleanEmail);
                                    htmlNew = htmlNew.Replace("{{DOMAIN_FE}}", _domainFE);
                                    listEmailToSend.Add(new EmailReqModel
                                    {
                                        Email = cleanEmail,
                                        HtmlContent = htmlNew
                                    });
                                }
                            }

                            guardian.Name = payloadG.Name;
                            guardian.Phone = payloadG.Phone ?? "";
                            await _userRepositories.Update(guardian, saveChanges: false);

                            existingSG.IsPrimary = payloadG.IsPrimary;
                            existingSG.Relationship = payloadG.Relationship;
                            await _studentGuardianRepositories.Update(existingSG, saveChanges: false);
                        }
                    }
                }

                await _userRepositories.SaveChangesAsync();
                await _userRepositories.CommitTransactionAsync();

                if (listEmailToSend.Count > 0)
                {
                    await _email.SendEmail("[Thông tin đăng nhập]", listEmailToSend);
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return new MessageResultModel
            {
                Message = "Ok"
            };
        }
    }
}
