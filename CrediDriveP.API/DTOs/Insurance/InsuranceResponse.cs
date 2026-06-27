namespace CrediDriveP.API.DTOs.Insurance;

public class InsuranceResponse
{
    public int     Id          { get; set; }
    public string  Name        { get; set; } = null!;
    public string  Type        { get; set; } = null!;
    public decimal Rate        { get; set; }
    public string  Base        { get; set; } = null!;
    public bool    IsMandatory { get; set; }
    public bool    IsActive    { get; set; }
}