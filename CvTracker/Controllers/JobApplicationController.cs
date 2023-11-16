using CvTracker.Models;
using CvTracker.Services;
using Microsoft.AspNetCore.Authorization;

namespace CvTracker.Controllers;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
            var applications = await _jobApplicationService.GetAllJobApplicationsAsync();
            return Ok(applications);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [Authorize("default")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetJobApplication(string id)
    {
        try
        {
            var application = await _jobApplicationService.GetJobApplicationByIdAsync(id);
            if (application == null)
                return NotFound();

            return Ok(application);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [Authorize("default")]
    [HttpPost]
    public async Task<IActionResult> CreateJobApplication([FromBody] JobApplication application)
    {
        try
        {

            var createdApplication = await _jobApplicationService.CreateJobApplicationAsync(application);
        
            return Created($"api/jobApplications/{createdApplication.Id}", createdApplication);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }



    [Authorize("default")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateJobApplication(string id, [FromBody] JobApplication application)
    {
        try
        {
            var updated = await _jobApplicationService.UpdateJobApplicationAsync(id, application);
            if (!updated)
                return NotFound();

            return Ok("Job application updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [Authorize("default")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJobApplication(string id)
    {
        try
        {
            var deleted = await _jobApplicationService.DeleteJobApplicationAsync(id);
            if (!deleted)
                return NotFound();

            return Ok("Job application deleted successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
