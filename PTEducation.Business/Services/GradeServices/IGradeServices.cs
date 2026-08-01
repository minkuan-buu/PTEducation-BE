using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;

namespace PTEducation.Business.Services.GradeServices
{
    public interface IGradeServices
    {
        Task<IEnumerable<GradeResModel>> GetAll();
        Task<GradeResModel> Create(GradeCreateReqModel req);
        Task<GradeResModel> Update(GradeUpdateReqModel req);
        Task<bool> Delete(int id);
    }
}
