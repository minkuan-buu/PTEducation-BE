using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.Repositories.AttendanceDetailRepositories
{
    public class AttendanceDetailRepositories : GenericRepositories<AttendanceDetail>, IAttendanceDetailRepositories
    {
        public AttendanceDetailRepositories(PteducationContext context) : base(context)
        {
        }
    }
}
