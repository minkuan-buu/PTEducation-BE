using AutoMapper;
using Org.BouncyCastle.Tls;
using PTEducation.Business.ApplicationMiddleware;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Data.Entities;
using PTEducation.Data.Enums;

namespace PTEducation.Business.MapperProfiles
{
    public class MapperProfileConfiguration : Profile
    {
        public MapperProfileConfiguration()
        {
            CreateMap<User, UserLoginResModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.IsResetPassword, opt => opt.MapFrom(src => src.Status.Equals(AccountStatusEnums.ResetPassword.ToString())));
            CreateMap<UserRegisterReqModel, User>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    TextConvert.ConvertToUnicodeEscape(src.Name)));
            CreateMap<ClassCreateReqModel, Class>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src =>
                    DateTime.Now))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    GeneralStatusEnums.Active.ToString()));
            CreateMap<StudentsImportWithClass, User>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    GeneralStatusEnums.Active.ToString()));
            CreateMap<Class, ClassDetailResModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src =>
                    new ClassCreatedByModel
                    {
                        Id = src.Id,
                        Name = TextConvert.ConvertFromUnicodeEscape(src.CreatedByNavigation.Name),
                        Email = src.CreatedByNavigation.Email
                    }))
                .ForMember(dest => dest.Students, opt => opt.MapFrom(src =>
                    src.StudentClasses.Select(x => new StudentClassModel
                    {
                        Id = x.Id,
                        StudentCode = x.Student.Id,
                        Name = TextConvert.ConvertFromUnicodeEscape(x.Student.Name),
                        Email = x.Student.Email,
                        Phone = x.Student.Phone
                    }).ToList()));
            CreateMap<Class, ListClassResModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    src.Status))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src =>
                    new ClassCreatedByModel
                    {
                        Id = src.Id,
                        Name = TextConvert.ConvertFromUnicodeEscape(src.CreatedByNavigation.Name),
                        Email = src.CreatedByNavigation.Email
                    }))
                .ForMember(dest => dest.TotalStudent, opt => opt.MapFrom(src =>
                    src.StudentClasses.Count));
            CreateMap<User, ManagerResModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    TextConvert.ConvertFromUnicodeEscape(src.Name)));
            CreateMap<ManagerRegisterReqModel, User>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    TextConvert.ConvertToUnicodeEscape(src.Name)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    GeneralStatusEnums.Active.ToString()))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src =>
                    RoleEnums.Manager.ToString()))
                .ForMember(dest => dest.IsNeedResetPassword, opt => opt.MapFrom(src =>
                    true));
            CreateMap<Class, ClassListSelectResModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    TextConvert.ConvertFromUnicodeEscape(src.Name)));
            CreateMap<ScoreCreateReqModel, Score>()
                .ForMember(dest => dest.Shift, opt => opt.MapFrom(src =>
                    TextConvert.ConvertToUnicodeEscape(src.Shift)))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src =>
                    DateTime.Now))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    GeneralStatusEnums.Active.ToString()));
            CreateMap<ScoreDetailReqModel, ScoreDetail>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src =>
                    Guid.NewGuid()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    GeneralStatusEnums.Active.ToString()));
            CreateMap<Score, ScoreDetailResModel>()
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src =>
                    TextConvert.ConvertFromUnicodeEscape(src.Class.Name)))
                .ForMember(dest => dest.CreateBy, opt => opt.MapFrom(src => new ScoreCreatedByModel()
                {
                    Id = src.CreateByNavigation.Id,
                    Name = TextConvert.ConvertFromUnicodeEscape(src.CreateByNavigation.Name),
                    Email = src.CreateByNavigation.Email
                }))
                .ForMember(dest => dest.ScoreDetails, opt => opt.MapFrom(src =>
                    src.ScoreDetails.Select(x => new ScoreDetailStudentResModel
                    {
                        StudentClassId = x.StudentClassId,
                        Id = x.StudentClass.Student.Id,
                        Name = TextConvert.ConvertFromUnicodeEscape(x.StudentClass.Student.Name),
                        Score = x.Score,
                        Note = x.Note != null ? TextConvert.ConvertFromUnicodeEscape(x.Note) : null
                    }).ToList()));
            CreateMap<Score, ScoreListResModel>()
                .ForMember(dest => dest.CreateBy, opt => opt.MapFrom(src => new ScoreCreatedByModel()
                {
                    Id = src.CreateByNavigation.Id,
                    Name = TextConvert.ConvertFromUnicodeEscape(src.CreateByNavigation.Name),
                    Email = src.CreateByNavigation.Email
                }))
                .ForMember(dest => dest.AverageScore, opt => opt.MapFrom(src =>
                    src.ScoreDetails.Count == 0 ? 0 : src.ScoreDetails.Average(x => x.Score)));
            CreateMap<User, UserProfileResModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src =>
                    src.StudentClasses.Count == 0 ? null : TextConvert.ConvertFromUnicodeEscape(src.StudentClasses.First().Class.Name)));
            CreateMap<AttendanceCreateReqModel, Attendance>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => GeneralStatusEnums.Active.ToString()));
            CreateMap<Attendance, AttendanceListResModel>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => new AttendanceCreatedByModel()
                {
                    Id = src.CreatedByNavigation.Id,
                    Name = TextConvert.ConvertFromUnicodeEscape(src.CreatedByNavigation.Name),
                    Email = src.CreatedByNavigation.Email
                }));
            CreateMap<Attendance, AttendanceDetailResModel>()
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src =>
                    TextConvert.ConvertFromUnicodeEscape(src.Class.Name)))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => new AttendanceCreatedByModel()
                {
                    Id = src.CreatedByNavigation.Id,
                    Name = TextConvert.ConvertFromUnicodeEscape(src.CreatedByNavigation.Name),
                    Email = src.CreatedByNavigation.Email
                }))
                .ForMember(dest => dest.AttendanceDetails, opt => opt.MapFrom(src =>
                    src.AttendanceDetails.Select(x => new AttendanceDetailStudentResModel
                    {
                        StudentClassId = x.StudentClassId,
                        Id = x.StudentClass.Student.Id,
                        Name = TextConvert.ConvertFromUnicodeEscape(x.StudentClass.Student.Name),
                        AttendanceStatus = AttendanceEnums.Có_mặt.ToString()
                    }).ToList()));
        }
    }
}
