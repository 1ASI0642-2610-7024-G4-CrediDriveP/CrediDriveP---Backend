using CrediDriveP.API.Data;
using CrediDriveP.API.DTOs.Client;
using CrediDriveP.API.Interfaces;
using CrediDriveP.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CrediDriveP.API.Services;

public class ClientService(AppDbContext db) : IClientService
{
    // ── Listar todos ───────────────────────────────────────────────
    public async Task<List<ClientResponse>> GetAllAsync()
    {
        return await db.Clients
            .Include(c => c.Creator)
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => MapToResponse(c))
            .ToListAsync();
    }

    // ── Obtener por ID ─────────────────────────────────────────────
    public async Task<ClientResponse?> GetByIdAsync(int id)
    {
        var client = await db.Clients
            .Include(c => c.Creator)
            .FirstOrDefaultAsync(c => c.Id == id);

        return client is null ? null : MapToResponse(client);
    }

    // ── Crear ──────────────────────────────────────────────────────
    public async Task<ClientResponse> CreateAsync(CreateClientRequest request, int createdBy)
    {
        var exists = await db.Clients.AnyAsync(c => c.Dni == request.Dni);
        if (exists)
            throw new InvalidOperationException("Ya existe un cliente con ese DNI.");

        var client = new Client
        {
            Dni           = request.Dni,
            FirstName     = request.FirstName,
            LastName      = request.LastName,
            BirthDate     = request.BirthDate,
            Phone         = request.Phone,
            MonthlyIncome = request.MonthlyIncome,
            CreditScore   = request.CreditScore,
            IsActive      = true,
            CreatedBy     = createdBy,
            CreatedAt     = DateTime.UtcNow,
            UpdatedAt     = DateTime.UtcNow
        };

        db.Clients.Add(client);
        await db.SaveChangesAsync();

        // Recargar con Creator para el response
        await db.Entry(client).Reference(c => c.Creator).LoadAsync();
        return MapToResponse(client);
    }

    // ── Editar ─────────────────────────────────────────────────────
    public async Task<ClientResponse> UpdateAsync(int id, UpdateClientRequest request)
    {
        var client = await db.Clients
            .Include(c => c.Creator)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Cliente no encontrado.");

        client.FirstName     = request.FirstName;
        client.LastName      = request.LastName;
        client.BirthDate     = request.BirthDate;
        client.Phone         = request.Phone;
        client.MonthlyIncome = request.MonthlyIncome;
        client.CreditScore   = request.CreditScore;
        client.UpdatedAt     = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return MapToResponse(client);
    }

    // ── Soft delete ────────────────────────────────────────────────
    public async Task<ClientResponse> ToggleAsync(int id)
    {
        var client = await db.Clients
            .Include(c => c.Creator)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Cliente no encontrado.");

        client.IsActive  = !client.IsActive;
        client.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return MapToResponse(client);
    }

    // ── Mapper ─────────────────────────────────────────────────────
    private static ClientResponse MapToResponse(Client c) => new()
    {
        Id            = c.Id,
        Dni           = c.Dni,
        FirstName     = c.FirstName,
        LastName      = c.LastName,
        BirthDate     = c.BirthDate,
        Phone         = c.Phone,
        MonthlyIncome = c.MonthlyIncome,
        CreditScore   = c.CreditScore,
        IsActive      = c.IsActive,
        CreatedByName = c.Creator?.Name ?? "—",
        CreatedAt     = c.CreatedAt
    };
}