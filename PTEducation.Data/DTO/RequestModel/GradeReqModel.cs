using System.ComponentModel.DataAnnotations;

namespace PTEducation.Data.DTO.RequestModel
{
    public class GradeCreateReqModel
    {
        [Required(ErrorMessage = "Tên khối không được để trống")]
        public string GradeName { get; set; } = null!;
        public int? GroupId { get; set; }
    }

    public class GradeUpdateReqModel
    {
        [Required(ErrorMessage = "ID không được để trống")]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Tên khối không được để trống")]
        public string GradeName { get; set; } = null!;
        public int? GroupId { get; set; }
    }
}
