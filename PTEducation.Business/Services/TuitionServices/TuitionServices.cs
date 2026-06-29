using AutoMapper;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.GradeRepositories;
using PTEducation.Data.Repositories.TuitionRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Business.Services.TuitionServices
{
    public class TuitionServices : ITuitionServices
    {
        private readonly ITuitionPeriodRepositories _tuitionPeriodRepositories;
        private readonly IStudentTuitionRepositories _studentTuitionRepositories;
        private readonly IGradeRepositories _gradeRepositories;
        private readonly IMapper _mapper;

        public TuitionServices(ITuitionPeriodRepositories tuitionPeriodRepositories, IStudentTuitionRepositories studentTuitionRepositories, IGradeRepositories gradeRepositories, IMapper mapper)
        {
            _tuitionPeriodRepositories = tuitionPeriodRepositories;
            _studentTuitionRepositories = studentTuitionRepositories;
            _gradeRepositories = gradeRepositories;
            _mapper = mapper;
        }

        public async Task<MessageResultModel> AddTuition(TuitionCreateReqModel tuitionCreateReqModel, string userId)
        {
            var CheckGradeExist = await _gradeRepositories.GetSingle(x => x.Id == tuitionCreateReqModel.GradeId);
            if (CheckGradeExist == null)
            {
                return new MessageResultModel
                {
                    Message = "Không tìm thấy khối lớp học!",
                };
            }
            var CheckExist = await _tuitionPeriodRepositories.GetSingle(x => x.Title == tuitionCreateReqModel.Title && x.GradeId == tuitionCreateReqModel.GradeId && x.FromDate == tuitionCreateReqModel.FromDate && x.ToDate == tuitionCreateReqModel.ToDate);
            if (CheckExist != null)
            {
                return new MessageResultModel
                {
                    Message = "Đã tồn tại học phí " + tuitionCreateReqModel.Title + "!",
                };
            }

            var tuitionPeriod = _mapper.Map<TuitionPeriod>(tuitionCreateReqModel);
            tuitionPeriod.CreatedBy = userId;
            await _tuitionPeriodRepositories.Insert(tuitionPeriod);
            return new MessageResultModel
            {
                Message = "Đã thêm " + tuitionCreateReqModel.Title + " thành công!",
            };
        }
    }
}
