using CodingChallenge.Api.Services;
using CodingChallenge.Api.Services.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CodingChallenge.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public ActionResult<LoginResultDto> Login([FromBody] LoginRequestDto dto)
    {
        _logger.LogInformation("POST /api/auth/login body: {@Login}", dto);

        var result = _authService.Login(dto.Username, dto.Password);
        return result is null ? Unauthorized(new { message = "Invalid user name or password." }) : Ok(result);
    }
}
