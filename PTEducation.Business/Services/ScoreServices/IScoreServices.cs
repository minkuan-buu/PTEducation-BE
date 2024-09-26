using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.ScoreServices
{
    public interface IScoreServices
    {
        Task<DataResultModel<ScoreDetailResModel>> GetScoreDetail(Guid Id);
        Task<DataResultModel<List<ScoreListResModel>>> GetListScore(int? pageIndex, ScoreFilter filter);
        Task<MessageResultModel> CreateScore(ScoreCreateReqModel ScoreReq, string token);
        Task<MessageResultModel> UpdateScore(ScoreUpdateReqModel ScoreReq);
        Task<MessageResultModel> SoftDeleteScore(Guid Id);
        Task<MessageResultModel> RestoreScore(Guid Id);
    }
}
