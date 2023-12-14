using CvTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [Authorize("default")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        try
        {
            Log.Information("Received request to get user with ID {UserId}", id);

            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                Log.Warning("User with ID {UserId} not found", id);
                return NotFound(new { Message = "User not found" });
            }

            Log.Information("Returning user with ID {UserId}", id);

            return Ok(user);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while getting user with ID {UserId}", id);
            return NotFound(new { Message = ex.Message });
        }
    }

    [Authorize("default")]
    [HttpGet("applications/{userId}")]
    public async Task<IActionResult> GetJobApplicationsForUser(string userId)
    {
        try
        {
            Log.Information("Received request to get job applications for user with ID {UserId}", userId);

            var jobApplications = await _userService.GetJobApplicationsForUserAsync(userId);

            Log.Information("Returning job applications for user with ID {UserId}", userId);

            return Ok(jobApplications.ToList());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while getting job applications for user with ID {UserId}", userId);
            return StatusCode(500, ex.Message);
        }
    }
}