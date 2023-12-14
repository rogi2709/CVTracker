using CvTracker.Models;
using CvTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;

[Route("api/jobApplications")]
[ApiController]
public class JobApplicationController : ControllerBase
{
    private readonly IJobApplicationService _jobApplicationService;

    public JobApplicationController(IJobApplicationService jobApplicationService)
    {
        _jobApplicationService = jobApplicationService;
    }

    [Authorize("default")]
    [HttpGet]
    public async Task<IActionResult> GetAllJobApplications()
    {
        try
        {
            Log.Information("Received request to get all job applications");

            var applications = await _jobApplicationService.GetAllJobApplicationsAsync();

            Log.Information("Returning {Count} job applications", applications?.Count() ?? 0);

            return Ok(applications);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while getting all job applications");
            return BadRequest(ex.Message);
        }
    }

    [Authorize("default")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetJobApplication(string id)
    {
        try
        {
            Log.Information("Received request to get job application with ID {JobApplicationId}", id);

            var application = await _jobApplicationService.GetJobApplicationByIdAsync(id);

            if (application == null)
            {
                Log.Warning("Job application with ID {JobApplicationId} not found", id);
                return NotFound();
            }

            Log.Information("Returning job application with ID {JobApplicationId}", id);

            return Ok(application);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while getting job application with ID {JobApplicationId}", id);
            return BadRequest(ex.Message);
        }
    }

    [Authorize("default")]
    [HttpPost]
    public async Task<IActionResult> CreateJobApplication([FromBody] JobApplication application)
    {
        try
        {
            Log.Information("Received request to create a new job application");

            var createdApplication = await _jobApplicationService.CreateJobApplicationAsync(application);

            Log.Information("Job application created with ID {JobApplicationId}", createdApplication.Id);

            return Created($"api/jobApplications/{createdApplication.Id}", createdApplication);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while creating a new job application");
            return BadRequest(ex.Message);
        }
    }

    [Authorize("default")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateJobApplication(string id, [FromBody] JobApplication application)
    {
        try
        {
            Log.Information("Received request to update job application with ID {JobApplicationId}", id);

            var updated = await _jobApplicationService.UpdateJobApplicationAsync(id, application);

            if (!updated)
            {
                Log.Warning("Job application with ID {JobApplicationId} not found for update", id);
                return NotFound();
            }

            Log.Information("Job application with ID {JobApplicationId} updated successfully", id);

            return Ok("Job application updated successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while updating job application with ID {JobApplicationId}", id);
            return BadRequest(ex.Message);
        }
    }

    [Authorize("default")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJobApplication(string id)
    {
        try
        {
            Log.Information("Received request to delete job application with ID {JobApplicationId}", id);

            var deleted = await _jobApplicationService.DeleteJobApplicationAsync(id);

            if (!deleted)
            {
                Log.Warning("Job application with ID {JobApplicationId} not found for delete", id);
                return NotFound();
            }

            Log.Information("Job application with ID {JobApplicationId} deleted successfully", id);

            return Ok("Job application deleted successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while deleting job application with ID {JobApplicationId}", id);
            return BadRequest(ex.Message);
        }
    }
}
