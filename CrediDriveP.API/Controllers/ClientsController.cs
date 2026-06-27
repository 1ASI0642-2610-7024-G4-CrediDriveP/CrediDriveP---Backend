using CrediDriveP.API.DTOs.Client;
using CrediDriveP.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrediDriveP.API.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController(IClientService clientService) : ControllerBase
{
    // GET /api/clients
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await clientService.GetAllAsync();
        return Ok(result);
    }

    // GET /api/clients/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await clientService.GetByIdAsync(id);
        return result is null ? NotFound(new { message = "Cliente no encontrado." }) : Ok(result);
    }

    // POST /api/clients
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await clientService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT /api/clients/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientRequest request)
    {
        try
        {
            var result = await clientService.UpdateAsync(id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // DELETE /api/clients/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Toggle(int id)
    {
        try
        {
            var result = await clientService.ToggleAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}