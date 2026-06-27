namespace CrediDriveP.API.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public string Condition { get; set; } = "NEW"; // NEW / USED
    public decimal Price { get; set; }
    public string PriceCurrency { get; set; } = "PEN"; // PEN / USD
    public string? Vin { get; set; }
    public string? ImageUrl { get; set; }
    public int Stock { get; set; } = 0;
    public string Status { get; set; } = "AVAILABLE"; // AVAILABLE / SOLD / RESERVED
    public bool IsActive { get; set; } = true;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public User Creator { get; set; } = null!;
    public ICollection<Simulation> Simulations { get; set; } = [];
    public ICollection<Loan> Loans { get; set; } = [];
}