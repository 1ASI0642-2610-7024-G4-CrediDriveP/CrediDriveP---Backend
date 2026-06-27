namespace CrediDriveP.API.DTOs.Commission;

public class CommissionRequest
{
    public string  Concept     { get; set; } = null!;
    public decimal Amount      { get; set; }
    public string  Periodicity { get; set; } = null!; // MONTHLY / ONE_TIME / ANNUAL
}