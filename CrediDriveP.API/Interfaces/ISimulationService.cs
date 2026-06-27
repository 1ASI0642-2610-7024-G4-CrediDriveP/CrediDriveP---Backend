using CrediDriveP.API.DTOs.Simulation;

namespace CrediDriveP.API.Interfaces;

public interface ISimulationService
{
    Task<List<SimulationSummaryResponse>> GetAllAsync(int userId, string role);
    Task<SimulationResponse?> GetByIdAsync(int id, int userId, string role);
    Task<SimulationResponse> CreateAsync(CreateSimulationRequest request, int createdBy);
    Task DeleteAsync(int id, int userId, string role);
    Task<SimulationResponse> ConvertToLoanAsync(int id, int userId, string role);
}