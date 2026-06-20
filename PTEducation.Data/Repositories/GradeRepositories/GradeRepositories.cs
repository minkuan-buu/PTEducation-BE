using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.Repositories.GradeRepositories
{
    public class GradeRepositories : GenericRepositories<Grade>, IGradeRepositories
    {
        public GradeRepositories(PteducationContext context)
        : base(context)
        {
        }
    }
}
