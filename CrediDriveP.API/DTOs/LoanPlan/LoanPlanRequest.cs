namespace CrediDriveP.API.DTOs.LoanPlan;

public class LoanPlanRequest
{
    public string  Name               { get; set; } = null!;
    public string  Currency           { get; set; } = "PEN";
    public string  RateType           { get; set; } = "TEA";
    public decimal InterestRate       { get; set; }
    public string? Capitalization     { get; set; }
    public int     TermMonths         { get; set; }
    public string  GraceType          { get; set; } = "NONE";
    public int     GraceMonths        { get; set; } = 0;
    public string  PaymentMethod      { get; set; } = "FRENCH";
    public decimal? BalloonPercentage { get; set; }
    public decimal CokAnnual          { get; set; }
    public List<int> InsuranceIds     { get; set; } = [];
    public List<int> CommissionIds    { get; set; } = [];
}