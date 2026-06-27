using CrediDriveP.API.DTOs.Auth;
using CrediDriveP.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrediDriveP.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // POST /api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        if (result is null)
            return Unauthorized(new { message = "Credenciales incorrectas." });

        return Ok(result);
    }

    // GET /api/auth/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await authService.GetMeAsync(userId);
        return result is null ? NotFound() : Ok(result);
    }

    // POST /api/auth/officers  ← solo ADMIN
    [HttpPost("officers")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateOfficer([FromBody] CreateOfficerRequest request)
    {
        try
        {
            var result = await authService.CreateOfficerAsync(request);
            return CreatedAtAction(nameof(GetOfficers), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /api/auth/officers  ← solo ADMIN
    [HttpGet("officers")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetOfficers()
    {
        var result = await authService.GetOfficersAsync();
        return Ok(result);
    }

    // PUT /api/auth/officers/{id}  ← solo ADMIN
    [HttpPut("officers/{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> UpdateOfficer(int id, [FromBody] UpdateOfficerRequest request)
    {
        try
        {
            var result = await authService.UpdateOfficerAsync(id, request);
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

    // PATCH /api/auth/officers/{id}/toggle  ← solo ADMIN
    [HttpPatch("officers/{id:int}/toggle")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> ToggleOfficer(int id)
    {
        try
        {
            var result = await authService.ToggleOfficerAsync(id);
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

    // PUT /api/auth/officers/{id}/reset-password  ← solo ADMIN
    [HttpPut("officers/{id:int}/reset-password")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
    {
        try
        {
            await authService.ResetPasswordAsync(id, request.NewPassword);
            return Ok(new { message = "Contraseña actualizada correctamente." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}