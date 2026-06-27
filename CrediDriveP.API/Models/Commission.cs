namespace CrediDriveP.API.Models;

public class Commission
{
    public int Id { get; set; }
    public string Concept { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Periodicity { get; set; } = null!; // MONTHLY / ONE_TIME / ANNUAL
    public bool IsActive { get; set; } = true;

    public ICollection<LoanPlanCommission> LoanPlanCommissions { get; set; } = [];
}