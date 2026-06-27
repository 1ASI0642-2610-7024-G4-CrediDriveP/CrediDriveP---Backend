using CrediDriveP.API.DTOs.LoanPlan;
using CrediDriveP.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrediDriveP.API.Controllers;

[ApiController]
[Route("api/loan-plans")]
[Authorize]
public class LoanPlansController(ILoanPlanService loanPlanService) : ControllerBase
{
    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await loanPlanService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await loanPlanService.GetByIdAsync(id);
        return result is null
            ? NotFound(new { message = "Plan no encontrado." })
            : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] LoanPlanRequest request)
    {
        var result = await loanPlanService.CreateAsync(request, UserId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(int id, [FromBody] LoanPlanRequest request)
    {
        try
        {
            var result = await loanPlanService.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/toggle")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Toggle(int id)
    {
        try
        {
            var result = await loanPlanService.ToggleAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}