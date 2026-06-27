namespace CrediDriveP.API.Models;

public class Client
{
    public int Id { get; set; }
    public string Dni { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly? BirthDate { get; set; }
    public string? Phone { get; set; }
    public decimal MonthlyIncome { get; set; }
    public int? CreditScore { get; set; }
    public bool IsActive { get; set; } = true;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public User Creator { get; set; } = null!;
    public ICollection<Simulation> Simulations { get; set; } = [];
    public ICollection<Loan> Loans { get; set; } = [];
}