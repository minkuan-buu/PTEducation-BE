using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.DTO.RequestModel
{
    public class UserReqModel
    {
    }

    public class UserLoginReqModel
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class UserRegisterReqModel
    {
        public string? Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Role { get; set; } = null!;
    }

    public class UserRegisterWithGuardianInfo
    {
        public string Name { get; set; } = null!;
        public Guid ClassId { get; set; }
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string School { get; set; } = null!;
        public List<GuardianInfoReqModel> Guardians { get; set; } = new List<GuardianInfoReqModel>();
    }

    public class GuardianInfoReqModel
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Relationship { get; set; } = null!;
        public bool IsPrimary { get; set; } = false;
    }

    public class ManagerRegisterReqModel
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
    }

    public class UserChangePasswordReqModel
    {
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }

    public class UserResetPasswordReqModel
    {
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }

    public class StudentUpdateReqModel
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? DefaultPassword { get; set; }
        public bool IsResendInfo { get; set; } = false;
    }

    public class AccessReqModel
    {
        [RegularExpression("^(Approved|Rejected)$")]
        public string AccessStatus { get; set; } = "Rejected";
    }
}
