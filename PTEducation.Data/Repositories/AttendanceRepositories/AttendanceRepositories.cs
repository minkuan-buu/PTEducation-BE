using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.Repositories.AttendanceRepositories
{
    public class AttendanceRepositories : GenericRepositories<Attendance>, IAttendanceRepositories
    {
        public AttendanceRepositories(PteducationContext context) : base(context)
        {
        }
    }
}
