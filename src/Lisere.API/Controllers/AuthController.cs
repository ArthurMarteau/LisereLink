using Lisere.Application.DTOs;
using Lisere.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Lisere.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        var userId = await _authService.RegisterAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Register), new { id = userId }, new { id = userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(dto, cancellationToken);
        return Ok(response);
    }
}
