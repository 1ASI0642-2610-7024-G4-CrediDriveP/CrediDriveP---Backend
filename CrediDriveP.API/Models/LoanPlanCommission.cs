namespace CrediDriveP.API.Models;

public class LoanPlanCommission
{
    public int PlanId { get; set; }
    public int CommissionId { get; set; }

    public LoanPlan LoanPlan { get; set; } = null!;
    public Commission Commission { get; set; } = null!;
}