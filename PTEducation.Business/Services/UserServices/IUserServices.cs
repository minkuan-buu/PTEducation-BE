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
        Task<DataResultModel<RawUserLoginResModel>> Login(string Username, string Password);
        Task<MessageResultModel> Register(UserRegisterReqModel ReqModel);
        Task<MessageResultModel> Register(UserRegisterWithGuardianInfo ReqModel);
        Task<MessageResultModel> ChangePassword(UserChangePasswordReqModel ReqModel, string token);
        Task<DataResultModel<UserProfileResModel>> GetMyProfile(string token);
        Task<MessageResultModel> ResetPassword(UserResetPasswordReqModel ReqModel, string token);
        Task<MessageResultModel> Register(List<ManagerRegisterReqModel> ReqModel);
        Task<PagedListDataResultModel<ManagerResModel>> GetManagers(int? pageIndex, UserFilter searchModel, string userId);
        Task<MessageResultModel> Deactivate(string userId);
        Task<MessageResultModel> ReActivate(string userId);
        Task<MessageResultModel> UpdateStudentInfo(StudentUpdateReqModel ReqModel, Guid StudentClassId);
        Task<MessageResultModel> DeleteStudent(Guid StudentClassId);
        Task<MessageResultModel> ConvertNameFromUnicodeEscapeToUnicode();
        Task<PagedListDataResultModel<UserListResModel>> GetAllStudents(int? pageIndex, UserFilter searchModel);
        Task<MessageResultModel> UpdateStudentAccess(string userId, AccessReqModel reqModel);
        Task<MessageResultModel> DeleteStudent(string userId);
        Task InitAdminIfNeeded();
        //Task<bool> SendMail();
        Task<DataResultModel<UserEditResModel>> GetUserDetail(string userId);
        Task<MessageResultModel> UploadAvatar(string userId, AttachmentReqModel reqModel);
        Task<MessageResultModel> ResetPassword(string userId);
    }
}
