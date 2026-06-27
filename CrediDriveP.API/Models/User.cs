namespace CrediDriveP.API.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "OFFICER"; // OFFICER / ADMIN
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public ICollection<Client> CreatedClients { get; set; } = [];
    public ICollection<Vehicle> CreatedVehicles { get; set; } = [];
    public ICollection<Simulation> Simulations { get; set; } = [];
    public ICollection<Loan> CreatedLoans { get; set; } = [];
    public ICollection<Loan> ApprovedLoans { get; set; } = [];
}