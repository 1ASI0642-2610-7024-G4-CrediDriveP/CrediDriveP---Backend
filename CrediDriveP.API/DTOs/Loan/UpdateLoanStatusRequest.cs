namespace CrediDriveP.API.DTOs.Loan;

public class UpdateLoanStatusRequest
{
    // APPROVED / REJECTED / ACTIVE / PAID / CANCELLED
    public string Status { get; set; } = null!;
    public string? Reason { get; set; }
}