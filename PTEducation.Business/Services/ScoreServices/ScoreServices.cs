using AutoMapper;
using Org.BouncyCastle.Asn1.Cmp;
using PTEducation.Business.ApplicationMiddleware;
using PTEducation.Business.Ultilities.FilterCombine;
using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Entities;
using PTEducation.Data.Enums;
using PTEducation.Data.Repositories.ClassRepositories;
using PTEducation.Data.Repositories.ScoreDetailRepositories;
using PTEducation.Data.Repositories.ScoreRepositories;
using PTEducation.Data.Repositories.StudentClassRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.ScoreServices
{
    public class ScoreServices : IScoreServices
    {
        private readonly IClassRepositories _classRepositories;
        private readonly IScoreRepositories _scoreRepositories;
        private readonly IScoreDetailRepositories _scoreDetailRepositories;
        private readonly IStudentClassRepositories _studentClassRepositories;
        private readonly IMapper _mapper;
        public ScoreServices(IScoreRepositories scoreRepositories, IScoreDetailRepositories scoreDetailRepositories, IMapper mapper, IStudentClassRepositories studentClassRepositories, IClassRepositories classRepositories)
        {
            _classRepositories = classRepositories;
            _scoreRepositories = scoreRepositories;
            _scoreDetailRepositories = scoreDetailRepositories;
            _studentClassRepositories = studentClassRepositories;
            _mapper = mapper;
        }

        public async Task<DataResultModel<ScoreDetailResModel>> GetScoreDetail(Guid Id)
        {
            var CheckExist = await _scoreRepositories.GetSingle(x => x.Id.Equals(Id), includeProperties: "CreateByNavigation,ScoreDetails.StudentClass.Student,Class");
            if (CheckExist == null)
            {
                throw new CustomException("Score not found");
            }
            var Result = _mapper.Map<ScoreDetailResModel>(CheckExist);
            var StudentInClass = await _studentClassRepositories.GetList(x => x.ClassId.Equals(CheckExist.ClassId));
            var ListStudentInClass = StudentInClass.Select(x => x.Id).ToList();
            var ListStudentNotHaveScore = ListStudentInClass.Except(CheckExist.ScoreDetails.Select(x => x.StudentClassId)).ToList();
            foreach (var student in ListStudentNotHaveScore) {
                var Student = await _studentClassRepositories.GetSingle(x => x.Id.Equals(student), includeProperties: "Student");
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                Result.ScoreDetails.Add(new ScoreDetailStudentResModel()
                {
                    StudentClassId = student,
                    Id = Student.Student.Id,
                    Name = TextConvert.ConvertFromUnicodeEscape(Student.Student.Name),
                    Score = 0
                });
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            return new DataResultModel<ScoreDetailResModel>()
            {
                Data = Result
            };
        }

        public async Task<DataResultModel<List<ScoreListResModel>>> GetListScore(int? pageIndex, ScoreFilter filter)
        {
            var allScore = await ViewAllScore(pageIndex, filter);
            var Result = _mapper.Map<List<ScoreListResModel>>(allScore);
            return new DataResultModel<List<ScoreListResModel>>()
            {
                Data = Result
            };
        }

        public async Task<MessageResultModel> CreateScore(ScoreCreateReqModel ScoreReq, string token)
        {
            var userId = Authentication.DecodeToken(token, "userid");
            var CheckExist = await _scoreRepositories.GetSingle(x => x.TestDateAt.Equals(ScoreReq.TestDateAt));
            if (CheckExist != null)
            {
                throw new CustomException("Score already exists");
            }
            var NewScoreId = Guid.NewGuid();
            var NewScore = _mapper.Map<Score>(ScoreReq);
            NewScore.Id = NewScoreId;
            NewScore.CreateBy = userId;
            List<ScoreDetail> ListScoreDetail = new();
            foreach (var item in ScoreReq.ScoreReqList)
            {
                var CheckStudentClass = await _studentClassRepositories.GetSingle(x => x.Id.Equals(item.StudentClassId), includeProperties: "Class,Student");
                if (CheckStudentClass == null)
                {
                    throw new CustomException("An error has excuted! Please download template and try again");
                }
                if(CheckStudentClass.Class.Id != ScoreReq.ClassId)
                {
                    throw new CustomException($"Student {CheckStudentClass.Student.Id} has not attended {CheckStudentClass.Class.Name}");
                }
                var NewScoreDetail = _mapper.Map<ScoreDetail>(item);
                NewScoreDetail.ScoreId = NewScoreId;
                ListScoreDetail.Add(NewScoreDetail);
            }
            await _scoreRepositories.Insert(NewScore);
            await _scoreDetailRepositories.InsertRange(ListScoreDetail);
            return new MessageResultModel
            {
                Message = "Ok",
            };
        }

        public async Task<MessageResultModel> UpdateScore(ScoreUpdateReqModel ScoreReq)
        {
            var CheckExist = await _scoreRepositories.GetSingle(x => x.Id.Equals(ScoreReq.Id));
            if (CheckExist == null)
            {
                throw new Exception("Score not found");
            }
            CheckExist.TestDateAt = ScoreReq.TestDateAt;
            await _scoreRepositories.Update(CheckExist);
            return new MessageResultModel
            {
                Message = "Ok",
            };
        }

        public async Task<MessageResultModel> SoftDeleteScore(Guid Id)
        {
            var CheckExist = await _scoreRepositories.GetSingle(x => x.Id.Equals(Id), includeProperties: "ScoreDetails");
            if (CheckExist == null)
            {
                throw new CustomException("Score not found");
            }
            if (CheckExist.Status.Equals(GeneralStatusEnums.Inactive.ToString()))
            {
                throw new CustomException("Score is deleted!");
            }
            CheckExist.Status = GeneralStatusEnums.Inactive.ToString();
            foreach (var item in CheckExist.ScoreDetails)
            {
                item.Status = GeneralStatusEnums.Inactive.ToString();
            }
            await _scoreRepositories.Update(CheckExist);
            return new MessageResultModel
            {
                Message = "Ok",
            };
        }

        public async Task<MessageResultModel> RestoreScore(Guid Id)
        {
            var CheckExist = await _scoreRepositories.GetSingle(x => x.Id.Equals(Id), includeProperties: "ScoreDetails");
            if (CheckExist == null)
            {
                throw new CustomException("Score not found");
            }
            if (CheckExist.Status.Equals(GeneralStatusEnums.Active.ToString()))
            {
                throw new CustomException("Score is active!");
            }
            CheckExist.Status = GeneralStatusEnums.Active.ToString();
            foreach (var item in CheckExist.ScoreDetails)
            {
                item.Status = GeneralStatusEnums.Active.ToString();
            }
            await _scoreRepositories.Update(CheckExist);
            return new MessageResultModel
            {
                Message = "Ok",
            };
        }

        private async Task<List<Score>> ViewAllScore(int? pageIndex, ScoreFilter searchModel)
        {
            Func<IQueryable<Score>, IOrderedQueryable<Score>> orderBy = o => o.OrderBy(p => p.TestDateAt);
            Expression<Func<Score, bool>> filter = p => p.ClassId.Equals(searchModel.ClassId);

            if (searchModel != null)
            {
                if (searchModel.OrderCreatedAt.HasValue)
                {
                    if (searchModel.OrderCreatedAt.Value)
                    {
                        orderBy = orderBy.AndThen(q => q.OrderByDescending(p => p.CreatedAt));
                    }
                    else
                    {
                        orderBy = orderBy.AndThen(q => q.OrderBy(p => p.CreatedAt));
                    }
                }

                if (searchModel.TestDateAt.HasValue)
                {
                    filter = filter.And(t => t.TestDateAt.Equals(searchModel.TestDateAt));
                }
            }

             var allScore = await _scoreRepositories.GetList(filter, orderBy, includeProperties: "CreateByNavigation,ScoreDetails", pageIndex ?? 1);

            return allScore.ToList();
        }
    }
}
