using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PTEducation.Data.Repositories.UserRepositories
{
    public class UserRepositories : GenericRepositories<User>, IUserRepositories
    {
        public UserRepositories(PteducationContext context)
        : base(context)
        {
        }

        public async Task<User?> GetOtherUserByEmail(string email, string UserId)
        {
            return await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email && x.Id != UserId);
        }
    }
}
