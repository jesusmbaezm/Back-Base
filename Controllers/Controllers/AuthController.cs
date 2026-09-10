using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Auth;
using Services.DTOs.Shared;
using Services.Services.Interfaces;

namespace Controllers.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (result is null)
        {
            return UnauthorizedResponse("Credenciales inválidas");
        }

        return OkResponse(result);
    }

    [Authorize]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader["Bearer ".Length..].Trim()
            : string.Empty;

        if (string.IsNullOrEmpty(token))
            return UnauthorizedResponse("Token no proporcionado.");

        var result = await _authService.RefreshTokenAsync(token);
        return OkResponse(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        await _authService.ForgotPasswordAsync(request);
        return OkResponse(ApiResponse.SuccessEmpty("Si el correo existe y esta activo, se enviaron instrucciones de recuperacion."));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var response = await _authService.ResetPasswordAsync(request);
        if (response)
            return SuccessEmptyResponse("Contrasena actualizada correctamente.");
        else
            return BadRequest();
    }
}
