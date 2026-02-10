using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Business.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using PTEducation.Business.Services.ClassServices;
using PTEducation.Data.DTO.ResponseModel;
using Microsoft.AspNetCore.Hosting;
using MiniSoftware;
using System.IO.Compression;
using PTEducation.Business.ApplicationMiddleware;

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

        [HttpPost("{id}/score/")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetReportByClass(Guid id, [FromQuery] ScoreFromDateToDate ReqDate, [FromBody] ScoreExportCommentReqModel commentReqModel)
        {
            try
            {
                // --- SỬA LỖI Ở ĐÂY ---
                // Thay vì DateTime.MinValue (năm 0001), ta dùng 01/01/1753 (Min của SQL)
                DateTime fromDateParam = ReqDate.FromDate ?? new DateTime(1753, 1, 1);

                // Thay vì DateTime.MaxValue, ta dùng 31/12/9999 cho an toàn tuyệt đối
                DateTime toDateParam = ReqDate.ToDate ?? new DateTime(9999, 12, 31);
                // ---------------------

                // 1. LẤY DỮ LIỆU
                var resultExport = await _classServices.GetStudentScoreByClassIdAndRangeDate(id, fromDateParam, toDateParam);

                if (resultExport == null || !resultExport.StudentData.Any())
                {
                    return BadRequest(new { message = "Không tìm thấy dữ liệu điểm cho lớp này." });
                }

                // 2. XÁC ĐỊNH ĐƯỜNG DẪN FILE MẪU
                string templatePath = Path.Combine(_env.ContentRootPath, "Templates", "Template_Report.docx");

                if (!System.IO.File.Exists(templatePath))
                {
                    return BadRequest(new { message = "Không tìm thấy file mẫu phiếu liên lạc (Template_Report.docx) trên server." });
                }

                // 3. GỌI HÀM XỬ LÝ WORD & ZIP
                byte[] zipFileBytes = GenerateStudentReportZip(resultExport.StudentData, templatePath, ReqDate, commentReqModel);

                // 4. TRẢ VỀ FILE ZIP
                string fileName = $"PhieuLienLac_Lop_{resultExport.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
                return File(zipFileBytes, "application/zip", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- HÀM XỬ LÝ LOGIC (DÙNG MINIWORD) ---

        private byte[] GenerateStudentReportZip(List<ScoreStudentResModel> students, string templatePath, ScoreFromDateToDate ReqDate, ScoreExportCommentReqModel commentReqModel)
        {
            using (var compressedFileStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, true))
                {
                    byte[] templateBytes = System.IO.File.ReadAllBytes(templatePath);

                    // --- [BƯỚC 1] TÍNH TOÁN NGÀY THÁNG HIỂN THỊ CHUNG ---
                    var allScores = students.SelectMany(s => s.Scores).ToList();
                    DateTime displayFrom;
                    DateTime displayTo;

                    if (allScores.Any())
                    {
                        displayFrom = allScores.Min(s => s.TestDateAt);
                        displayTo = allScores.Max(s => s.TestDateAt);
                    }
                    else
                    {
                        displayFrom = ReqDate.FromDate ?? DateTime.Now;
                        displayTo = ReqDate.ToDate ?? DateTime.Now;
                    }

                    string dateRangeString = $"{displayFrom:dd/MM/yyyy} - {displayTo:dd/MM/yyyy}";

                    // --- [BƯỚC 2] TẠO FILE WORD CHO TỪNG HỌC SINH ---
                    foreach (var student in students)
                    {
                        // [SỬA Ở ĐÂY]: Đổi từ double sang decimal
                        decimal averageScore = 0;
                        string autoComment = "";

                        // Lấy danh sách điểm
                        // (Lưu ý: đảm bảo s.Score trong Model ScoreStudentResModel cũng là decimal hoặc float)
                        var scoresList = student.Scores.Select(s => s.Score).ToList();

                        if (scoresList.Any())
                        {
                            // Hàm Average() của List<decimal> sẽ trả về decimal -> Không còn lỗi convert
                            averageScore = scoresList.Where(x => x > 0).Average();

                            // Làm tròn 2 chữ số thập phân
                            averageScore = Math.Round(averageScore, 2);

                            // So sánh (Cả 2 vế đều là decimal nên so sánh được ngay)
                            if (averageScore < commentReqModel.PointMilestone1)
                            {
                                autoComment = commentReqModel.CommentLow;
                            }
                            else if (averageScore < commentReqModel.PointMilestone2)
                            {
                                autoComment = commentReqModel.CommentMid;
                            }
                            else
                            {
                                autoComment = commentReqModel.CommentHigh;
                            }
                        }
                        else
                        {
                            autoComment = "Chưa có dữ liệu điểm để nhận xét.";
                        }

                        // B. Chuẩn bị dữ liệu đổ vào Template Word
                        var value = new Dictionary<string, object>()
                        {
                            ["Date"] = dateRangeString,
                            ["StudentName"] = student.Name ?? "No Name",
                            ["AverageScore"] = averageScore, // Truyền thêm điểm TB nếu muốn hiện trong Word
                            ["TeacherComment"] = autoComment, // <--- BIẾN QUAN TRỌNG: Nhận xét tự động
                            ["Scores"] = student.Scores.Select((s, index) => new Dictionary<string, object>
                            {
                                ["STT"] = index + 1,
                                ["TestDateAt"] = s.TestDateAt.ToString("dd/MM/yyyy"),
                                ["Shift"] = s.Shift ?? "",
                                ["NoteContent"] = $"Điểm: {s.Score}{(s.Score == 0 ? " - Vắng" : (!string.IsNullOrEmpty(s.Note) ? $" - {TextConvert.ConvertFromUnicodeEscape(s.Note)}" : ""))}"
                            }).ToList()
                        };

                        using (var msWord = new MemoryStream())
                        {
                            MiniWord.SaveAsByTemplate(msWord, templateBytes, value);
                            string safeName = ValidateFileName(student.Name ?? "NoName");
                            string fileName = $"{safeName}_{student.Id}.docx";

                            var zipEntry = zipArchive.CreateEntry(fileName);
                            using (var entryStream = zipEntry.Open())
                            {
                                msWord.Position = 0;
                                msWord.CopyTo(entryStream);
                            }
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
