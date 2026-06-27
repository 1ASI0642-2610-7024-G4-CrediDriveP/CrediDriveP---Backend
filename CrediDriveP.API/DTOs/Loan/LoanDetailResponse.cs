using CrediDriveP.API.DTOs.Simulation;

namespace CrediDriveP.API.DTOs.Loan;

public class LoanDetailResponse
{
    public int      Id             { get; set; }
    public string   Name           { get; set; } = null!;
    public string   Status         { get; set; } = null!;
    public string   Currency       { get; set; } = null!;
    public decimal  VehiclePrice   { get; set; }
    public decimal  DownPayment    { get; set; }
    public decimal  AmountFinanced { get; set; }
    public string   RateType       { get; set; } = null!;
    public decimal  InterestRate   { get; set; }
    public string?  Capitalization { get; set; }
    public int      TermMonths     { get; set; }
    public string   GraceType      { get; set; } = null!;
    public int      GraceMonths    { get; set; }
    public string   PaymentMethod  { get; set; } = null!;
    public decimal? BalloonPct     { get; set; }
    public DateOnly StartDate      { get; set; }
    public DateTime CreatedAt      { get; set; }

    // Relaciones
    public string   ClientName     { get; set; } = null!;
    public string   ClientDni      { get; set; } = null!;
    public string   VehicleName    { get; set; } = null!;
    public string   CreatedByName  { get; set; } = null!;
    public string?  ApprovedByName { get; set; }

    // Indicadores y cronograma
    public SimulationIndicatorDto? Indicators { get; set; }
    public List<ScheduleRowPaidDto> Schedule  { get; set; } = [];
}

public class ScheduleRowPaidDto
{
    public int     PeriodNumber          { get; set; }
    public string  GraceApplied          { get; set; } = null!;
    public decimal OpeningBalance        { get; set; }
    public decimal Interest              { get; set; }
    public decimal Principal             { get; set; }
    public decimal InsuranceDesgravamen  { get; set; }
    public decimal InsuranceVehicular    { get; set; }
    public decimal Commission            { get; set; }
    public decimal Balloon               { get; set; }
    public decimal TotalPayment          { get; set; }
    public decimal ClosingBalance        { get; set; }
    public bool    IsPaid                { get; set; }
}