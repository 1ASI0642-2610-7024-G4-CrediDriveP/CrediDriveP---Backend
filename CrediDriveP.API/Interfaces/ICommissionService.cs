using CrediDriveP.API.DTOs.Commission;

namespace CrediDriveP.API.Interfaces;

public interface ICommissionService
{
    Task<List<CommissionResponse>> GetAllAsync();
    Task<CommissionResponse?> GetByIdAsync(int id);
    Task<CommissionResponse> CreateAsync(CommissionRequest request);
    Task<CommissionResponse> UpdateAsync(int id, CommissionRequest request);
    Task<CommissionResponse> ToggleAsync(int id);
}