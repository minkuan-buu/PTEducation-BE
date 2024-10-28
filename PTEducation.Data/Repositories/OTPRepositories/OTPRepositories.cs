using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.Repositories.OTPRepositories
{
    public class OTPRepositories : GenericRepositories<Otp>, IOTPRepositories
    {
        public OTPRepositories(PteducationContext context) : base(context)
        {
        }
    }
}
