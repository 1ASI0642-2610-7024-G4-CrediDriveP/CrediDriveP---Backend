using CrediDriveP.API.Data;
using CrediDriveP.API.DTOs.Simulation;
using CrediDriveP.API.Helpers;
using CrediDriveP.API.Interfaces;
using CrediDriveP.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CrediDriveP.API.Services;

public class SimulationService(AppDbContext db) : ISimulationService
{
    // ── Listar ─────────────────────────────────────────────────────
    public async Task<List<SimulationSummaryResponse>> GetAllAsync(int userId, string role)
    {
        var query = db.Simulations
            .Include(s => s.Vehicle)
            .Include(s => s.Client)
            .Include(s => s.Indicator)
            .AsQueryable();

        // Officer solo ve sus propias simulaciones
        if (role == "OFFICER")
            query = query.Where(s => s.CreatedBy == userId);

        return await query
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SimulationSummaryResponse
            {
                Id             = s.Id,
                Name           = s.Name,
                Status         = s.Status,
                VehicleName    = $"{s.Vehicle.Brand} {s.Vehicle.Model} {s.Vehicle.Year}",
                ClientName     = s.Client != null
                    ? $"{s.Client.FirstName} {s.Client.LastName}" : null,
                AmountFinanced = s.AmountFinanced,
                Currency       = s.Currency,
                TermMonths     = s.TermMonths,
                Tcea           = s.Indicator != null ? s.Indicator.Tcea : null,
                CreatedAt      = s.CreatedAt
            })
            .ToListAsync();
    }

    // ── Detalle ────────────────────────────────────────────────────
    public async Task<SimulationResponse?> GetByIdAsync(int id, int userId, string role)
    {
        var s = await db.Simulations
            .Include(s => s.Creator)
            .Include(s => s.Client)
            .Include(s => s.Vehicle)
            .Include(s => s.Indicator)
            .Include(s => s.Schedules.OrderBy(r => r.PeriodNumber))
            .FirstOrDefaultAsync(s => s.Id == id);

        if (s is null) return null;
        if (role == "OFFICER" && s.CreatedBy != userId)
            throw new UnauthorizedAccessException("No tienes acceso a esta simulación.");

        return MapToResponse(s);
    }

    // ── Crear + Calcular ───────────────────────────────────────────
    public async Task<SimulationResponse> CreateAsync(
        CreateSimulationRequest req, int createdBy)
    {
        // Validar vehículo
        var vehicle = await db.Vehicles.FindAsync(req.VehicleId)
            ?? throw new KeyNotFoundException("Vehículo no encontrado.");

        // Validar cliente si se envió
        if (req.ClientId.HasValue)
        {
            var clientExists = await db.Clients.AnyAsync(c => c.Id == req.ClientId);
            if (!clientExists)
                throw new KeyNotFoundException("Cliente no encontrado.");
        }

        // Precio del vehículo en la moneda de la simulación
        decimal vehiclePrice = vehicle.Price;

        // Monto financiado
        decimal amountFinanced = Math.Round(vehiclePrice - req.DownPayment, 2);
        if (amountFinanced <= 0)
            throw new InvalidOperationException("La cuota inicial supera el precio del vehículo.");

        // Convertir tasa a TEA
        decimal tea = req.RateType == "TNA"
            ? FinancialEngine.TnaToTea(req.InterestRate, req.Capitalization!)
            : req.InterestRate;

        // Parámetros del motor
        var scheduleParams = new FinancialEngine.ScheduleParams
        {
            AmountFinanced       = amountFinanced,
            Tea                  = tea,
            TermMonths           = req.TermMonths,
            GraceType            = req.GraceType,
            GraceMonths          = req.GraceMonths,
            PaymentMethod        = req.PaymentMethod,
            BalloonPct           = req.BalloonPct ?? 0,
            VehiclePrice         = vehiclePrice,
            RateDesgravamen      = req.RateDesgravamen,
            RateVehicular        = req.RateVehicular,
            CommissionMonthly    = req.CommissionMonthly,
            InsuranceBaseDesgrv  = req.InsuranceBaseDesgrv,
            InsuranceBaseVehic   = req.InsuranceBaseVehic
        };

        // Generar cronograma
        var rows = FinancialEngine.GenerateSchedule(scheduleParams);

        // Calcular indicadores
        decimal van        = FinancialEngine.CalculateVan(amountFinanced, rows, req.CokAnnual);
        decimal tirMonthly = FinancialEngine.CalculateTirMonthly(amountFinanced, rows);
        decimal tirAnnual  = FinancialEngine.TirMonthlyToAnnual(tirMonthly);
        decimal tcea       = FinancialEngine.CalculateTcea(tirAnnual);

        // Generar nombre automático si no se envió
        string name = string.IsNullOrWhiteSpace(req.Name)
            ? $"SIM-{DateTime.UtcNow:yyyyMMdd-HHmmss}"
            : req.Name;

        // Guardar en BD
        var simulation = new Simulation
        {
            CreatedBy      = createdBy,
            ClientId       = req.ClientId,
            VehicleId      = req.VehicleId,
            Name           = name,
            Currency       = req.Currency.ToUpper(),
            ExchangeRate   = req.ExchangeRate,
            VehiclePrice   = vehiclePrice,
            DownPayment    = req.DownPayment,
            AmountFinanced = amountFinanced,
            RateType       = req.RateType.ToUpper(),
            InterestRate   = req.InterestRate,
            Capitalization = req.Capitalization,
            TermMonths     = req.TermMonths,
            GraceType      = req.GraceType.ToUpper(),
            GraceMonths    = req.GraceMonths,
            PaymentMethod  = req.PaymentMethod.ToUpper(),
            BalloonPct     = req.BalloonPct,
            StartDate      = req.StartDate,
            Status         = "SAVED",
            CreatedAt      = DateTime.UtcNow
        };

        db.Simulations.Add(simulation);
        await db.SaveChangesAsync();

        // Guardar cronograma
        var scheduleEntities = rows.Select((r, idx) => new SimulationSchedule
        {
            SimulationId          = simulation.Id,
            PeriodNumber          = r.PeriodNumber,
            DueDate               = req.StartDate.AddMonths(idx + 1),
            GraceApplied          = r.GraceApplied,
            OpeningBalance        = r.OpeningBalance,
            Interest              = r.Interest,
            Principal             = r.Principal,
            InsuranceDesgravamen  = r.InsuranceDesgravamen,
            InsuranceVehicular    = r.InsuranceVehicular,
            Commission            = r.Commission,
            Balloon               = r.Balloon,
            TotalPayment          = r.TotalPayment,
            ClosingBalance        = r.ClosingBalance
        }).ToList();

        db.SimulationSchedules.AddRange(scheduleEntities);

        // Guardar indicadores
        var indicator = new SimulationIndicator
        {
            SimulationId  = simulation.Id,
            Van           = van,
            TirMonthly    = tirMonthly,
            TirAnnual     = tirAnnual,
            Tcea          = tcea,
            CokUsed       = req.CokAnnual,
            CalculatedAt  = DateTime.UtcNow
        };

        db.SimulationIndicators.Add(indicator);
        await db.SaveChangesAsync();

        // Recargar para response completo
        await db.Entry(simulation).Reference(s => s.Creator).LoadAsync();
        await db.Entry(simulation).Reference(s => s.Vehicle).LoadAsync();
        if (simulation.ClientId.HasValue)
            await db.Entry(simulation).Reference(s => s.Client).LoadAsync();

        simulation.Schedules  = scheduleEntities;
        simulation.Indicator  = indicator;

        return MapToResponse(simulation);
    }

    // ── Eliminar ───────────────────────────────────────────────────
    public async Task DeleteAsync(int id, int userId, string role)
    {
        var simulation = await db.Simulations.FindAsync(id)
            ?? throw new KeyNotFoundException("Simulación no encontrada.");

        if (role == "OFFICER" && simulation.CreatedBy != userId)
            throw new UnauthorizedAccessException("No tienes permiso.");

        if (simulation.Status == "CONVERTED")
            throw new InvalidOperationException(
                "No se puede eliminar una simulación ya convertida a préstamo.");

        db.Simulations.Remove(simulation);
        await db.SaveChangesAsync();
    }

    // ── Convertir a préstamo ───────────────────────────────────────
    public async Task<SimulationResponse> ConvertToLoanAsync(int id, int userId, string role)
    {
        var simulation = await db.Simulations
            .Include(s => s.Schedules)
            .Include(s => s.Indicator)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException("Simulación no encontrada.");

        if (role == "OFFICER" && simulation.CreatedBy != userId)
            throw new UnauthorizedAccessException("No tienes permiso.");

        if (simulation.Status == "CONVERTED")
            throw new InvalidOperationException("Ya fue convertida a préstamo.");

        if (!simulation.ClientId.HasValue)
            throw new InvalidOperationException(
                "Debes asignar un cliente antes de convertir a préstamo.");

        // Crear préstamo desde la simulación
        var loan = new Loan
        {
            SimulationId   = simulation.Id,
            ClientId       = simulation.ClientId!.Value,
            VehicleId      = simulation.VehicleId,
            CreatedBy      = userId,
            Name           = simulation.Name,
            Currency       = simulation.Currency,
            ExchangeRate   = simulation.ExchangeRate,
            VehiclePrice   = simulation.VehiclePrice,
            DownPayment    = simulation.DownPayment,
            AmountFinanced = simulation.AmountFinanced,
            RateType       = simulation.RateType,
            InterestRate   = simulation.InterestRate,
            Capitalization = simulation.Capitalization,
            TermMonths     = simulation.TermMonths,
            GraceType      = simulation.GraceType,
            GraceMonths    = simulation.GraceMonths,
            PaymentMethod  = simulation.PaymentMethod,
            BalloonPct     = simulation.BalloonPct,
            StartDate      = simulation.StartDate,
            Status         = "PENDING_APPROVAL",
            CreatedAt      = DateTime.UtcNow,
            UpdatedAt      = DateTime.UtcNow
        };

        db.Loans.Add(loan);
        await db.SaveChangesAsync();

        // Copiar cronograma al préstamo
        var paymentSchedules = simulation.Schedules.Select(s => new PaymentSchedule
        {
            LoanId                = loan.Id,
            PeriodNumber          = s.PeriodNumber,
            DueDate               = s.DueDate,
            GraceApplied          = s.GraceApplied,
            OpeningBalance        = s.OpeningBalance,
            Interest              = s.Interest,
            Principal             = s.Principal,
            InsuranceDesgravamen  = s.InsuranceDesgravamen,
            InsuranceVehicular    = s.InsuranceVehicular,
            Commission            = s.Commission,
            Balloon               = s.Balloon,
            TotalPayment          = s.TotalPayment,
            ClosingBalance        = s.ClosingBalance,
            IsPaid                = false
        }).ToList();

        db.PaymentSchedules.AddRange(paymentSchedules);

        // Copiar indicadores al préstamo
        if (simulation.Indicator is not null)
        {
            var loanIndicator = new LoanIndicator
            {
                LoanId       = loan.Id,
                Van          = simulation.Indicator.Van,
                TirMonthly   = simulation.Indicator.TirMonthly,
                TirAnnual    = simulation.Indicator.TirAnnual,
                Tcea         = simulation.Indicator.Tcea,
                CokUsed      = simulation.Indicator.CokUsed,
                CalculatedAt = DateTime.UtcNow
            };
            db.LoanIndicators.Add(loanIndicator);
        }

        // Marcar simulación como convertida
        simulation.Status = "CONVERTED";
        await db.SaveChangesAsync();

        return await GetByIdAsync(simulation.Id, userId, role)
            ?? throw new Exception("Error al recuperar simulación.");
    }

    // ── Mapper ─────────────────────────────────────────────────────
    private static SimulationResponse MapToResponse(Simulation s) => new()
    {
        Id             = s.Id,
        Name           = s.Name,
        Status         = s.Status,
        Currency       = s.Currency,
        VehiclePrice   = s.VehiclePrice,
        DownPayment    = s.DownPayment,
        AmountFinanced = s.AmountFinanced,
        RateType       = s.RateType,
        InterestRate   = s.InterestRate,
        Capitalization = s.Capitalization,
        TermMonths     = s.TermMonths,
        GraceType      = s.GraceType,
        GraceMonths    = s.GraceMonths,
        PaymentMethod  = s.PaymentMethod,
        BalloonPct     = s.BalloonPct,
        StartDate      = s.StartDate,
        CreatedAt      = s.CreatedAt,
        CreatedByName  = s.Creator?.Name ?? "—",
        ClientName     = s.Client is not null
            ? $"{s.Client.FirstName} {s.Client.LastName}" : null,
        VehicleName    = s.Vehicle is not null
            ? $"{s.Vehicle.Brand} {s.Vehicle.Model} {s.Vehicle.Year}" : "—",
        Indicators     = s.Indicator is null ? null : new SimulationIndicatorDto
        {
            Van        = s.Indicator.Van,
            TirMonthly = s.Indicator.TirMonthly,
            TirAnnual  = s.Indicator.TirAnnual,
            Tcea       = s.Indicator.Tcea,
            CokUsed    = s.Indicator.CokUsed
        },
        Schedule = s.Schedules.Select(r => new ScheduleRowDto
        {
            PeriodNumber         = r.PeriodNumber,
            GraceApplied         = r.GraceApplied,
            OpeningBalance       = r.OpeningBalance,
            Interest             = r.Interest,
            Principal            = r.Principal,
            InsuranceDesgravamen = r.InsuranceDesgravamen,
            InsuranceVehicular   = r.InsuranceVehicular,
            Commission           = r.Commission,
            Balloon              = r.Balloon,
            TotalPayment         = r.TotalPayment,
            ClosingBalance       = r.ClosingBalance
        }).ToList()
    };
}