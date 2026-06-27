namespace CrediDriveP.API.Models;

public class LoanPlanInsurance
{
    public int PlanId { get; set; }
    public int InsuranceId { get; set; }

    public LoanPlan LoanPlan { get; set; } = null!;
    public Insurance Insurance { get; set; } = null!;
}