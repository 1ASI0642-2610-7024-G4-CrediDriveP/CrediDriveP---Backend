namespace CrediDriveP.API.DTOs.Simulation;

public class SimulationResponse
{
    public int      Id              { get; set; }
    public string   Name            { get; set; } = null!;
    public string   Status          { get; set; } = null!;
    public string   Currency        { get; set; } = null!;
    public decimal  VehiclePrice    { get; set; }
    public decimal  DownPayment     { get; set; }
    public decimal  AmountFinanced  { get; set; }
    public string   RateType        { get; set; } = null!;
    public decimal  InterestRate    { get; set; }
    public string?  Capitalization  { get; set; }
    public int      TermMonths      { get; set; }
    public string   GraceType       { get; set; } = null!;
    public int      GraceMonths     { get; set; }
    public string   PaymentMethod   { get; set; } = null!;
    public decimal? BalloonPct      { get; set; }
    public DateOnly StartDate       { get; set; }
    public DateTime CreatedAt       { get; set; }

    // Relaciones
    public string   CreatedByName   { get; set; } = null!;
    public string?  ClientName      { get; set; }
    public string   VehicleName     { get; set; } = null!;

    // Indicadores
    public SimulationIndicatorDto? Indicators { get; set; }

    // Cronograma
    public List<ScheduleRowDto> Schedule { get; set; } = [];
}

public class SimulationIndicatorDto
{
    public decimal Van        { get; set; }
    public decimal TirMonthly { get; set; }
    public decimal TirAnnual  { get; set; }
    public decimal Tcea       { get; set; }
    public decimal CokUsed    { get; set; }
}

public class ScheduleRowDto
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
}