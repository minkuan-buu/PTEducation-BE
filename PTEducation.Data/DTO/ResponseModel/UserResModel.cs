using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.DTO.ResponseModel
{
    public class UserResModel
    {
    }

    public class CreateHashPasswordModel
    {
        public byte[] Salt { get; set; } = null!;
        public byte[] HashedPassword { get; set; } = null!;
    }

    public class UserLoginResModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsResetPassword = false;
        public string Token { get; set; } = null!;
        public bool IsNeedChangePassword { get; set; } = false;
    }

    public class RawUserLoginResModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsResetPassword = false;
        public string Token { get; set; } = null!;
        public string EncryptedToken { get; set; } = null!;
        public bool IsNeedChangePassword { get; set; } = false;
    }

    public class UserTemp
    {
        public string TempToken { get; set; } = null!;
    }

    public class UserProfileResModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? ClassName { get; set; }
    }

    public class ManagerResModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Status { get; set; } = null!;
    }

    public class UserFilter
    {
        public string? Keyword { get; set; }
    }

    public class UserListResModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? ClassName { get; set; }
        public List<UserGuardianListResModel> Guardians { get; set; } = null!;
    }

    public class UserGuardianListResModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Relationship { get; set; } = null!;
    }
}
