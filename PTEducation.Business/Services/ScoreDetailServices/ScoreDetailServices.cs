using PTEducation.Business.ApplicationMiddleware;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Entities;
using PTEducation.Data.Enums;
using PTEducation.Data.Repositories.ScoreDetailRepositories;
using PTEducation.Data.Repositories.ScoreRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.ScoreDetailServices
{
    public class ScoreDetailServices : IScoreDetailServices
    {
        private readonly IScoreDetailRepositories _scoreDetailRepositories;
        private readonly IScoreRepositories _scoreRepositories;
        public ScoreDetailServices(IScoreDetailRepositories scoreDetailRepositories, IScoreRepositories scoreRepositories)
        {
            _scoreDetailRepositories = scoreDetailRepositories;
            _scoreRepositories = scoreRepositories;
        }

        public async Task<MessageResultModel> UpdateScore(ScoreDetailUpdateReqModel ScoreReq)
        {
            var Score = await _scoreRepositories.GetSingle(x => x.Id.Equals(ScoreReq.Id), includeProperties: "ScoreDetails");
            var ScoreDetail = Score.ScoreDetails.ToList();
            foreach (var score in ScoreReq.ScoreReqList)
            {
                var ScoreUpdate = ScoreDetail.FirstOrDefault(x => x.StudentClassId.Equals(score.StudentClassId));
                if (ScoreUpdate != null)
                {
                    ScoreUpdate.Score = score.Score;
                    ScoreUpdate.Note = score.Note != null ? TextConvert.ConvertToUnicodeEscape(score.Note) : null;
                }
                else
                {
                    var NewScore = new ScoreDetail()
                    {
                        Id = Guid.NewGuid(),
                        ScoreId = ScoreReq.Id,
                        StudentClassId = score.StudentClassId,
                        Score = score.Score,
                        Note = score.Note != null ? TextConvert.ConvertToUnicodeEscape(score.Note) : null,
                        Status = GeneralStatusEnums.Active.ToString()
                    };
                    ScoreDetail.Add(NewScore);
                }
            }
            Score.ScoreDetails = ScoreDetail;
            await _scoreRepositories.Update(Score);
            return new MessageResultModel()
            {
                Message = "Ok"
            };
        }
    }
}
