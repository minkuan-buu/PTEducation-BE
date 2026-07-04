using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.SwaggerGen;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using PTEducation.API.Middleware;
using PTEducation.API.Hubs;
using PTEducation.API.Swagger;
using PTEducation.API.Realtime;
using PTEducation.API.Serialization;
using PTEducation.Business.MapperProfiles;
using PTEducation.Business.Services.AttendanceDetailServices;
using PTEducation.Business.Services.AttendanceServices;
using PTEducation.Business.Services.AuthServices;
using PTEducation.Business.Services.ClassServices;
using PTEducation.Business.Services.OTPServices;
using PTEducation.Business.Services.OverviewServices;
using PTEducation.Business.Services.ScoreDetailServices;
using PTEducation.Business.Services.ScoreServices;
using PTEducation.Business.Services.StudentClassServices;
using PTEducation.Business.Services.StudentServices;
using PTEducation.Business.Services.UserServices;
using PTEducation.Business.Services.StorageServices;
using PTEducation.Business.Services.TuitionServices;
using PTEducation.Business.Ultilities.Email;
using PTEducation.API.HostedServices;
using PTEducation.Data.Entities;
using Quartz;
using PTEducation.Data.Repositories.AttendanceDetailRepositories;
using PTEducation.Data.Repositories.AttendanceRepositories;
using PTEducation.Data.Repositories.ClassRepositories;
using PTEducation.Data.Repositories.GenericRepositories;
using PTEducation.Data.Repositories.OTPRepositories;
using PTEducation.Data.Repositories.ScoreDetailRepositories;
using PTEducation.Data.Repositories.ScoreRepositories;
using PTEducation.Data.Repositories.StudentClassRepositories;
using PTEducation.Data.Repositories.UserRepositories;
using System.Text;
using System.Text.Json.Serialization;
using PTEducation.Data.Repositories.StudentGuardianRepositories;
using PTEducation.Data.Repositories.ChatRepositories;
using PTEducation.Data.Repositories.GradeRepositories;
using PTEducation.Data.Repositories.TuitionRepositories;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

//========================================== SWAGGER ==============================================

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. " +
                            "\n\nEnter your token in the text input below. " +
                              "\n\nExample: '12345abcde'",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });

    c.MapType<TimeOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "time",
        Example = new OpenApiString("08:00:00")
    });
});
//========================================== DATABASE =============================================

var rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(rawConnectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

var StudentDefaultPassword = Environment.GetEnvironmentVariable("STUDENT_DEFAULT_PASSWORD");
if (string.IsNullOrEmpty(StudentDefaultPassword))
{
    throw new InvalidOperationException("Student default password is not configured.");
}
var AdminDefaultPassword = Environment.GetEnvironmentVariable("ADMIN_DEFAULT_PASSWORD");
if (string.IsNullOrEmpty(AdminDefaultPassword))
{
    throw new InvalidOperationException("Admin default password is not configured.");
}

var connectionString = rawConnectionString
    .Replace("${DB_HOST}", Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost")
    .Replace("${DB_PORT}", Environment.GetEnvironmentVariable("DB_PORT") ?? "1433")
    .Replace("${DB_USER}", Environment.GetEnvironmentVariable("DB_USER") ?? "sa")
    .Replace("${DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "your_password")
    .Replace("${DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME") ?? "PTEducation");

builder.Services.AddDbContext<PteducationContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.EnableSensitiveDataLogging();
}
);
//========================================== VERSIONING ===========================================
builder.Services.AddApiVersioning(options =>
{
    // Nếu client không truyền version thì dùng mặc định v1.0
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);

    // Trả về các version hỗ trợ qua response header
    options.ReportApiVersions = true;

    // Hỗ trợ đọc version từ nhiều nguồn
    options.ApiVersionReader = ApiVersionReader.Combine(
        new HeaderApiVersionReader("x-api-version"),      // Header: x-api-version: 1.0
        new MediaTypeApiVersionReader("x-api-version"),   // Accept: application/json; x-api-version=1.0
        new UrlSegmentApiVersionReader()                    // /api/v1/users
    );
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";    // v1, v1.0, v2 ...
    options.SubstituteApiVersionInUrl = true;
});
//========================================== MAPPER ===============================================

builder.Services.AddAutoMapper(typeof(MapperProfileConfiguration).Assembly);

//========================================== MIDDLEWARE ===========================================

builder.Services.AddSingleton<GlobalExceptionMiddleware>();
builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
        });

//========================================== REPOSITORY ===========================================

builder.Services.AddTransient<IUserRepositories, UserRepositories>();
builder.Services.AddTransient<IClassRepositories, ClassRepositories>();
builder.Services.AddTransient<IStudentClassRepositories, StudentClassRepositories>();
builder.Services.AddTransient<IScoreRepositories, ScoreRepositories>();
builder.Services.AddTransient<IScoreDetailRepositories, ScoreDetailRepositories>();
builder.Services.AddTransient<IAttendanceRepositories, AttendanceRepositories>();
builder.Services.AddTransient<IAttendanceDetailRepositories, AttendanceDetailRepositories>();
builder.Services.AddTransient<IStudentGuardianRepositories, StudentGuardianRepositories>();
builder.Services.AddTransient<IOTPRepositories, OTPRepositories>();
builder.Services.AddTransient<IChatRepositories, ChatRepositories>();
builder.Services.AddTransient<IChatDetailRepositories, ChatDetailRepositories>();
builder.Services.AddTransient<IChatMessageRepositories, ChatMessageRepositories>();
builder.Services.AddTransient<IGradeRepositories, GradeRepositories>();
builder.Services.AddTransient<ITuitionPeriodRepositories, TuitionPeriodRepositories>();
builder.Services.AddTransient<IStudentTuitionRepositories, StudentTuitionRepositories>();
builder.Services.AddScoped(typeof(IGenericRepositories<>), typeof(GenericRepositories<>));

//=========================================== SERVICE =============================================

// Cloudflare R2 Configurations Option binding
builder.Services.Configure<CloudflareR2Settings>(options =>
{
    var section = builder.Configuration.GetSection("CloudflareR2");
    options.AccessKeyId = Environment.GetEnvironmentVariable("R2_ACCESS_KEY_ID") ?? section["AccessKeyId"] ?? string.Empty;
    options.SecretAccessKey = Environment.GetEnvironmentVariable("R2_SECRET_ACCESS_KEY") ?? section["SecretAccessKey"] ?? string.Empty;
    options.ServiceUrl = Environment.GetEnvironmentVariable("R2_SERVICE_URL") ?? section["ServiceUrl"] ?? string.Empty;
    options.BucketName = Environment.GetEnvironmentVariable("R2_BUCKET_NAME") ?? section["BucketName"] ?? string.Empty;
    options.PublicUrl = Environment.GetEnvironmentVariable("R2_PUBLIC_URL") ?? section["PublicUrl"] ?? string.Empty;

    // Bỏ qua giá trị nếu nó là chuỗi placeholder dạng ${VAR_NAME}
    if (options.AccessKeyId.StartsWith("${")) options.AccessKeyId = string.Empty;
    if (options.SecretAccessKey.StartsWith("${")) options.SecretAccessKey = string.Empty;
    if (options.ServiceUrl.StartsWith("${")) options.ServiceUrl = string.Empty;
    if (options.BucketName.StartsWith("${")) options.BucketName = string.Empty;
    if (options.PublicUrl.StartsWith("${")) options.PublicUrl = string.Empty;
});

builder.Services.AddScoped<IStorageServices, StorageServices>();
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IClassServices, ClassServices>();
builder.Services.AddScoped<IScoreServices, ScoreServices>();
builder.Services.AddScoped<IScoreDetailServices, ScoreDetailServices>();
builder.Services.AddScoped<IStudentClassServices, StudentClassServices>();
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.AddScoped<IStudentServices, StudentServices>();
builder.Services.AddScoped<IEmail, Email>();
builder.Services.AddScoped<IAttendanceServices, AttendanceServices>();
builder.Services.AddScoped<IAttendanceDetailServices, AttendanceDetailServices>();
builder.Services.AddScoped<IOTPServices, OTPServices>();
builder.Services.AddScoped<IOverviewServices, OverviewServices>();
builder.Services.AddScoped<IAttendanceRealtimeNotifier, AttendanceRealtimeNotifier>();
builder.Services.AddScoped<ITuitionServices, TuitionServices>();
builder.Services.AddScoped<PTEducation.Business.Services.AttendanceServices.IAttendanceScheduler, PTEducation.API.Scheduling.AttendanceScheduler>();

// Quartz scheduler
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
    // jobs will be scheduled dynamically when attendances are created/updated
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddScoped<PTEducation.API.Jobs.AttendanceWindowJob>();

builder.Services.AddHostedService<AdminInitializerHostedService>();
builder.Services.AddHostedService<DatabaseMigrationHostedService>();
builder.Services.AddHostedService<AttendanceWindowReconciliationHostedService>();

//=========================================== CORS ================================================

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAllOrigin", policy =>
    {
        policy
            .WithOrigins(allowedOrigins!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition", "X-Access-Token");
    });
});


//========================================== AUTHENTICATION =======================================

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer("Bearer", options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidIssuer = "IssuerFromServerhttp://api.pteducation.edu.vn",
//            ValidAudience = "AudienceForhttp://tradiem.pteducation.edu.vn",
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TestingIssuerSigningKeyPTEducationMS@123")),
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateIssuerSigningKey = true,
//            ValidateLifetime = true,
//        };
//    });

builder.Services.AddAuthentication("PTEducationAuthentication")
    .AddScheme<AuthenticationSchemeOptions, AuthorizeMiddleware>("PTEducationAuthentication", null);

//===================================================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseCors("AllowAllOrigin");

app.UseHttpsRedirection();

app.UseAuthentication(); // Th�m middleware Authentication

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthorization();

app.MapControllers();
app.MapHub<AttendanceHub>("/hubs/attendance");

app.Run();
