using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.ClassServices
{
    public interface IClassServices
    {
        Task<DataResultModel<ClassDetailResModel>> GetClassDetail(Guid Id);
        Task<PagedListDataResultModel<ListClassResModel>> GetClassList(int? pageIndex, ClassFilter searchModel);
        Task<DataResultModel<List<ListClassResModel>>> GetClassList();
        Task<MessageResultModel> CreateClass(ClassCreateReqModel ClassReq, string userId);
        Task<MessageResultModel> CreateClassV2(ClassCreateReqModelV2 ClassReq, string userId);
        Task<MessageResultModel> UpdateClass(ClassUpdateReqModel ClassReq);
        Task<MessageResultModel> SoftDeleteClass(Guid Id);
        Task<MessageResultModel> HardDeleteClass(Guid Id);
        Task<MessageResultModel> RestoreClass(Guid Id);
        Task<MessageResultModel> ManualAddStudent(ManualAddStudentClassModel AddStudentsReq);
        Task<MessageResultModel> MoveOutStudent(MoveOutStudentClassModel MoveOutReq);
        Task<ListDataResultModel<ClassListSelectResModel>> GetClassSelectList();
        Task<DataResultModel<Guid>> GetClassIdByName(string ClassName);
        Task<ClassScoreStudentExport> GetStudentScoreByClassIdAndRangeDate(Guid ClassId, DateTime? FromDate, DateTime? ToDate);
    }
}
