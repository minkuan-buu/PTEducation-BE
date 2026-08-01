using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.OverviewServices
{
    public interface IOverviewServices
    {
        Task<ListDataResultModel<ClassOptionResModel>> GetStudentClasses(string userId);
        Task<DataResultModel<StudentGuardianOverviewResModel>> GetOverviewForStudentOrGuardian(string userId, string? classId = null);
        Task<DataResultModel<AttendanceStudentGuardianOverviewResModel>> GetAttendanceOverviewForStudentOrGuardian(string userId, string? classId = null);
    }
}