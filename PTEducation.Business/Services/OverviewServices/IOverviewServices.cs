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
        Task<DataResultModel<StudentGuardianOverviewResModel>> GetOverviewForStudentOrGuardian(string userId);
    }
}