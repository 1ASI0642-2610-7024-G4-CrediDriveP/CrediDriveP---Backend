namespace CrediDriveP.API.DTOs.Simulation;

public class CreateSimulationRequest
{
    public int?    ClientId      { get; set; }  // opcional
    public int     VehicleId     { get; set; }
    public string  Name          { get; set; } = null!;
    public string  Currency      { get; set; } = "PEN";
    public decimal? ExchangeRate { get; set; }
    public decimal DownPayment   { get; set; }
    public string  RateType      { get; set; } = "TEA"; // TEA / TNA
    public decimal InterestRate  { get; set; }
    public string? Capitalization { get; set; }         // Solo si TNA
    public int     TermMonths    { get; set; }
    public string  GraceType     { get; set; } = "NONE";
    public int     GraceMonths   { get; set; } = 0;
    public string  PaymentMethod { get; set; } = "FRENCH";
    public decimal? BalloonPct   { get; set; }
    public DateOnly StartDate    { get; set; }
    public decimal CokAnnual     { get; set; }

    // Seguros (tasas mensuales decimales)
    public decimal RateDesgravamen     { get; set; } = 0;
    public decimal RateVehicular       { get; set; } = 0;
    public string  InsuranceBaseDesgrv { get; set; } = "SALDO_INSOLUTO";
    public string  InsuranceBaseVehic  { get; set; } = "VALOR_VEHICULO";

    // Comisión mensual fija
    public decimal CommissionMonthly { get; set; } = 0;
}