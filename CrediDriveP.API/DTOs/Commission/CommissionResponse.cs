namespace CrediDriveP.API.DTOs.Commission;

public class CommissionResponse
{
    public int     Id          { get; set; }
    public string  Concept     { get; set; } = null!;
    public decimal Amount      { get; set; }
    public string  Periodicity { get; set; } = null!;
    public bool    IsActive    { get; set; }
}