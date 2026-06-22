using Microsoft.AspNetCore.Http;

namespace PTEducation.Data.DTO.RequestModel;

public class AttachmentReqModel
{
    public IFormFile File { get; set; }
}