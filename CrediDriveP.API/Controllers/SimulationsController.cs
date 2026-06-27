using CrediDriveP.API.DTOs.Simulation;
using CrediDriveP.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrediDriveP.API.Controllers;

[ApiController]
[Route("api/simulations")]
[Authorize]
public class SimulationsController(ISimulationService simulationService) : ControllerBase
{
    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string UserRole =>
        User.FindFirstValue(ClaimTypes.Role)!;

    // GET /api/simulations
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await simulationService.GetAllAsync(UserId, UserRole);
        return Ok(result);
    }

    // GET /api/simulations/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await simulationService.GetByIdAsync(id, UserId, UserRole);
            return result is null
                ? NotFound(new { message = "Simulación no encontrada." })
                : Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // POST /api/simulations
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSimulationRequest request)
    {
        try
        {
            var result = await simulationService.CreateAsync(request, UserId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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

    // DELETE /api/simulations/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await simulationService.DeleteAsync(id, UserId, UserRole);
            return Ok(new { message = "Simulación eliminada." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/simulations/{id}/convert
    [HttpPost("{id:int}/convert")]
    public async Task<IActionResult> Convert(int id)
    {
        try
        {
            var result = await simulationService.ConvertToLoanAsync(id, UserId, UserRole);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}