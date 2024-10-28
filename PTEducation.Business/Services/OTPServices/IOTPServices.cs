using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GenericRepositories;

namespace PTEducation.Business.Services.OTPServices
{
    public interface IOTPServices
    {
        Task<MessageResultModel> SendOTPEmailRequest(string Email);
        Task<DataResultModel<UserTemp>> VerifyOTPCode(string Email, string OTPCode);
    }
}
