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

namespace PTEducation.Business.Services.UserServices
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepositories _userRepositories;
        private readonly IStudentClassRepositories _studentClassRepositories;
        private readonly IOTPRepositories _otpRepositories;
        private readonly IAttendanceDetailRepositories _attendanceDetailRepositories;
        private readonly IScoreDetailRepositories _scoreDetailRepositories;
        private readonly IEmail _email;
        private readonly IMapper _mapper;
        public UserServices(IUserRepositories userRepositories, IMapper mapper, IEmail email, IStudentClassRepositories studentClassRepositories, IAttendanceDetailRepositories attendanceDetailRepositories, IScoreDetailRepositories scoreDetailRepositories, IOTPRepositories otpRepositories)
        {
            _studentClassRepositories = studentClassRepositories;
            _userRepositories = userRepositories;
            _email = email;
            _mapper = mapper;
            _attendanceDetailRepositories = attendanceDetailRepositories;
            _scoreDetailRepositories = scoreDetailRepositories;
            _otpRepositories = otpRepositories;
        }

        public async Task<DataResultModel<UserLoginResModel>> Login(string Username, string Password)
        {
            var CheckExist = await _userRepositories.GetSingle(x => (x.Email.Equals(Username) || x.Id.Equals(Username)) && x.Status.Equals(AccountStatusEnums.Active.ToString()));
            if (CheckExist == null)
            {
                throw new CustomException("Không tìm thấy tài khoản!");
            }
            var Auth = Authentication.VerifyPasswordHashed(Password, CheckExist.Salt, CheckExist.Password);
            if (!Auth)
            {
                throw new CustomException("Mật khẩu không chính xác!");
            }
            var User = _mapper.Map<UserLoginResModel>(CheckExist);
            User.Token = Authentication.GenerateJWT(CheckExist);
            User.IsNeedChangePassword = CheckExist.IsNeedResetPassword;
            return new DataResultModel<UserLoginResModel>()
            {
                Data = User
            };
        }

        public async Task<MessageResultModel> Register(UserRegisterReqModel ReqModel)
        {
            var CheckExist = await _userRepositories.GetSingle(x => x.Email == ReqModel.Email || x.Id == ReqModel.Id);
            if (CheckExist != null)
            {
                throw new CustomException("Tài khoản với Email hoặc Id này đã tồn tại!");
            }
            var NewUser = _mapper.Map<User>(ReqModel);
            if (ReqModel.Id == null)
            {
                Random rnd = new Random();
                NewUser.Id = $"{ReqModel.Role}-{rnd.Next(100000, 999999)}";
            }
            var GeneratePassword = Authentication.GenerateRandomPassword();
            CreateHashPasswordModel HashedPassword = Authentication.CreateHashPassword(GeneratePassword);
            NewUser.Status = AccountStatusEnums.Active.ToString();
            NewUser.Password = HashedPassword.HashedPassword;
            NewUser.Salt = HashedPassword.Salt;
            await _userRepositories.Insert(NewUser);
            string FilePath = "../PTEducation.Business/TemplateEmail/FirstInformation.html";
            string Html = File.ReadAllText(FilePath);
            Html = Html.Replace("{{Password}}", GeneratePassword);
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
            return new MessageResultModel
            {
                Message = "Ok"
            };
        }

        public async Task<MessageResultModel> Register(UserRegisterWithGuardianInfo ReqModel)
        {
            var CheckExist = await _userRepositories.GetSingle(x => x.Email == ReqModel.Email);
            if (CheckExist != null)
            {
                throw new CustomException("Tài khoản với Email này đã tồn tại!");
            }
            List<User> ListAddUser = new List<User>();
            var NewStudent = _mapper.Map<User>(ReqModel);
            var GeneratePassword = Authentication.GenerateRandomPassword();
            string HashedPassword = Authentication.CreateHashPasswordBCrypt(GeneratePassword);
            NewStudent.PasswordBcrypt = HashedPassword;
            NewStudent.Status = AccountStatusEnums.Active.ToString();
            ListAddUser.Add(NewStudent);
            List<StudentGuardian> ListStudentGuardian = new List<StudentGuardian>();
            foreach (var guardian in ReqModel.Guardians)
            {
                var NewGuardian = _mapper.Map<User>(guardian);
                NewGuardian.PasswordBcrypt = HashedPassword;
                NewGuardian.Status = AccountStatusEnums.Active.ToString();
                ListAddUser.Add(NewGuardian);

                var NewStudentGuardian = new StudentGuardian
                {
                    Id = Guid.NewGuid(),
                    
                    Guardian = NewGuardian,
                    IsPrimary = guardian.IsPrimary,
                    Relationship = guardian.Relationship
                };
            }
            return new MessageResultModel
            {
                Message = "Ok"
            };
        }

        public async Task<MessageResultModel> ChangePassword(UserChangePasswordReqModel ReqModel, string token)
        {
            var userId = Authentication.DecodeToken(token, "userid");
            var user = await _userRepositories.GetSingle(x => x.Id == userId);
            if (user == null)
            {
                throw new CustomException("Không tìm thấy người dùng!");
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
            var Auth = Authentication.VerifyPasswordHashed(ReqModel.OldPassword, user.Salt, user.Password);
            if (!Auth)
            {
                throw new CustomException("Mật khẩu cũ không chính xác!");
            }
            CreateHashPasswordModel HashedPassword = Authentication.CreateHashPassword(ReqModel.NewPassword);
            user.Password = HashedPassword.HashedPassword;
            user.Salt = HashedPassword.Salt;
            user.IsNeedResetPassword = false;
            await _userRepositories.Update(user);
            return new MessageResultModel
            {
                Message = "Ok"
            };
        }

        public async Task<DataResultModel<UserProfileResModel>> GetMyProfile(string token)
        {
            var userId = Authentication.DecodeToken(token, "userid");
            var user = await _userRepositories.GetSingle(x => x.Id == userId && x.Status.Equals(GeneralStatusEnums.Active.ToString()), includeProperties: "StudentClasses.Class");
            if (user == null)
            {
                throw new CustomException("Không tìm thấy người dùng!");
            }
            var Result = _mapper.Map<UserProfileResModel>(user);
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
                var Auth = Authentication.CreateHashPassword(ReqModel.NewPassword);
                user.Password = Auth.HashedPassword;
                user.Salt = Auth.Salt;
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
                var GeneratePassword = Environment.GetEnvironmentVariable("ADMIN_DEFAULT_PASSWORD") ?? throw new CustomException("Default admin password is not configured in the system. Please contact the administrator.");
                CreateHashPasswordModel HashedPassword = Authentication.CreateHashPassword(GeneratePassword);
                NewUser.Status = AccountStatusEnums.Active.ToString();
                NewUser.Password = HashedPassword.HashedPassword;
                NewUser.Salt = HashedPassword.Salt;
                listUser.Add(NewUser);
                string FilePath = "../PTEducation.Business/TemplateEmail/ManagerInformation.html";
                string Html = File.ReadAllText(FilePath);
                Html = Html.Replace("{{ID}}", NewUser.Id);
                Html = Html.Replace("{{Password}}", GeneratePassword);
                Html = Html.Replace("{{Email}}", item.Email);
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

        public async Task<PagedListDataResultModel<ManagerResModel>> GetManagers(int? pageIndex, UserFilter searchModel, string token)
        {
            var userId = Authentication.DecodeToken(token, "userid");
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
            CreateHashPasswordModel HashedPassword = Authentication.CreateHashPassword(GeneratePassword);
            StudentClass.Student.Password = HashedPassword.HashedPassword;
            StudentClass.Student.Salt = HashedPassword.Salt;
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
    }
}
