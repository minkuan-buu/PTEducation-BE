using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.Repositories.TuitionRepositories
{
    public class TuitionPeriodRepositories : GenericRepositories<TuitionPeriod>, ITuitionPeriodRepositories
    {
        public TuitionPeriodRepositories(PteducationContext context)
        : base(context)
        {
        }
    }

    public class StudentTuitionRepositories : GenericRepositories<StudentTuition>, IStudentTuitionRepositories
    {
        public StudentTuitionRepositories(PteducationContext context)
        : base(context)
        {
        }
    }
}
