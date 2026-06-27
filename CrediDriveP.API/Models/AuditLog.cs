namespace CrediDriveP.API.Models;

public class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Entity { get; set; } = null!;
    public int? EntityId { get; set; }
    public string Action { get; set; } = null!; // CREATE/UPDATE/DELETE/LOGIN/LOGOUT
    public string? Payload { get; set; }         // JSON como string
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}