namespace CrediDriveP.API.DTOs.Client;

public class CreateClientRequest
{
    public string Dni { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly? BirthDate { get; set; }
    public string? Phone { get; set; }
    public decimal MonthlyIncome { get; set; }
    public int? CreditScore { get; set; }
}