using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.UserServices
{
    public interface IUserServices
    {
        Task<DataResultModel<UserLoginResModel>> Login(string Username, string Password);
        Task<MessageResultModel> Register(UserRegisterReqModel ReqModel);
        Task<MessageResultModel> ChangePassword(UserChangePasswordReqModel ReqModel, string token);
        Task<DataResultModel<UserProfileResModel>> GetMyProfile(string token);
        //Task<bool> SendMail();
    }
}
