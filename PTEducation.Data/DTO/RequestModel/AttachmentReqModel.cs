using Microsoft.AspNetCore.Http;

namespace PTEducation.Data.DTO.RequestModel;

public class AttachmentReqModel
{
    public required IFormFile File { get; set; }
}