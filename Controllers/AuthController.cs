using System.Security.Claims;
using AuthStream.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrekReserve.Auth.Services;

namespace TrekReserve.Auth.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthenticationService _authenticationService;
    public AuthController(AuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [HttpPost("Register")]
    public async Task<ActionResult<ServiceResponse<string>>> RegisterAsync(RegisterForm registerForm)
    {
        var serviceResponse = await _authenticationService.RegisterAsync(registerForm);

        return GenerateResponseCode(serviceResponse);
    }

    [HttpPost("Login")]
    public async Task<ActionResult<ServiceResponse<string>>> LoginAsync(LoginForm loginForm)
    {
        var serviceResponse = await _authenticationService.LoginAsync(loginForm);

        return GenerateResponseCode(serviceResponse);
    }

    [HttpPut("Verify/{confirmationToken}")]
    public async Task<ActionResult<ServiceResponse<string>>> VerifyEmail(string confirmationToken)
    {
        var serviceResponse = await _authenticationService.VerifyEmailAsync(confirmationToken);

        return GenerateResponseCode(serviceResponse);
    }

    [Authorize]
    [HttpPost("ResendConfirmationEmail")]
    public async Task<ActionResult<ServiceResponse<string>>> ResendConfirmationEmail()
    {
        var serviceResponse = await _authenticationService.ResendConfirmationEmail(HttpContext.User.Claims);

        return GenerateResponseCode(serviceResponse);
    }

    private ActionResult<ServiceResponse<string>> GenerateResponseCode(ServiceResponse<string> serviceResponse)
    {
        if (serviceResponse.Success is false)
        {
            return BadRequest(serviceResponse);
        }

        return Ok(serviceResponse);
    }
}