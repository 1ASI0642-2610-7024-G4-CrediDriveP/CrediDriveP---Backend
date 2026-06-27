namespace CrediDriveP.API.Models;

public class Loan
{
    public int Id { get; set; }
    public int? SimulationId { get; set; }
    public int ClientId { get; set; }
    public int VehicleId { get; set; }
    public int CreatedBy { get; set; }
    public int? ApprovedBy { get; set; }
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
    public string Status { get; set; } = "PENDING_APPROVAL";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public Simulation? Simulation { get; set; }
    public Client Client { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public User? Approver { get; set; }
    public ICollection<PaymentSchedule> Schedules { get; set; } = [];
    public LoanIndicator? Indicator { get; set; }
}