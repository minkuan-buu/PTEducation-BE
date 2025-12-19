using PTEducation.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.StudentClassServices
{
    public interface IStudentClassServices
    {
        Task<List<StudentClass>> GetStudentInClass(Guid classId);
        Task<List<StudentClassResModelForSheet>> GetStudentInClassForSheet(Guid classId);
    }
}
