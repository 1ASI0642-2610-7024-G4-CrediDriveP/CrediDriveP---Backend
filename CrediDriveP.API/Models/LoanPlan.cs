namespace CrediDriveP.API.Models;

public class LoanPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Currency { get; set; } = null!;       // PEN / USD
    public string RateType { get; set; } = null!;       // TEA / TNA
    public decimal InterestRate { get; set; }
    public string? Capitalization { get; set; }          // Solo si TNA
    public int TermMonths { get; set; }
    public string GraceType { get; set; } = "NONE";     // NONE / TOTAL / PARTIAL
    public int GraceMonths { get; set; } = 0;
    public string PaymentMethod { get; set; } = "FRENCH"; // FRENCH / FRENCH_BALLOON
    public decimal? BalloonPercentage { get; set; }
    public decimal CokAnnual { get; set; }
    public bool IsActive { get; set; } = true;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public User Creator { get; set; } = null!;
    public ICollection<LoanPlanInsurance> LoanPlanInsurances { get; set; } = [];
    public ICollection<LoanPlanCommission> LoanPlanCommissions { get; set; } = [];
}