public class StudentClassResModelForSheet
{
    public Guid StudentClassId { get; set; }
    public string Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Score { get; set; }
    public string? Note { get; set; }
}