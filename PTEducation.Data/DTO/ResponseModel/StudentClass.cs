using PTEducation.Data.DTO.ResponseModel;

public class StudentClassResModelForSheet
{
    public Guid StudentClassId { get; set; }
    public string Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Score { get; set; }
    public string? Note { get; set; }
}

public class StudentInClassResModel
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public List<UserGuardianListResModel> Guardians { get; set; } = null!;
}