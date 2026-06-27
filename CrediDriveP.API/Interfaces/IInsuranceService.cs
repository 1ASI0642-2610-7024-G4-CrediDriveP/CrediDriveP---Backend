using CrediDriveP.API.DTOs.Insurance;

namespace CrediDriveP.API.Interfaces;

public interface IInsuranceService
{
    Task<List<InsuranceResponse>> GetAllAsync();
    Task<InsuranceResponse?> GetByIdAsync(int id);
    Task<InsuranceResponse> CreateAsync(InsuranceRequest request);
    Task<InsuranceResponse> UpdateAsync(int id, InsuranceRequest request);
    Task<InsuranceResponse> ToggleAsync(int id);
}