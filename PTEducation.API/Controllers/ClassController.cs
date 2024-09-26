using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Business.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using PTEducation.Business.Services.ClassServices;
using PTEducation.Data.DTO.ResponseModel;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/class")]
    public class ClassController : ControllerBase
    {
        private readonly IClassServices _classServices;
        public ClassController(IClassServices classServices)
        {
            _classServices = classServices;
        }



        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetClassDetail(Guid id)
        {
            try
            {
                var Result = await _classServices.GetClassDetail(id);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetList(int? pageIndex, [FromQuery] ClassFilter searchModel)
        {
            try
            {
                var Result = await _classServices.GetClassList(pageIndex, searchModel);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("select/all")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetListSelect()
        {
            try
            {
                var Result = await _classServices.GetClassSelectList();
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateClass([FromBody] ClassCreateReqModel ClassReq)
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var Result = await _classServices.CreateClass(ClassReq, token);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateClass([FromBody] ClassUpdateReqModel ClassReq)
        {
            try
            {
                var Result = await _classServices.UpdateClass(ClassReq);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("delete")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> SoftDeleteClass([FromBody] Guid Id)
        {
            try
            {
                var Result = await _classServices.SoftDeleteClass(Id);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("restore")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RestoreClass([FromBody] Guid Id)
        {
            try
            {
                var Result = await _classServices.RestoreClass(Id);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("add-student")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ManualAddStudentClass([FromBody] ManualAddStudentClassModel AddStudentsReq)
        {
            try
            {
                var Result = await _classServices.ManualAddStudent(AddStudentsReq);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
