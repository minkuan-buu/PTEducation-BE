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
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
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

        [HttpGet("import-manager")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public IActionResult GetImportManagerTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ImportManagers");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Name";
                worksheet.Cell(currentRow, 2).Value = "Email";
                worksheet.Cell(currentRow, 3).Value = "Phone";
                worksheet.Range(currentRow, 1, currentRow, 3).Style.Font.Bold = true;
                worksheet.Range(currentRow, 1, currentRow, 3).Style.Fill.BackgroundColor = XLColor.Yellow;
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Managers.xlsx");
                }
            }
        }

        [HttpGet("import-score")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
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
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetImportAttendanceTemplate(Guid ClassId)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ImportAttendance");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "ID";
                worksheet.Range(currentRow, 1, currentRow, 1).Style.Font.Bold = true;
                worksheet.Range(currentRow, 1, currentRow, 1).Style.Fill.BackgroundColor = XLColor.Yellow;
                var mappingsheet = workbook.Worksheets.Add("MappingStudents");
                var ListStudents = await _studentClassServices.GetStudentInClass(ClassId);
                mappingsheet.Cell(1, 1).Value = "ID";
                mappingsheet.Cell(1, 2).Value = "Name";
                mappingsheet.Cell(1, 3).Value = "Email";
                mappingsheet.Cell(1, 4).Value = "Phone";
                mappingsheet.Range(1, 1, 1, 4).Style.Font.Bold = true;
                mappingsheet.Range(1, 1, 1, 4).Style.Fill.BackgroundColor = XLColor.Yellow;
                var mappingRow = 1;
                foreach (var student in ListStudents)
                {
                    mappingRow++;
                    mappingsheet.Cell(mappingRow, 1).Value = student.StudentId;
                    mappingsheet.Cell(mappingRow, 2).Value = student.Student.Name;
                    mappingsheet.Cell(mappingRow, 3).Value = student.Student.Email;
                    mappingsheet.Cell(mappingRow, 4).Value = student.Student.Phone;
                }
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
