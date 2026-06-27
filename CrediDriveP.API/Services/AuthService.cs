using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CrediDriveP.API.Data;
using CrediDriveP.API.DTOs.Auth;
using CrediDriveP.API.Interfaces;
using CrediDriveP.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CrediDriveP.API.Services;

public class AuthService(AppDbContext db, IConfiguration config) : IAuthService
{
    // ── Login ──────────────────────────────────────────────────────
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var token = GenerateJwt(user);
        var hours = int.Parse(config["Jwt:ExpiresInHours"]!);

        return new LoginResponse
        {
            Token     = token,
            Name      = user.Name,
            Email     = user.Email,
            Role      = user.Role,
            ExpiresAt = DateTime.UtcNow.AddHours(hours)
        };
    }

    // ── Crear Officer ──────────────────────────────────────────────
    public async Task<OfficerResponse> CreateOfficerAsync(CreateOfficerRequest request)
    {
        var exists = await db.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
            throw new InvalidOperationException("El correo ya está registrado.");

        var user = new User
        {
            Name         = request.Name,
            Email        = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role         = "OFFICER",
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return MapToResponse(user);
    }

    // ── Listar Officers ────────────────────────────────────────────
    public async Task<List<OfficerResponse>> GetOfficersAsync()
    {
        var officers = await db.Users
            .Where(u => u.Role == "OFFICER")
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return officers.Select(MapToResponse).ToList();
    }

    // ── Editar Officer ─────────────────────────────────────────────
    public async Task<OfficerResponse> UpdateOfficerAsync(int id, UpdateOfficerRequest request)
    {
        var user = await db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException("Officer no encontrado.");

        var emailTaken = await db.Users
            .AnyAsync(u => u.Email == request.Email && u.Id != id);
        if (emailTaken)
            throw new InvalidOperationException("El correo ya está en uso.");

        user.Name      = request.Name;
        user.Email     = request.Email;
        user.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return MapToResponse(user);
    }

    // ── Activar / Desactivar Officer ───────────────────────────────
    public async Task<OfficerResponse> ToggleOfficerAsync(int id)
    {
        var user = await db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException("Officer no encontrado.");

        if (user.Role == "ADMIN")
            throw new InvalidOperationException("No se puede desactivar un Admin.");

        user.IsActive  = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return MapToResponse(user);
    }

    // ── Reset Password ─────────────────────────────────────────────
    public async Task ResetPasswordAsync(int id, string newPassword)
    {
        var user = await db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException("Officer no encontrado.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt    = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    // ── Mi perfil ──────────────────────────────────────────────────
    public async Task<OfficerResponse?> GetMeAsync(int userId)
    {
        var user = await db.Users.FindAsync(userId);
        return user is null ? null : MapToResponse(user);
    }

    // ── Helpers privados ───────────────────────────────────────────
    private string GenerateJwt(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var hours = int.Parse(config["Jwt:ExpiresInHours"]!);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Name,           user.Name),
            new Claim(ClaimTypes.Role,           user.Role)
        };

        var token = new JwtSecurityToken(
            issuer:             config["Jwt:Issuer"],
            audience:           config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(hours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static OfficerResponse MapToResponse(User user) => new()
    {
        Id        = user.Id,
        Name      = user.Name,
        Email     = user.Email,
        Role      = user.Role,
        IsActive  = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}