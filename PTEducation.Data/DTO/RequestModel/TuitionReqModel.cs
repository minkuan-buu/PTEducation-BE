namespace PTEducation.Data.DTO.RequestModel
{
    public class TuitionCreateReqModel
    {
        public int GradeId { get; set; }
        public string Title { get; set; } = null!;
        public DateTime? DueDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal Amount { get; set; }
    }
}