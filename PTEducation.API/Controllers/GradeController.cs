using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTEducation.Business.Services.GradeServices;
using PTEducation.Data.DTO.RequestModel;

using Asp.Versioning;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/grade")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class GradeController : ControllerBase
    {
        private readonly IGradeServices _gradeServices;
        public GradeController(IGradeServices gradeServices)
        {
            _gradeServices = gradeServices;
        }

        [HttpGet]    
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAll()    
        {
            var result = await _gradeServices.GetAll();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([FromBody] GradeCreateReqModel req)
        {
            var result = await _gradeServices.Create(req);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] GradeUpdateReqModel req)
        {
            req.Id = id; // Ensure ID in URL matches payload or overrides it
            var result = await _gradeServices.Update(req);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _gradeServices.Delete(id);
            return Ok(result);
        }
    }
}
