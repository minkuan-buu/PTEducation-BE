using PTEducation.Business.ApplicationMiddleware;
using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.StudentClassRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.StudentClassServices
{
    public class StudentClassServices : IStudentClassServices
    {
        private readonly IStudentClassRepositories _studentClassRepository;
        public StudentClassServices(IStudentClassRepositories studentClassRepository)
        {
            _studentClassRepository = studentClassRepository;
        }

        public async Task<List<StudentClass>> GetStudentInClass(Guid classId)
        {
            var ListStudent = await _studentClassRepository.GetList(x => x.ClassId.Equals(classId), includeProperties: "Student");
            foreach (var item in ListStudent)
            {
                item.Student.Name = TextConvert.ConvertFromUnicodeEscape(item.Student.Name);
            }
            return ListStudent.ToList();
        }

        public async Task<List<StudentClassResModelForSheet>> GetStudentInClassForSheet(Guid classId)
        {
            var ListStudent = await _studentClassRepository.GetList(x => x.ClassId.Equals(classId), includeProperties: "Student");
            foreach (var item in ListStudent)
            {
                item.Student.Name = TextConvert.ConvertFromUnicodeEscape(item.Student.Name);
            }
            return ListStudent.Select(x => new StudentClassResModelForSheet()
            {
                StudentClassId = x.Id,
                Id = x.Student.Id,
                Name = x.Student.Name,
                Score = 0,
                Note = null
            }).ToList();
        }
    }
}
