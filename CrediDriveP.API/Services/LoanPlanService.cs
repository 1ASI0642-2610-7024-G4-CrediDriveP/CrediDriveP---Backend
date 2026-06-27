using CrediDriveP.API.Data;
using CrediDriveP.API.DTOs.Commission;
using CrediDriveP.API.DTOs.Insurance;
using CrediDriveP.API.DTOs.LoanPlan;
using CrediDriveP.API.Interfaces;
using CrediDriveP.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CrediDriveP.API.Services;

public class LoanPlanService(AppDbContext db) : ILoanPlanService
{
    public async Task<List<LoanPlanResponse>> GetAllAsync()
    {
        return await db.LoanPlans
            .Include(lp => lp.Creator)
            .Include(lp => lp.LoanPlanInsurances)
                .ThenInclude(lpi => lpi.Insurance)
            .Include(lp => lp.LoanPlanCommissions)
                .ThenInclude(lpc => lpc.Commission)
            .OrderByDescending(lp => lp.CreatedAt)
            .Select(lp => MapToResponse(lp))
            .ToListAsync();
    }

    public async Task<LoanPlanResponse?> GetByIdAsync(int id)
    {
        var plan = await db.LoanPlans
            .Include(lp => lp.Creator)
            .Include(lp => lp.LoanPlanInsurances)
                .ThenInclude(lpi => lpi.Insurance)
            .Include(lp => lp.LoanPlanCommissions)
                .ThenInclude(lpc => lpc.Commission)
            .FirstOrDefaultAsync(lp => lp.Id == id);

        return plan is null ? null : MapToResponse(plan);
    }

    public async Task<LoanPlanResponse> CreateAsync(LoanPlanRequest request, int createdBy)
    {
        var plan = new LoanPlan
        {
            Name              = request.Name,
            Currency          = request.Currency.ToUpper(),
            RateType          = request.RateType.ToUpper(),
            InterestRate      = request.InterestRate,
            Capitalization    = request.Capitalization?.ToUpper(),
            TermMonths        = request.TermMonths,
            GraceType         = request.GraceType.ToUpper(),
            GraceMonths       = request.GraceMonths,
            PaymentMethod     = request.PaymentMethod.ToUpper(),
            BalloonPercentage = request.BalloonPercentage,
            CokAnnual         = request.CokAnnual,
            IsActive          = true,
            CreatedBy         = createdBy,
            CreatedAt         = DateTime.UtcNow
        };

        db.LoanPlans.Add(plan);
        await db.SaveChangesAsync();

        // Asociar seguros
        foreach (var insuranceId in request.InsuranceIds)
        {
            db.LoanPlanInsurances.Add(new LoanPlanInsurance
            {
                PlanId      = plan.Id,
                InsuranceId = insuranceId
            });
        }

        // Asociar comisiones
        foreach (var commissionId in request.CommissionIds)
        {
            db.LoanPlanCommissions.Add(new LoanPlanCommission
            {
                PlanId       = plan.Id,
                CommissionId = commissionId
            });
        }

        await db.SaveChangesAsync();

        return await GetByIdAsync(plan.Id)
            ?? throw new Exception("Error al recuperar el plan.");
    }

    public async Task<LoanPlanResponse> UpdateAsync(int id, LoanPlanRequest request)
    {
        var plan = await db.LoanPlans
            .Include(lp => lp.LoanPlanInsurances)
            .Include(lp => lp.LoanPlanCommissions)
            .FirstOrDefaultAsync(lp => lp.Id == id)
            ?? throw new KeyNotFoundException("Plan no encontrado.");

        plan.Name              = request.Name;
        plan.Currency          = request.Currency.ToUpper();
        plan.RateType          = request.RateType.ToUpper();
        plan.InterestRate      = request.InterestRate;
        plan.Capitalization    = request.Capitalization?.ToUpper();
        plan.TermMonths        = request.TermMonths;
        plan.GraceType         = request.GraceType.ToUpper();
        plan.GraceMonths       = request.GraceMonths;
        plan.PaymentMethod     = request.PaymentMethod.ToUpper();
        plan.BalloonPercentage = request.BalloonPercentage;
        plan.CokAnnual         = request.CokAnnual;

        // Reemplazar seguros
        db.LoanPlanInsurances.RemoveRange(plan.LoanPlanInsurances);
        foreach (var insuranceId in request.InsuranceIds)
        {
            db.LoanPlanInsurances.Add(new LoanPlanInsurance
            {
                PlanId      = plan.Id,
                InsuranceId = insuranceId
            });
        }

        // Reemplazar comisiones
        db.LoanPlanCommissions.RemoveRange(plan.LoanPlanCommissions);
        foreach (var commissionId in request.CommissionIds)
        {
            db.LoanPlanCommissions.Add(new LoanPlanCommission
            {
                PlanId       = plan.Id,
                CommissionId = commissionId
            });
        }

        await db.SaveChangesAsync();

        return await GetByIdAsync(plan.Id)
            ?? throw new Exception("Error al recuperar el plan.");
    }

    public async Task<LoanPlanResponse> ToggleAsync(int id)
    {
        var plan = await db.LoanPlans
            .Include(lp => lp.Creator)
            .Include(lp => lp.LoanPlanInsurances)
                .ThenInclude(lpi => lpi.Insurance)
            .Include(lp => lp.LoanPlanCommissions)
                .ThenInclude(lpc => lpc.Commission)
            .FirstOrDefaultAsync(lp => lp.Id == id)
            ?? throw new KeyNotFoundException("Plan no encontrado.");

        plan.IsActive = !plan.IsActive;
        await db.SaveChangesAsync();
        return MapToResponse(plan);
    }

    private static LoanPlanResponse MapToResponse(LoanPlan lp) => new()
    {
        Id                = lp.Id,
        Name              = lp.Name,
        Currency          = lp.Currency,
        RateType          = lp.RateType,
        InterestRate      = lp.InterestRate,
        Capitalization    = lp.Capitalization,
        TermMonths        = lp.TermMonths,
        GraceType         = lp.GraceType,
        GraceMonths       = lp.GraceMonths,
        PaymentMethod     = lp.PaymentMethod,
        BalloonPercentage = lp.BalloonPercentage,
        CokAnnual         = lp.CokAnnual,
        IsActive          = lp.IsActive,
        CreatedByName     = lp.Creator?.Name ?? "—",
        CreatedAt         = lp.CreatedAt,
        Insurances = lp.LoanPlanInsurances.Select(lpi => new InsuranceResponse
        {
            Id          = lpi.Insurance.Id,
            Name        = lpi.Insurance.Name,
            Type        = lpi.Insurance.Type,
            Rate        = lpi.Insurance.Rate,
            Base        = lpi.Insurance.Base,
            IsMandatory = lpi.Insurance.IsMandatory,
            IsActive    = lpi.Insurance.IsActive
        }).ToList(),
        Commissions = lp.LoanPlanCommissions.Select(lpc => new CommissionResponse
        {
            Id          = lpc.Commission.Id,
            Concept     = lpc.Commission.Concept,
            Amount      = lpc.Commission.Amount,
            Periodicity = lpc.Commission.Periodicity,
            IsActive    = lpc.Commission.IsActive
        }).ToList()
    };
}