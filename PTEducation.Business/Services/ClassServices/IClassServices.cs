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
        Task<MessageResultModel> CreateClass(ClassCreateReqModel ClassReq, string token);
        Task<MessageResultModel> UpdateClass(ClassUpdateReqModel ClassReq);
        Task<MessageResultModel> SoftDeleteClass(Guid Id);
        Task<MessageResultModel> HardDeleteClass(Guid Id);
        Task<MessageResultModel> RestoreClass(Guid Id);
        Task<MessageResultModel> ManualAddStudent(ManualAddStudentClassModel AddStudentsReq);
        Task<MessageResultModel> MoveOutStudent(MoveOutStudentClassModel MoveOutReq);
        Task<ListDataResultModel<ClassListSelectResModel>> GetClassSelectList();
    }
}
