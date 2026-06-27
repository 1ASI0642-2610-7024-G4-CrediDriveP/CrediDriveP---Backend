using CrediDriveP.API.DTOs.Commission;
using CrediDriveP.API.DTOs.Insurance;

namespace CrediDriveP.API.DTOs.LoanPlan;

public class LoanPlanResponse
{
    public int     Id                { get; set; }
    public string  Name              { get; set; } = null!;
    public string  Currency          { get; set; } = null!;
    public string  RateType          { get; set; } = null!;
    public decimal InterestRate      { get; set; }
    public string? Capitalization    { get; set; }
    public int     TermMonths        { get; set; }
    public string  GraceType         { get; set; } = null!;
    public int     GraceMonths       { get; set; }
    public string  PaymentMethod     { get; set; } = null!;
    public decimal? BalloonPercentage { get; set; }
    public decimal CokAnnual         { get; set; }
    public bool    IsActive          { get; set; }
    public string  CreatedByName     { get; set; } = null!;
    public DateTime CreatedAt        { get; set; }
    public List<InsuranceResponse>   Insurances   { get; set; } = [];
    public List<CommissionResponse>  Commissions  { get; set; } = [];
}