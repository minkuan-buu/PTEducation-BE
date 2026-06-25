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
            CreateMap(typeof(DataResultModel<>), typeof(DataResultModel<>));
            CreateMap<RawUserLoginResModel, UserLoginResModel>();
            CreateMap<User, RawUserLoginResModel>()
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
            CreateMap<ClassCreateReqModelV2, Class>()
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
                        Id = src.CreatedByNavigation.Id,
                        Name = src.CreatedByNavigation.Name,
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
                .ForMember(dest => dest.WeeklySchedules, opt => opt.MapFrom(src =>
                    src.ClassSchedules
                        .Where(x => x.Status.Equals(GeneralStatusEnums.Active.ToString()))
                        .Select(x => new ClassScheduleResModel
                        {
                            DayOfWeek = x.DayOfWeek,
                            StartTime = x.StartTime,
                            EndTime = x.EndTime
                        }).ToList()))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src =>
                    new ClassCreatedByModel
                    {
                        Id = src.CreatedByNavigation.Id,
                        Name = src.CreatedByNavigation.Name,
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
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>src.Name))
                .ForMember(dest => dest.WeeklySchedules, opt => opt.MapFrom(src => src.ClassSchedules.Select(x => new ClassScheduleResModel
                {
                    DayOfWeek = x.DayOfWeek,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime
                }).ToList()));
            CreateMap<ClassSchedule, ClassScheduleResModel>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime));
            CreateMap<ScoreCreateReqModel, Score>()
                .ForMember(dest => dest.Shift, opt => opt.MapFrom(src =>
                    src.Shift != null ? TextConvert.ConvertToUnicodeEscape(src.Shift) : null))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src =>
                    DateTime.Now))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    GeneralStatusEnums.Active.ToString()));
            CreateMap<ScoreDetailReqModel, ScoreDetail>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src =>
                    Guid.NewGuid()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    GeneralStatusEnums.Active.ToString()))
                .ForMember(dest => dest.Note, opt => opt.MapFrom(src =>
                    src.Note != null ? TextConvert.ConvertToUnicodeEscape(src.Note) : null));
            CreateMap<Score, ScoreDetailResModel>()
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src =>
                    TextConvert.ConvertFromUnicodeEscape(src.Class.Name)))
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
                .ForMember(dest => dest.AverageScore, opt => opt.MapFrom(src =>
                    src.ScoreDetails.Count == 0 ? 0 : src.ScoreDetails.Average(x => x.Score)));
            CreateMap<User, UserProfileResModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    TextConvert.ConvertFromUnicodeEscape(src.Name)))
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src =>
                    src.StudentClasses.Count == 0 ? null : TextConvert.ConvertFromUnicodeEscape(src.StudentClasses.First().Class.Name)));
            CreateMap<AttendanceCreateReqModel, Attendance>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => AttendanceStatusEnums.Pending.ToString()))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.Date)));
            CreateMap<Attendance, AttendanceListResModel>();
            CreateMap<Attendance, AttendanceDetailResModel>()
                .ForMember(dest => dest.Session, opt => opt.MapFrom(src =>
                    new AttendanceDetailSessionResModel
                    {
                        Id = src.Id,
                        Date = src.Date,
                        StartTime = src.StartTime,
                        EndTime = src.EndTime,
                        ClassScheduleId = src.ClassScheduleId,
                        SessionType = src.SessionType,
                        Note = src.Note != null ? TextConvert.ConvertFromUnicodeEscape(src.Note) : null,
                        Status = src.Status
                    }))
                .ForMember(dest => dest.AttendanceDetails, opt => opt.MapFrom(src =>
                    src.AttendanceDetailAttendances.Select(x => new AttendanceDetailStudentResModel
                    {
                        StudentClassId = x.StudentClassId,
                        StudentId = x.StudentClass.Student.Id,
                        StudentName = TextConvert.ConvertFromUnicodeEscape(x.StudentClass.Student.Name),
                        AttendanceStatus = x.Status,
                        Guardians = x.StudentClass.Student.StudentGuardianStudents.Select(g => new UserGuardianListResModel
                        {
                            Id = g.Guardian.Id,
                            Name = g.Guardian.Name,
                            Email = g.Guardian.Email,
                            Phone = g.Guardian.Phone,
                            Relationship = g.Relationship,
                            IsPrimary = g.IsPrimary
                        }).ToList()
                    }).ToList()));
            CreateMap<User, UserListResModel>()
                .ForMember(dest => dest.ClassName, opt => opt.MapFrom(src =>
                    src.StudentClasses.Count == 0 ? null : src.StudentClasses.First().Class.Name))
                .ForMember(dest => dest.Guardians, opt => opt.MapFrom(src =>
                    src.StudentGuardianStudents.Select(x => new UserGuardianListResModel
                    {
                        Id = x.Guardian.Id,
                        Name = x.Guardian.Name,
                        Email = x.Guardian.Email,
                        Phone = x.Guardian.Phone,
                        Relationship = x.Relationship,
                        IsPrimary = x.IsPrimary
                    }).ToList()));
            CreateMap<StudentClass, StudentInClassResModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Student.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    TextConvert.ConvertFromUnicodeEscape(src.Student.Name)))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Student.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Student.Phone))
                .ForMember(dest => dest.Guardians, opt => opt.MapFrom(src =>
                    src.Student.StudentGuardianStudents.Select(x => new UserGuardianListResModel
                    {
                        Id = x.Guardian.Id,
                        Name = x.Guardian.Name,
                        Email = x.Guardian.Email,
                        Phone = x.Guardian.Phone,
                        Relationship = x.Relationship,
                        IsPrimary = x.IsPrimary
                    }).ToList()));

            CreateMap<Attendance, AttendanceSessionResModel>();
        }
    }
}
