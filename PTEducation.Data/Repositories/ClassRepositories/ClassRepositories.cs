using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.Repositories.ClassRepositories
{
    public class ClassRepositories : GenericRepositories<Class>, IClassRepositories
    {
        public ClassRepositories(PteducationContext context)
        : base(context)
        {
        }
    }
}
