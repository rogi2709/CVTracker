using CvTracker.Models;
using CvTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CvTracker.Controllers;

[Route("api/user")]
[ApiController]
public class UserController:ControllerBase
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
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
    [Authorize("default")]
    [HttpGet("applications/{userId}")]
    public async Task<IActionResult> GetJobApplicationsForUser(string userId)
    {
        try
        {
            var jobApplications = await _userService.GetJobApplicationsForUserAsync(userId);
            return Ok(jobApplications.ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message); 
        }
    }

}