namespace CrediDriveP.API.DTOs.Loan;

public class LoanSummaryResponse
{
    public int      Id             { get; set; }
    public string   Name           { get; set; } = null!;
    public string   Status         { get; set; } = null!;
    public string   ClientName     { get; set; } = null!;
    public string   VehicleName    { get; set; } = null!;
    public decimal  AmountFinanced { get; set; }
    public string   Currency       { get; set; } = null!;
    public int      TermMonths     { get; set; }
    public decimal? Tcea           { get; set; }
    public string   CreatedByName  { get; set; } = null!;
    public string?  ApprovedByName { get; set; }
    public DateTime CreatedAt      { get; set; }
}