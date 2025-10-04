using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.Repositories.UserRepositories
{
    public interface IUserRepositories : IGenericRepositories<User>
    {
        Task<User?> GetOtherUserByEmail(string email, string UserId);
    }
}
