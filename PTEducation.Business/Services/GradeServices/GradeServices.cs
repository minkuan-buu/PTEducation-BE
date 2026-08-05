using Microsoft.EntityFrameworkCore;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GradeRepositories;

namespace PTEducation.Business.Services.GradeServices
{
    public class GradeServices : IGradeServices
    {
        private readonly IGradeRepositories _gradeRepositories;

        public GradeServices(IGradeRepositories gradeRepositories)
        {
            _gradeRepositories = gradeRepositories;
        }

        public async Task<IEnumerable<GradeResModel>> GetAll()
        {
            var grades = await _gradeRepositories.GetList(
                includeProperties: "Classes",
                orderBy: q => q.OrderBy(g => g.GradeName)
            );

            return grades.Select(g => new GradeResModel
            {
                Id = g.Id,
                GradeName = g.GradeName,
                GroupId = g.GroupId,
                TotalClasses = g.Classes?.Count ?? 0
            });
        }

        public async Task<GradeResModel> Create(GradeCreateReqModel req)
        {
            var allGrades = await _gradeRepositories.GetList();
            var gradeId = allGrades.Any() ? allGrades.Max(g => g.Id) + 1 : 1;

            var grade = new Grade
            {
                Id = gradeId,
                GradeName = req.GradeName,
                GroupId = req.GroupId
            };

            await _gradeRepositories.Insert(grade);

            return new GradeResModel
            {
                Id = grade.Id,
                GradeName = grade.GradeName,
                GroupId = grade.GroupId,
                TotalClasses = 0
            };
        }

        public async Task<GradeResModel> Update(GradeUpdateReqModel req)
        {
            var grade = await _gradeRepositories.GetSingle(
                filter: g => g.Id == req.Id,
                includeProperties: "Classes"
            );

            if (grade == null)
            {
                throw new Exception("Khối không tồn tại");
            }

            grade.GradeName = req.GradeName;
            grade.GroupId = req.GroupId;

            await _gradeRepositories.Update(grade);

            return new GradeResModel
            {
                Id = grade.Id,
                GradeName = grade.GradeName,
                GroupId = grade.GroupId,
                TotalClasses = grade.Classes?.Count ?? 0
            };
        }

        public async Task<bool> Delete(int id)
        {
            var grade = await _gradeRepositories.GetSingle(
                filter: g => g.Id == id,
                includeProperties: "Classes"
            );

            if (grade == null)
            {
                throw new Exception("Khối không tồn tại");
            }

            if (grade.Classes != null && grade.Classes.Count > 0)
            {
                throw new Exception("Không thể xóa khối đang có lớp học");
            }

            await _gradeRepositories.Delete(grade);
            return true;
        }
    }
}
