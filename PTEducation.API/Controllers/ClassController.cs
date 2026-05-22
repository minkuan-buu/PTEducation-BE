using Asp.Versioning;
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
    [Route("api/v{version:apiVersion}/classes")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class ClassController : ControllerBase
    {
        private readonly IClassServices _classServices;
        private readonly IWebHostEnvironment _env;
        public ClassController(IClassServices classServices, IWebHostEnvironment env)
        {
            _classServices = classServices;
            _env = env;
        }



        [HttpGet("{id:guid}")]
        [MapToApiVersion("1.0")]
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

        [HttpGet]
        [MapToApiVersion("1.0")]
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

        [HttpGet("sheet")]
        [MapToApiVersion("1.0")]
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

        // [HttpGet("select")]
        // [MapToApiVersion("1.0")]
        // [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        // public async Task<IActionResult> GetListSelect()
        // {
        //     try
        //     {
        //         var Result = await _classServices.GetClassSelectList();
        //         return Ok(Result);
        //     }
        //     catch (CustomException ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        [HttpGet("select")]
        [MapToApiVersion("2.0")]
        [AllowAnonymous]
        public async Task<IActionResult> GetListSelectV2()
        {
            try
            {
                var result = await _classServices.GetClassSelectList();
                return Ok(result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [MapToApiVersion("1.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateClass([FromBody] ClassCreateReqModel ClassReq)
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new { message = "User is not authenticated." });
                }

                var Result = await _classServices.CreateClass(ClassReq, userId);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [MapToApiVersion("2.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateClassV2([FromBody] ClassCreateReqModelV2 ClassReq)
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new { message = "User is not authenticated." });
                }

                var Result = await _classServices.CreateClassV2(ClassReq, userId);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        [MapToApiVersion("1.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateClass(Guid id, [FromBody] ClassUpdateReqModel ClassReq)
        {
            try
            {
                ClassReq.Id = id;
                var Result = await _classServices.UpdateClass(ClassReq);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        [MapToApiVersion("1.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> SoftDeleteClass(Guid id)
        {
            try
            {
                var Result = await _classServices.SoftDeleteClass(id);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}/hard")]
        [MapToApiVersion("1.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> HardDeleteClass(Guid id)
        {
            try
            {
                var Result = await _classServices.HardDeleteClass(id);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id:guid}/restore")]
        [MapToApiVersion("1.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> RestoreClass(Guid id)
        {
            try
            {
                var Result = await _classServices.RestoreClass(id);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/students")]
        [MapToApiVersion("1.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> ManualAddStudentClass(Guid id, [FromBody] ManualAddStudentClassModel AddStudentsReq)
        {
            try
            {
                AddStudentsReq.Id = id;
                var Result = await _classServices.ManualAddStudent(AddStudentsReq);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("students/move-out")]
        [MapToApiVersion("1.0")]
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

        [HttpGet("lookup")]
        [MapToApiVersion("1.0")]
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

        [HttpPost("{id:guid}/scores/report")]
        [MapToApiVersion("1.0")]
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
                    // --- [BƯỚC 2] TẠO FILE WORD CHO TỪNG HỌC SINH ---
                    foreach (var student in students)
                    {
                        decimal averageScore = 0;
                        string autoComment = "";

                        // Lấy danh sách điểm gốc
                        var scoresList = student.Scores.Select(s => s.Score).ToList();

                        // [QUAN TRỌNG] Lọc ra danh sách các điểm > 0 trước
                        var validScores = scoresList.Where(x => x > 0).ToList();

                        // Kiểm tra: Phải có ít nhất 1 điểm > 0 thì mới tính Average
                        if (validScores.Any())
                        {
                            // Tính trung bình trên danh sách đã lọc
                            averageScore = validScores.Average();

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
                            // Trường hợp không có điểm nào hoặc toàn bộ là điểm 0
                            averageScore = 0;
                            autoComment = "";
                        }

                        // B. Chuẩn bị dữ liệu đổ vào Template Word
                        var value = new Dictionary<string, object>()
                        {
                            ["Date"] = dateRangeString,
                            ["StudentName"] = student.Name ?? "No Name",
                            ["AverageScore"] = averageScore,
                            ["TeacherComment"] = autoComment,
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
