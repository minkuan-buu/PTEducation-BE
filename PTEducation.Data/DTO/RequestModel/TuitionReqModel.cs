namespace PTEducation.Data.DTO.RequestModel
{
    public class TuitionCreateReqModel
    {
        public int GradeId { get; set; }
        public string Title { get; set; } = null!;
        public DateTime? DueDate { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public decimal Amount { get; set; }
    }
}