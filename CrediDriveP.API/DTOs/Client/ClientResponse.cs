namespace CrediDriveP.API.DTOs.Client;

public class ClientResponse
{
    public int Id { get; set; }
    public string Dni { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName => $"{FirstName} {LastName}";
    public DateOnly? BirthDate { get; set; }
    public string? Phone { get; set; }
    public decimal MonthlyIncome { get; set; }
    public int? CreditScore { get; set; }
    public bool IsActive { get; set; }
    public string CreatedByName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}