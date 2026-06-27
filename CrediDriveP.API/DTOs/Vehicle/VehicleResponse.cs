namespace CrediDriveP.API.DTOs.Vehicle;

public class VehicleResponse
{
    public int Id { get; set; }
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string FullName => $"{Brand} {Model} {Year}";
    public int Year { get; set; }
    public string Condition { get; set; } = null!;
    public decimal Price { get; set; }
    public string PriceCurrency { get; set; } = null!;
    public string? Vin { get; set; }
    public string? ImageUrl { get; set; }
    public int Stock { get; set; }
    public string Status { get; set; } = null!;
    public bool IsActive { get; set; }
    public string CreatedByName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}