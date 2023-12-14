using CvTracker.Models;
using CvTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace CvTracker.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }
    
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserInputModel userInput)
    {
        try
        {
            Log.ForContext("SensitiveInfo", new { userInput.Username, userInput.Email })
                .Information("Registration attempt for username: {Username}, email: {Email}", userInput.Username, userInput.Email);
            var user = await _userService.CreateUserAsync(userInput);
            Log.ForContext("SensitiveInfo", new { userInput.Username, userInput.Email })
                .Information("User successfully registered. Username: {Username}, email: {Email}", userInput.Username, userInput.Email);
            return Ok(new { User = user });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Registration failed for username: {Username}, email: {Email}", userInput.Username, userInput.Email);
            return BadRequest(new { Message = ex.Message });
        }
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginModel userLoginModel)
    {
        try
        {
            Log.ForContext("SensitiveInfo", new { userLoginModel.Username })
                .Information("Login attempt for username: {Username}");

            var accessToken = await _userService.LoginAsync(userLoginModel);
            
            Log.ForContext("SensitiveInfo", new { userLoginModel.Username })
                .Information("User successfully logged in. Username: {Username}");

            return Ok(new { AccessToken = accessToken.AccessToken, accessToken.RefreshToken });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Login failed for username: {Username}", userLoginModel.Username);

            return Unauthorized(new { Message = ex.Message });
        }
    }
    
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel refreshTokenModel)
    {
        try
        {
            Log.ForContext("SensitiveInfo", new { refreshTokenModel.RefreshToken })
                .Information("Refresh token attempt");

            var newAccessToken = await _userService.RefreshAccessTokenAsync(refreshTokenModel.RefreshToken);
            return Ok(new { AccessToken = newAccessToken.AccessToken, newAccessToken.RefreshToken });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error refreshing token");
            return Unauthorized(new { Message = ex.Message });
        }
    }

}