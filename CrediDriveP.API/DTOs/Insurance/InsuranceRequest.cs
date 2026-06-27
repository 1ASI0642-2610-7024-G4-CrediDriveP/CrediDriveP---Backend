namespace CrediDriveP.API.DTOs.Insurance;

public class InsuranceRequest
{
    public string  Name        { get; set; } = null!;
    public string  Type        { get; set; } = null!; // DESGRAVAMEN / VEHICULAR / OTHER
    public decimal Rate        { get; set; }
    public string  Base        { get; set; } = null!; // SALDO_INSOLUTO / VALOR_VEHICULO / MONTO_PRESTAMO
    public bool    IsMandatory { get; set; } = true;
}