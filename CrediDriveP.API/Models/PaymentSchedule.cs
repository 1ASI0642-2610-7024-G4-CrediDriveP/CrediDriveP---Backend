namespace CrediDriveP.API.Models;

public class PaymentSchedule
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public int PeriodNumber { get; set; }
    public DateOnly DueDate { get; set; }
    public string GraceApplied { get; set; } = "NONE";
    public decimal OpeningBalance { get; set; }
    public decimal Interest { get; set; }
    public decimal Principal { get; set; }
    public decimal InsuranceDesgravamen { get; set; }
    public decimal InsuranceVehicular { get; set; }
    public decimal Commission { get; set; }
    public decimal Balloon { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal ClosingBalance { get; set; }
    public bool IsPaid { get; set; } = false;

    public Loan Loan { get; set; } = null!;
}