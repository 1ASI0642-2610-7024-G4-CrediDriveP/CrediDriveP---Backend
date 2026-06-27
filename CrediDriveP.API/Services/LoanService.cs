using CrediDriveP.API.Data;
using CrediDriveP.API.DTOs.Loan;
using CrediDriveP.API.DTOs.Simulation;
using CrediDriveP.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrediDriveP.API.Services;

public class LoanService(AppDbContext db) : ILoanService
{
    // ── Listar ─────────────────────────────────────────────────────
    public async Task<List<LoanSummaryResponse>> GetAllAsync(int userId, string role)
    {
        var query = db.Loans
            .Include(l => l.Client)
            .Include(l => l.Vehicle)
            .Include(l => l.Creator)
            .Include(l => l.Approver)
            .Include(l => l.Indicator)
            .AsQueryable();

        // Officer solo ve sus préstamos creados
        if (role == "OFFICER")
            query = query.Where(l => l.CreatedBy == userId);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LoanSummaryResponse
            {
                Id             = l.Id,
                Name           = l.Name,
                Status         = l.Status,
                ClientName     = $"{l.Client.FirstName} {l.Client.LastName}",
                VehicleName    = $"{l.Vehicle.Brand} {l.Vehicle.Model} {l.Vehicle.Year}",
                AmountFinanced = l.AmountFinanced,
                Currency       = l.Currency,
                TermMonths     = l.TermMonths,
                Tcea           = l.Indicator != null ? l.Indicator.Tcea : null,
                CreatedByName  = l.Creator.Name,
                ApprovedByName = l.Approver != null ? l.Approver.Name : null,
                CreatedAt      = l.CreatedAt
            })
            .ToListAsync();
    }

    // ── Detalle ────────────────────────────────────────────────────
    public async Task<LoanDetailResponse?> GetByIdAsync(int id, int userId, string role)
    {
        var loan = await db.Loans
            .Include(l => l.Client)
            .Include(l => l.Vehicle)
            .Include(l => l.Creator)
            .Include(l => l.Approver)
            .Include(l => l.Indicator)
            .Include(l => l.Schedules.OrderBy(s => s.PeriodNumber))
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan is null) return null;

        if (role == "OFFICER" && loan.CreatedBy != userId)
            throw new UnauthorizedAccessException("No tienes acceso a este préstamo.");

        return MapToDetail(loan);
    }

    // ── Cambiar estado ─────────────────────────────────────────────
    public async Task<LoanDetailResponse> UpdateStatusAsync(
        int id, UpdateLoanStatusRequest request, int approvedBy, string role)
    {
        var loan = await db.Loans
            .Include(l => l.Client)
            .Include(l => l.Vehicle)
            .Include(l => l.Creator)
            .Include(l => l.Approver)
            .Include(l => l.Indicator)
            .Include(l => l.Schedules.OrderBy(s => s.PeriodNumber))
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException("Préstamo no encontrado.");

        var newStatus = request.Status.ToUpper();

        // Validar transiciones permitidas
        var allowed = (loan.Status, newStatus) switch
        {
            // Solo ADMIN puede aprobar o rechazar
            ("PENDING_APPROVAL", "APPROVED")  => role == "ADMIN",
            ("PENDING_APPROVAL", "REJECTED")  => role == "ADMIN",
            // ADMIN activa un préstamo aprobado
            ("APPROVED",         "ACTIVE")    => role == "ADMIN",
            // Cualquiera puede marcar como pagado o cancelado
            ("ACTIVE",           "PAID")      => true,
            ("ACTIVE",           "CANCELLED") => true,
            ("PENDING_APPROVAL", "CANCELLED") => true,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException(
                $"Transición '{loan.Status}' → '{newStatus}' no permitida para tu rol.");

        loan.Status    = newStatus;
        loan.UpdatedAt = DateTime.UtcNow;

        // Si se aprueba o rechaza, registrar quién lo hizo
        if (newStatus is "APPROVED" or "REJECTED")
            loan.ApprovedBy = approvedBy;

        await db.SaveChangesAsync();
        return MapToDetail(loan);
    }

    // ── Mapper ─────────────────────────────────────────────────────
    private static LoanDetailResponse MapToDetail(Models.Loan l) => new()
    {
        Id             = l.Id,
        Name           = l.Name,
        Status         = l.Status,
        Currency       = l.Currency,
        VehiclePrice   = l.VehiclePrice,
        DownPayment    = l.DownPayment,
        AmountFinanced = l.AmountFinanced,
        RateType       = l.RateType,
        InterestRate   = l.InterestRate,
        Capitalization = l.Capitalization,
        TermMonths     = l.TermMonths,
        GraceType      = l.GraceType,
        GraceMonths    = l.GraceMonths,
        PaymentMethod  = l.PaymentMethod,
        BalloonPct     = l.BalloonPct,
        StartDate      = l.StartDate,
        CreatedAt      = l.CreatedAt,
        ClientName     = $"{l.Client.FirstName} {l.Client.LastName}",
        ClientDni      = l.Client.Dni,
        VehicleName    = $"{l.Vehicle.Brand} {l.Vehicle.Model} {l.Vehicle.Year}",
        CreatedByName  = l.Creator.Name,
        ApprovedByName = l.Approver?.Name,
        Indicators     = l.Indicator is null ? null : new SimulationIndicatorDto
        {
            Van        = l.Indicator.Van,
            TirMonthly = l.Indicator.TirMonthly,
            TirAnnual  = l.Indicator.TirAnnual,
            Tcea       = l.Indicator.Tcea,
            CokUsed    = l.Indicator.CokUsed
        },
        Schedule = l.Schedules.Select(s => new ScheduleRowPaidDto
        {
            PeriodNumber         = s.PeriodNumber,
            GraceApplied         = s.GraceApplied,
            OpeningBalance       = s.OpeningBalance,
            Interest             = s.Interest,
            Principal            = s.Principal,
            InsuranceDesgravamen = s.InsuranceDesgravamen,
            InsuranceVehicular   = s.InsuranceVehicular,
            Commission           = s.Commission,
            Balloon              = s.Balloon,
            TotalPayment         = s.TotalPayment,
            ClosingBalance       = s.ClosingBalance,
            IsPaid               = s.IsPaid
        }).ToList()
    };
}