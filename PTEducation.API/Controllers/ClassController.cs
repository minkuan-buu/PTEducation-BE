using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Business.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using PTEducation.Business.Services.ClassServices;
using PTEducation.Data.DTO.ResponseModel;
using Microsoft.AspNetCore.Hosting;
using Xceed.Words.NET;
using Xceed.Document.NET;
using System.IO.Compression;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/class")]
    public class ClassController : ControllerBase
    {
        private readonly IClassServices _classServices;
        private readonly IWebHostEnvironment _env;
        public ClassController(IClassServices classServices, IWebHostEnvironment env)
        {
            _classServices = classServices;
            _env = env;
        }



        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
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
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
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

        [HttpGet("sheet/all")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var Result = await _classServices.GetClassList();
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("select/all")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
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
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
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
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
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
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
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

        [HttpDelete("delete")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> HardDeleteClass([FromBody] Guid Id)
        {
            try
            {
                var Result = await _classServices.HardDeleteClass(Id);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("restore")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
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
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
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

        [HttpPut("move-out")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> MoveOutStudent([FromBody] MoveOutStudentClassModel MoveOutReq)
        {
            try
            {
                var Result = await _classServices.MoveOutStudent(MoveOutReq);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetClassIdByName([FromQuery] string ClassName)
        {
            try
            {
                var Result = await _classServices.GetClassIdByName(ClassName);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/score/")]
        // [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetReportByClass(Guid id, [FromQuery] ScoreFromDateToDate ReqDate)
        {
            try
            {
                // 1. LẤY DỮ LIỆU
                List<ScoreStudentResModel> students = await _classServices.GetStudentScoreByClassIdAndRangeDate(id, ReqDate.FromDate, ReqDate.ToDate);

                if (students == null || !students.Any())
                {
                    return BadRequest(new { message = "Không tìm thấy dữ liệu điểm cho lớp này." });
                }

                // 2. XÁC ĐỊNH ĐƯỜNG DẪN FILE MẪU
                // Đảm bảo tên file mẫu trên server đúng là Template_Report.docx
                string templatePath = Path.Combine(_env.ContentRootPath, "Templates", "Template_Report.docx");

                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest(new { message = "Không tìm thấy file mẫu phiếu liên lạc (Template_Report.docx) trên server." });
                }

                // 3. GỌI HÀM XỬ LÝ WORD & ZIP
                byte[] zipFileBytes = GenerateStudentReportZip(students, templatePath);

                // 4. TRẢ VỀ FILE ZIP
                string fileName = $"BangDiem_Lop_{id}_{DateTime.Now:yyyyMMdd}.zip";
                return File(zipFileBytes, "application/zip", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- HÀM XỬ LÝ LOGIC (PRIVATE) ---
        private byte[] GenerateStudentReportZip(List<ScoreStudentResModel> students, string templatePath)
        {
            using (var compressedFileStream = new MemoryStream())
            {
                // Tạo file Zip
                using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, true))
                {
                    // Cache file mẫu vào RAM
                    byte[] templateBytes = System.IO.File.ReadAllBytes(templatePath);

                    foreach (var student in students)
                    {
                        // Tạo tên file an toàn
                        string safeName = ValidateFileName(student.Name ?? "NoName");
                        string fileName = $"{safeName}_{student.Id}.docx";

                        // Tạo Entry trong Zip
                        var zipEntry = zipArchive.CreateEntry(fileName);

                        using (var entryStream = zipEntry.Open())
                        using (var msWord = new MemoryStream())
                        {
                            // Load file mẫu từ RAM
                            using (var docStream = new MemoryStream(templateBytes))
                            using (var doc = DocX.Load(docStream))
                            {
                                // A. Thay thế thông tin Header
                                doc.ReplaceText("[Date]", DateTime.Now.ToString("dd/MM/yyyy"));
                                doc.ReplaceText("[StudentName]", student.Name ?? "");

                                // B. Đổ dữ liệu vào bảng
                                // [CẢNH BÁO]: Logic này đang lấy bảng ĐẦU TIÊN trong file Word.
                                // Nếu file mẫu có bảng ở Header (như hình), nó có thể lấy nhầm bảng.
                                // Nên kiểm tra kỹ file mẫu xem bảng điểm là bảng thứ mấy (Tables[0], Tables[1]...)
                                var table = doc.Tables.FirstOrDefault();

                                if (table != null && student.Scores != null)
                                {
                                    int stt = 1;
                                    foreach (var score in student.Scores)
                                    {
                                        // Thêm dòng mới vào cuối bảng
                                        var row = table.InsertRow();

                                        // [FIX 2] Sử dụng Alignment từ namespace đã import
                                        // Col 0: STT
                                        row.Cells[0].Paragraphs[0].Append(stt.ToString()).Alignment = Alignment.center;

                                        // Col 1: Ngày (Code hiện tại đang điền NGÀY)
                                        // CẢNH BÁO: Hình mẫu cột 1 là "Môn". Bạn cần xem lại dữ liệu trả về.
                                        row.Cells[1].Paragraphs[0].Append(score.TestDateAt.ToString("dd/MM/yyyy")).Alignment = Alignment.center;

                                        // Col 2: Ca (Code hiện tại đang điền CA)
                                        // CẢNH BÁO: Hình mẫu cột 2 là "TB HKI".
                                        row.Cells[2].Paragraphs[0].Append(score.Shift ?? "").Alignment = Alignment.center;

                                        // Col 3: Ghi chú
                                        string noteContent = $"Điểm: {score.Score}";
                                        if (!string.IsNullOrEmpty(score.Note))
                                        {
                                            noteContent += $" - {score.Note}";
                                        }
                                        // Kiểm tra xem bảng trong Word có đủ cột thứ 4 (index 3) không
                                        if (row.Cells.Count > 3)
                                        {
                                            row.Cells[3].Paragraphs[0].Append(noteContent);
                                        }

                                        stt++;
                                    }
                                }

                                doc.SaveAs(msWord);
                            }

                            msWord.Position = 0;
                            msWord.CopyTo(entryStream);
                        }
                    }
                }
                return compressedFileStream.ToArray();
            }
        }

        private string ValidateFileName(string name)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalidChars)
            {
                name = name.Replace(c.ToString(), "");
            }
            return name.Trim();
        }
    }
}
