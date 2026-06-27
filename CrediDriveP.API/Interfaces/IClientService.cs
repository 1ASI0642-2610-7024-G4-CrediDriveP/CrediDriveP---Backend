using CrediDriveP.API.DTOs.Client;

namespace CrediDriveP.API.Interfaces;

public interface IClientService
{
    Task<List<ClientResponse>> GetAllAsync();
    Task<ClientResponse?> GetByIdAsync(int id);
    Task<ClientResponse> CreateAsync(CreateClientRequest request, int createdBy);
    Task<ClientResponse> UpdateAsync(int id, UpdateClientRequest request);
    Task<ClientResponse> ToggleAsync(int id);
}