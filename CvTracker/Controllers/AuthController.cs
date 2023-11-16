using System.Threading.Tasks;
using CvTracker.Models;
using CvTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
            var accessToken = await _userService.CreateUserAsync(userInput);
            
            return Ok(new { AccessToken = accessToken });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginModel userLoginModel)
    {
        try
        {
            var accessToken = await _userService.LoginAsync(userLoginModel);
            return Ok(new { AccessToken = accessToken });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }
}