using CrediDriveP.API.DTOs.Insurance;
using CrediDriveP.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrediDriveP.API.Controllers;

[ApiController]
[Route("api/insurances")]
[Authorize]
public class InsurancesController(IInsuranceService insuranceService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await insuranceService.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await insuranceService.GetByIdAsync(id);
        return result is null
            ? NotFound(new { message = "Seguro no encontrado." })
            : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] InsuranceRequest request)
    {
        var result = await insuranceService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(int id, [FromBody] InsuranceRequest request)
    {
        try
        {
            var result = await insuranceService.UpdateAsync(id, request);
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
            var result = await insuranceService.ToggleAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}