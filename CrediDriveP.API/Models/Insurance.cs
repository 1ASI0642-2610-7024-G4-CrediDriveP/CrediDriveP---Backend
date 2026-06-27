namespace CrediDriveP.API.Models;

public class Insurance
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!; // DESGRAVAMEN / VEHICULAR / OTHER
    public decimal Rate { get; set; }
    public string Base { get; set; } = null!; // SALDO_INSOLUTO / VALOR_VEHICULO / MONTO_PRESTAMO
    public bool IsMandatory { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public ICollection<LoanPlanInsurance> LoanPlanInsurances { get; set; } = [];
}