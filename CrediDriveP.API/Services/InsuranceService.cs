using CrediDriveP.API.Data;
using CrediDriveP.API.DTOs.Insurance;
using CrediDriveP.API.Interfaces;
using CrediDriveP.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CrediDriveP.API.Services;

public class InsuranceService(AppDbContext db) : IInsuranceService
{
    public async Task<List<InsuranceResponse>> GetAllAsync()
    {
        return await db.Insurances
            .OrderBy(i => i.Name)
            .Select(i => MapToResponse(i))
            .ToListAsync();
    }

    public async Task<InsuranceResponse?> GetByIdAsync(int id)
    {
        var insurance = await db.Insurances.FindAsync(id);
        return insurance is null ? null : MapToResponse(insurance);
    }

    public async Task<InsuranceResponse> CreateAsync(InsuranceRequest request)
    {
        var insurance = new Insurance
        {
            Name        = request.Name,
            Type        = request.Type.ToUpper(),
            Rate        = request.Rate,
            Base        = request.Base.ToUpper(),
            IsMandatory = request.IsMandatory,
            IsActive    = true
        };

        db.Insurances.Add(insurance);
        await db.SaveChangesAsync();
        return MapToResponse(insurance);
    }

    public async Task<InsuranceResponse> UpdateAsync(int id, InsuranceRequest request)
    {
        var insurance = await db.Insurances.FindAsync(id)
            ?? throw new KeyNotFoundException("Seguro no encontrado.");

        insurance.Name        = request.Name;
        insurance.Type        = request.Type.ToUpper();
        insurance.Rate        = request.Rate;
        insurance.Base        = request.Base.ToUpper();
        insurance.IsMandatory = request.IsMandatory;

        await db.SaveChangesAsync();
        return MapToResponse(insurance);
    }

    public async Task<InsuranceResponse> ToggleAsync(int id)
    {
        var insurance = await db.Insurances.FindAsync(id)
            ?? throw new KeyNotFoundException("Seguro no encontrado.");

        insurance.IsActive = !insurance.IsActive;
        await db.SaveChangesAsync();
        return MapToResponse(insurance);
    }

    private static InsuranceResponse MapToResponse(Insurance i) => new()
    {
        Id          = i.Id,
        Name        = i.Name,
        Type        = i.Type,
        Rate        = i.Rate,
        Base        = i.Base,
        IsMandatory = i.IsMandatory,
        IsActive    = i.IsActive
    };
}