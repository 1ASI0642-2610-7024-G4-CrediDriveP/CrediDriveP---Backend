using CrediDriveP.API.DTOs.Vehicle;

namespace CrediDriveP.API.Interfaces;

public interface IVehicleService
{
    Task<List<VehicleResponse>> GetAllAsync(string? status, string? brand);
    Task<VehicleResponse?> GetByIdAsync(int id);
    Task<VehicleResponse> CreateAsync(CreateVehicleRequest request, int createdBy);
    Task<VehicleResponse> UpdateAsync(int id, UpdateVehicleRequest request);
    Task<VehicleResponse> ToggleAsync(int id);
}