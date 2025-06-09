using AutoMapper;
using PTEducation.Business.ApplicationMiddleware;
using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Repositories.UserRepositories;
using PTEducation.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PTEducation.Data.Enums;
using PTEducation.Business.Ultilities.Email;
using PTEducation.Data.Repositories.StudentClassRepositories;

namespace PTEducation.Business.Services.UserServices
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepositories _userRepositories;
        private readonly IStudentClassRepositories _studentClassRepositories;
        private readonly IEmail _email;
        private readonly IMapper _mapper;
        public UserServices(IUserRepositories userRepositories, IMapper mapper, IEmail email, IStudentClassRepositories studentClassRepositories)
        {
            _studentClassRepositories = studentClassRepositories;
            _userRepositories = userRepositories;
            _email = email;
            _mapper = mapper;
        }

        public async Task<DataResultModel<UserLoginResModel>> Login(string Username, string Password)
        {
            var CheckExist = await _userRepositories.GetSingle(x => x.Email.Equals(Username) || x.Id.Equals(Username));
            if (CheckExist == null)
            {
                throw new CustomException("Account not found!");
            }
            var Auth = Authentication.VerifyPasswordHashed(Password, CheckExist.Salt, CheckExist.Password);
            if (!Auth)
            {
                throw new CustomException("Password is incorrect!");
            }
            var User = _mapper.Map<UserLoginResModel>(CheckExist);
            User.Token = Authentication.GenerateJWT(CheckExist);
            User.IsNeedChangePassword = CheckExist.IsNeedResetPassoword;
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
                throw new CustomException("Account with this Email or Id is existed!");
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

        public async Task<MessageResultModel> ChangePassword(UserChangePasswordReqModel ReqModel, string token)
        {
            var userId = Authentication.DecodeToken(token, "userid");
            var user = await _userRepositories.GetSingle(x => x.Id == userId);
            if (user == null)
            {
                throw new CustomException("User not found!");
            }
            if (ReqModel.NewPassword != ReqModel.ConfirmPassword)
            {
                throw new CustomException("New password and confirm password is not match!");
            }
            if (ReqModel.OldPassword == ReqModel.NewPassword)
            {
                throw new CustomException("New password is the same as old password!");
            }
            if (ReqModel.NewPassword.Length < 6)
            {
                throw new CustomException("Password must be at least 6 characters!");
            }
            var Auth = Authentication.VerifyPasswordHashed(ReqModel.OldPassword, user.Salt, user.Password);
            if (!Auth)
            {
                throw new CustomException("Old password is incorrect!");
            }
            CreateHashPasswordModel HashedPassword = Authentication.CreateHashPassword(ReqModel.NewPassword);
            user.Password = HashedPassword.HashedPassword;
            user.Salt = HashedPassword.Salt;
            user.IsNeedResetPassoword = false;
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
                throw new CustomException("User not found!");
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
                    throw new CustomException("User not found!");
                }
                if (ReqModel.NewPassword != ReqModel.ConfirmPassword)
                {
                    throw new CustomException("New password and confirm password is not match!");
                }
                if (ReqModel.NewPassword.Length < 6)
                {
                    throw new CustomException("Password must be at least 6 characters!");
                }
                var Auth = Authentication.CreateHashPassword(ReqModel.NewPassword);
                user.Password = Auth.HashedPassword;
                user.Salt = Auth.Salt;
                user.IsNeedResetPassoword = false;
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
    }
}
