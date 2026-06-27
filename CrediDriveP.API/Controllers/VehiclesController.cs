using CrediDriveP.API.DTOs.Vehicle;
using CrediDriveP.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrediDriveP.API.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclesController(IVehicleService vehicleService) : ControllerBase
{
    // GET /api/vehicles?status=AVAILABLE&brand=Toyota
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? brand)
    {
        var result = await vehicleService.GetAllAsync(status, brand);
        return Ok(result);
    }

    // GET /api/vehicles/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await vehicleService.GetByIdAsync(id);
        return result is null ? NotFound(new { message = "Vehículo no encontrado." }) : Ok(result);
    }

    // POST /api/vehicles
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await vehicleService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT /api/vehicles/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleRequest request)
    {
        try
        {
            var result = await vehicleService.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // DELETE /api/vehicles/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Toggle(int id)
    {
        try
        {
            var result = await vehicleService.ToggleAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}