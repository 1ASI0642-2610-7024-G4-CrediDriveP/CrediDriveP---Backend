using CrediDriveP.API.Data;
using CrediDriveP.API.DTOs.Vehicle;
using CrediDriveP.API.Interfaces;
using CrediDriveP.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CrediDriveP.API.Services;

public class VehicleService(AppDbContext db) : IVehicleService
{
    public async Task<List<VehicleResponse>> GetAllAsync(string? status, string? brand)
    {
        var query = db.Vehicles
            .Include(v => v.Creator)
            .Where(v => v.IsActive);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(v => v.Status == status.ToUpper());

        if (!string.IsNullOrEmpty(brand))
            query = query.Where(v => v.Brand.Contains(brand));

        return await query
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => MapToResponse(v))
            .ToListAsync();
    }

    public async Task<VehicleResponse?> GetByIdAsync(int id)
    {
        var vehicle = await db.Vehicles
            .Include(v => v.Creator)
            .FirstOrDefaultAsync(v => v.Id == id);

        return vehicle is null ? null : MapToResponse(vehicle);
    }

    public async Task<VehicleResponse> CreateAsync(CreateVehicleRequest request, int createdBy)
    {
        if (!string.IsNullOrEmpty(request.Vin))
        {
            var vinExists = await db.Vehicles.AnyAsync(v => v.Vin == request.Vin);
            if (vinExists)
                throw new InvalidOperationException("Ya existe un vehículo con ese VIN.");
        }

        var vehicle = new Vehicle
        {
            Brand         = request.Brand,
            Model         = request.Model,
            Year          = request.Year,
            Condition     = request.Condition.ToUpper(),
            Price         = request.Price,
            PriceCurrency = request.PriceCurrency.ToUpper(),
            Vin           = request.Vin,
            ImageUrl      = request.ImageUrl,
            Stock         = request.Stock,
            Status        = "AVAILABLE",
            IsActive      = true,
            CreatedBy     = createdBy,
            CreatedAt     = DateTime.UtcNow,
            UpdatedAt     = DateTime.UtcNow
        };

        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        await db.Entry(vehicle).Reference(v => v.Creator).LoadAsync();
        return MapToResponse(vehicle);
    }

    public async Task<VehicleResponse> UpdateAsync(int id, UpdateVehicleRequest request)
    {
        var vehicle = await db.Vehicles
            .Include(v => v.Creator)
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException("Vehículo no encontrado.");

        if (!string.IsNullOrEmpty(request.Vin) && request.Vin != vehicle.Vin)
        {
            var vinExists = await db.Vehicles.AnyAsync(v => v.Vin == request.Vin);
            if (vinExists)
                throw new InvalidOperationException("Ya existe un vehículo con ese VIN.");
        }

        vehicle.Brand         = request.Brand;
        vehicle.Model         = request.Model;
        vehicle.Year          = request.Year;
        vehicle.Condition     = request.Condition.ToUpper();
        vehicle.Price         = request.Price;
        vehicle.PriceCurrency = request.PriceCurrency.ToUpper();
        vehicle.Vin           = request.Vin;
        vehicle.ImageUrl      = request.ImageUrl;
        vehicle.Stock         = request.Stock;
        vehicle.Status        = request.Status.ToUpper();
        vehicle.UpdatedAt     = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return MapToResponse(vehicle);
    }

    public async Task<VehicleResponse> ToggleAsync(int id)
    {
        var vehicle = await db.Vehicles
            .Include(v => v.Creator)
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException("Vehículo no encontrado.");

        vehicle.IsActive  = !vehicle.IsActive;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return MapToResponse(vehicle);
    }

    private static VehicleResponse MapToResponse(Vehicle v) => new()
    {
        Id            = v.Id,
        Brand         = v.Brand,
        Model         = v.Model,
        Year          = v.Year,
        Condition     = v.Condition,
        Price         = v.Price,
        PriceCurrency = v.PriceCurrency,
        Vin           = v.Vin,
        ImageUrl      = v.ImageUrl,
        Stock         = v.Stock,
        Status        = v.Status,
        IsActive      = v.IsActive,
        CreatedByName = v.Creator?.Name ?? "—",
        CreatedAt     = v.CreatedAt
    };
}