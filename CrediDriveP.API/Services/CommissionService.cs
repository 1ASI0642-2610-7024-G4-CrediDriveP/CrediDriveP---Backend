using CrediDriveP.API.Data;
using CrediDriveP.API.DTOs.Commission;
using CrediDriveP.API.Interfaces;
using CrediDriveP.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CrediDriveP.API.Services;

public class CommissionService(AppDbContext db) : ICommissionService
{
    public async Task<List<CommissionResponse>> GetAllAsync()
    {
        return await db.Commissions
            .OrderBy(c => c.Concept)
            .Select(c => MapToResponse(c))
            .ToListAsync();
    }

    public async Task<CommissionResponse?> GetByIdAsync(int id)
    {
        var commission = await db.Commissions.FindAsync(id);
        return commission is null ? null : MapToResponse(commission);
    }

    public async Task<CommissionResponse> CreateAsync(CommissionRequest request)
    {
        var commission = new Commission
        {
            Concept     = request.Concept,
            Amount      = request.Amount,
            Periodicity = request.Periodicity.ToUpper(),
            IsActive    = true
        };

        db.Commissions.Add(commission);
        await db.SaveChangesAsync();
        return MapToResponse(commission);
    }

    public async Task<CommissionResponse> UpdateAsync(int id, CommissionRequest request)
    {
        var commission = await db.Commissions.FindAsync(id)
            ?? throw new KeyNotFoundException("Comisión no encontrada.");

        commission.Concept     = request.Concept;
        commission.Amount      = request.Amount;
        commission.Periodicity = request.Periodicity.ToUpper();

        await db.SaveChangesAsync();
        return MapToResponse(commission);
    }

    public async Task<CommissionResponse> ToggleAsync(int id)
    {
        var commission = await db.Commissions.FindAsync(id)
            ?? throw new KeyNotFoundException("Comisión no encontrada.");

        commission.IsActive = !commission.IsActive;
        await db.SaveChangesAsync();
        return MapToResponse(commission);
    }

    private static CommissionResponse MapToResponse(Commission c) => new()
    {
        Id          = c.Id,
        Concept     = c.Concept,
        Amount      = c.Amount,
        Periodicity = c.Periodicity,
        IsActive    = c.IsActive
    };
}