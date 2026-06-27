using CrediDriveP.API.DTOs.Loan;

namespace CrediDriveP.API.Interfaces;

public interface ILoanService
{
    Task<List<LoanSummaryResponse>> GetAllAsync(int userId, string role);
    Task<LoanDetailResponse?> GetByIdAsync(int id, int userId, string role);
    Task<LoanDetailResponse> UpdateStatusAsync(
        int id, UpdateLoanStatusRequest request, int approvedBy, string role);
}