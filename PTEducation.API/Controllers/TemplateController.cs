using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Business.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using PTEducation.Business.Services.StudentClassServices;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/template")]
    public class TemplateController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly IStudentClassServices _studentClassServices;
        public TemplateController(IUserServices userServices, IStudentClassServices studentClassServices)
        {
            _userServices = userServices;
            _studentClassServices = studentClassServices;
        }

        [HttpGet("import-student")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult GetImportStudentTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ImportStudents");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "ID";
                worksheet.Cell(currentRow, 2).Value = "Name";
                worksheet.Cell(currentRow, 3).Value = "Email";
                worksheet.Cell(currentRow, 4).Value = "Phone";
                worksheet.Range(currentRow, 1, currentRow, 4).Style.Font.Bold = true;
                worksheet.Range(currentRow, 1, currentRow, 4).Style.Fill.BackgroundColor = XLColor.Yellow;
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Students.xlsx");
                }
            }
        }

        [HttpGet("import-score")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetImportScoreTemplate(Guid ClassId)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ScoreStudents");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "StudentClassID";
                worksheet.Cell(currentRow, 2).Value = "Id";
                worksheet.Cell(currentRow, 3).Value = "Name";
                worksheet.Cell(currentRow, 4).Value = "Score";
                worksheet.Range(currentRow, 1, currentRow, 4).Style.Font.Bold = true;
                worksheet.Range(currentRow, 1, currentRow, 4).Style.Fill.BackgroundColor = XLColor.Yellow;
                var ListStudents = await _studentClassServices.GetStudentInClass(ClassId);
                foreach (var student in ListStudents)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = student.Id.ToString();
                    worksheet.Cell(currentRow, 2).Value = student.Student.Id.ToString();
                    worksheet.Cell(currentRow, 3).Value = student.Student.Name;
                    worksheet.Cell(currentRow, 4).Value = 0;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StudentsScore.xlsx");
                }
            }
        }
        [HttpGet("import-attendance")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult GetImportAttendanceTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ImportAttendance");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "ID";
                worksheet.Range(currentRow, 1, currentRow, 1).Style.Font.Bold = true;
                worksheet.Range(currentRow, 1, currentRow, 1).Style.Fill.BackgroundColor = XLColor.Yellow;
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "StudentsAttendance.xlsx");
                }
            }
        }
    }
}
