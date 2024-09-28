using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PTEducation.API.Middleware;
using PTEducation.Business.MapperProfiles;
using PTEducation.Business.Services.AttendanceDetailServices;
using PTEducation.Business.Services.AttendanceServices;
using PTEducation.Business.Services.AuthServices;
using PTEducation.Business.Services.ClassServices;
using PTEducation.Business.Services.ScoreDetailServices;
using PTEducation.Business.Services.ScoreServices;
using PTEducation.Business.Services.StudentClassServices;
using PTEducation.Business.Services.StudentServices;
using PTEducation.Business.Services.UserServices;
using PTEducation.Business.Ultilities.Email;
using PTEducation.Data.Entities;
using PTEducation.Data.Repositories.AttendanceDetailRepositories;
using PTEducation.Data.Repositories.AttendanceRepositories;
using PTEducation.Data.Repositories.ClassRepositories;
using PTEducation.Data.Repositories.GenericRepositories;
using PTEducation.Data.Repositories.ScoreDetailRepositories;
using PTEducation.Data.Repositories.ScoreRepositories;
using PTEducation.Data.Repositories.StudentClassRepositories;
using PTEducation.Data.Repositories.UserRepositories;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//========================================== SWAGGER ==============================================

builder.Services.AddSwaggerGen(c =>
{

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "PTEducation.API",
        Description = "PT Education"
    });
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
});

builder.Services.AddDbContext<PteducationContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging();
}
);
//========================================== MAPPER ===============================================

builder.Services.AddAutoMapper(typeof(MapperProfileConfiguration).Assembly);

//========================================== MIDDLEWARE ===========================================

builder.Services.AddSingleton<GlobalExceptionMiddleware>();
builder.Services.AddControllers()
        .AddJsonOptions(options => {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

//========================================== REPOSITORY ===========================================

builder.Services.AddTransient<IUserRepositories, UserRepositories>();
builder.Services.AddTransient<IClassRepositories, ClassRepositories>();
builder.Services.AddTransient<IStudentClassRepositories, StudentClassRepositories>();
builder.Services.AddTransient<IScoreRepositories, ScoreRepositories>();
builder.Services.AddTransient<IScoreDetailRepositories, ScoreDetailRepositories>();
builder.Services.AddTransient<IAttendanceRepositories, AttendanceRepositories>();
builder.Services.AddTransient<IAttendanceDetailRepositories, AttendanceDetailRepositories>();
builder.Services.AddScoped(typeof(IGenericRepositories<>), typeof(GenericRepositories<>));

//=========================================== SERVICE =============================================

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

//=========================================== CORS ================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAll", policy =>
    {
        policy
        //.WithOrigins("http://tradiem.pteducation.edu.vn")
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
        //.AllowCredentials();
    });
});

//========================================== AUTHENTICATION =======================================

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = "IssuerFromServerhttp://api.pteducation.edu.vn",
            ValidAudience = "AudienceForhttp://tradiem.pteducation.edu.vn",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("TestingIssuerSigningKeyPTEducationMS@123")),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
        };
    });

//===================================================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication(); // Thêm middleware Authentication

app.UseAuthorization();

app.MapControllers();

app.Run();
