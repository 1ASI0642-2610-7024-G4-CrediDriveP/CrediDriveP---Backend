using CrediDriveP.API.DTOs.LoanPlan;

namespace CrediDriveP.API.Interfaces;

public interface ILoanPlanService
{
    Task<List<LoanPlanResponse>> GetAllAsync();
    Task<LoanPlanResponse?> GetByIdAsync(int id);
    Task<LoanPlanResponse> CreateAsync(LoanPlanRequest request, int createdBy);
    Task<LoanPlanResponse> UpdateAsync(int id, LoanPlanRequest request);
    Task<LoanPlanResponse> ToggleAsync(int id);
}