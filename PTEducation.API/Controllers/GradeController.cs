// using PTEducation.Data.DTO.Custom;
// using PTEducation.Data.DTO.RequestModel;
// using PTEducation.Business.Services.UserServices;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using PTEducation.Business.Services.AuthServices;
// using PTEducation.Business.Services.StudentClassServices;

// namespace PTEducation.API.Controllers
// {
//     [ApiController]
//     [Route("api/grade/")]
//     public class GradeController : ControllerBase
//     {
//         private readonly IGradeServices _gradeServices;
//         public GradeController(IGradeServices gradeServices)
//         {
//             _gradeServices = gradeServices;
//         }

//         [HttpGet("{grade}")]    
//         [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
//         public async Task<IActionResult> GetAll()    
//         {
//             var Result = await _gradeServices.GetAll();
//             return Ok(Result);
//         }
//     }
// }
