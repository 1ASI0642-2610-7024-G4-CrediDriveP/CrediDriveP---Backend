using CrediDriveP.API.DTOs.Auth;

namespace CrediDriveP.API.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<OfficerResponse> CreateOfficerAsync(CreateOfficerRequest request);
    Task<List<OfficerResponse>> GetOfficersAsync();
    Task<OfficerResponse> UpdateOfficerAsync(int id, UpdateOfficerRequest request);
    Task<OfficerResponse> ToggleOfficerAsync(int id);
    Task ResetPasswordAsync(int id, string newPassword);
    Task<OfficerResponse?> GetMeAsync(int userId);
}