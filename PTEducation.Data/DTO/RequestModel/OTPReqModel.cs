using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.DTO.RequestModel
{
    public class OTPReqModel
    {
    }

    public class UserVerifyOTPReqModel
    {
        public string Email { get; set; } = null!;
        public string OTPCode { get; set; } = null!;
    }
}
