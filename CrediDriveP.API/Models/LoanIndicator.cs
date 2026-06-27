namespace CrediDriveP.API.Models;

public class LoanIndicator
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public decimal Van { get; set; }
    public decimal TirMonthly { get; set; }
    public decimal TirAnnual { get; set; }
    public decimal Tcea { get; set; }
    public decimal CokUsed { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    public Loan Loan { get; set; } = null!;
}