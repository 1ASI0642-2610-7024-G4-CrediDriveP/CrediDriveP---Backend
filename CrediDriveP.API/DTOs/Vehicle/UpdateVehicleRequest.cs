namespace CrediDriveP.API.DTOs.Vehicle;

public class UpdateVehicleRequest
{
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public string Condition { get; set; } = null!;
    public decimal Price { get; set; }
    public string PriceCurrency { get; set; } = null!;
    public string? Vin { get; set; }
    public string? ImageUrl { get; set; }
    public int Stock { get; set; }
    public string Status { get; set; } = null!;
}