using CrediDriveP.API.DTOs.Loan;
using CrediDriveP.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrediDriveP.API.Controllers;

[ApiController]
[Route("api/loans")]
[Authorize]
public class LoansController(ILoanService loanService) : ControllerBase
{
    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string UserRole =>
        User.FindFirstValue(ClaimTypes.Role)!;

    // GET /api/loans
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await loanService.GetAllAsync(UserId, UserRole);
        return Ok(result);
    }

    // GET /api/loans/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await loanService.GetByIdAsync(id, UserId, UserRole);
            return result is null
                ? NotFound(new { message = "Préstamo no encontrado." })
                : Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // PUT /api/loans/{id}/status  ← ADMIN aprueba/rechaza, ambos pueden cancelar
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id, [FromBody] UpdateLoanStatusRequest request)
    {
        try
        {
            var result = await loanService.UpdateStatusAsync(id, request, UserId, UserRole);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}