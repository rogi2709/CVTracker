using CvTracker.Models;
using MongoDB.Bson;

namespace CvTracker.Services;

public interface IJobApplicationService
{
    Task<IEnumerable<JobApplication>> GetAllJobApplicationsAsync();
    Task<JobApplication> GetJobApplicationByIdAsync(string id);
    Task<JobApplication> CreateJobApplicationAsync(JobApplication application);
    Task<bool> UpdateJobApplicationAsync(string id, JobApplication application);
    Task<bool> DeleteJobApplicationAsync(string id);
}