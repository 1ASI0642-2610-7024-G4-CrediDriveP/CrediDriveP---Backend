using CrediDriveP.API.Data;
using CrediDriveP.API.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrediDriveP.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(AppDbContext db) : ControllerBase
{
    // GET /api/dashboard/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var totalClients     = await db.Clients.CountAsync(c => c.IsActive);
        var totalVehicles    = await db.Vehicles.CountAsync(v => v.IsActive);
        var totalSimulations = await db.Simulations.CountAsync();
        var pendingLoans     = await db.Loans
            .CountAsync(l => l.Status == "PENDING_APPROVAL");
        var approvedLoans    = await db.Loans
            .CountAsync(l => l.Status == "APPROVED" || l.Status == "ACTIVE");

        // Actividad reciente: últimos 5 eventos combinados
        var recentClients = await db.Clients
            .OrderByDescending(c => c.CreatedAt)
            .Take(3)
            .Select(c => new RecentActivityDto
            {
                Type        = "CLIENT",
                Description = $"Nuevo cliente: {c.FirstName} {c.LastName}",
                CreatedAt   = c.CreatedAt
            }).ToListAsync();

        var recentSimulations = await db.Simulations
            .Include(s => s.Vehicle)
            .OrderByDescending(s => s.CreatedAt)
            .Take(3)
            .Select(s => new RecentActivityDto
            {
                Type        = "SIMULATION",
                Description = $"Simulación: {s.Name}",
                CreatedAt   = s.CreatedAt
            }).ToListAsync();

        var recentLoans = await db.Loans
            .OrderByDescending(l => l.CreatedAt)
            .Take(3)
            .Select(l => new RecentActivityDto
            {
                Type        = "LOAN",
                Description = $"Préstamo {l.Status}: {l.Name}",
                CreatedAt   = l.CreatedAt
            }).ToListAsync();

        var activity = recentClients
            .Concat(recentSimulations)
            .Concat(recentLoans)
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .ToList();

        return Ok(new DashboardSummaryResponse
        {
            TotalClients     = totalClients,
            TotalVehicles    = totalVehicles,
            TotalSimulations = totalSimulations,
            PendingLoans     = pendingLoans,
            ApprovedLoans    = approvedLoans,
            RecentActivity   = activity
        });
    }
}