namespace CrediDriveP.API.Models;

public class Simulation
{
    public int Id { get; set; }
    public int CreatedBy { get; set; }
    public int? ClientId { get; set; }
    public int VehicleId { get; set; }
    public string Name { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public decimal? ExchangeRate { get; set; }
    public decimal VehiclePrice { get; set; }
    public decimal DownPayment { get; set; }
    public decimal AmountFinanced { get; set; }
    public string RateType { get; set; } = null!;
    public decimal InterestRate { get; set; }
    public string? Capitalization { get; set; }
    public int TermMonths { get; set; }
    public string GraceType { get; set; } = "NONE";
    public int GraceMonths { get; set; } = 0;
    public string PaymentMethod { get; set; } = "FRENCH";
    public decimal? BalloonPct { get; set; }
    public DateOnly StartDate { get; set; }
    public string Status { get; set; } = "SAVED"; // SAVED / CONVERTED
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public User Creator { get; set; } = null!;
    public Client? Client { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public ICollection<SimulationSchedule> Schedules { get; set; } = [];
    public SimulationIndicator? Indicator { get; set; }
    public Loan? Loan { get; set; }
}