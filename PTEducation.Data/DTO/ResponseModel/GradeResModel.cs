namespace PTEducation.Data.DTO.ResponseModel
{
    public class GradeResModel
    {
        public int Id { get; set; }
        public string GradeName { get; set; } = null!;
        public int? GroupId { get; set; }
        public int TotalClasses { get; set; }
    }
}
