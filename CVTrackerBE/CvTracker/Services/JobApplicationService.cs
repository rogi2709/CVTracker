using CvTracker.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace CvTracker.Services;

public class JobApplicationService:IJobApplicationService
{
    private readonly IMongoCollection<JobApplication> _mongoCollection;

    public JobApplicationService(IMongoCollection<JobApplication> mongoCollection)
    {
        _mongoCollection = mongoCollection;
    }


    public async Task<IEnumerable<JobApplication>> GetAllJobApplicationsAsync()
    {
        var operationId = Guid.NewGuid();
        Log.Information("Operation {OperationId}: Retrieving all Job applications", operationId);
        try
        {
            return await _mongoCollection.Find(_ => true).ToListAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Operation {OperationId}: An error occurred while retrieving job applications", operationId);
            throw;
        }
    }

    public async Task<JobApplication> GetJobApplicationByIdAsync(string id)
    {
        var operationId = Guid.NewGuid();
        Log.Information("Operation {OperationId}: Getting job application for user {UserId}", operationId, id);
    
        var objectId = new ObjectId(id);

        try
        {
            return await _mongoCollection.Find(app => app.Id == objectId).FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Operation {OperationId}: An error occurred while retrieving job application for user {UserId}", operationId, id);
            throw;
        }
    }

    public async Task<JobApplication> CreateJobApplicationAsync(JobApplication application)
    {
        var operationId = Guid.NewGuid();
    
        try
        {
            Log.Information("Operation {OperationId}: Creating job application {@JobApplication}", operationId, application);
        
            await _mongoCollection.InsertOneAsync(application);

            Log.Information("Operation {OperationId}: Job application created successfully", operationId);

            return application;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Operation {OperationId}: An error occurred while creating job application {@JobApplication}", operationId, application);
            throw;
        }
    }
    public async Task<bool> UpdateJobApplicationAsync(string id, JobApplication application)
    {
        var objectId = new ObjectId(id);
        var operationId = Guid.NewGuid();

        try
        {
            Log.Information("Operation {OperationId}: Updating job application with ID {JobApplicationId}. New data: {@JobApplication}", operationId, id, application);

            var result = await _mongoCollection.ReplaceOneAsync(app => app.Id == objectId, application);

            if (result.ModifiedCount > 0)
            {
                Log.Information("Operation {OperationId}: Job application updated successfully", operationId);
                return true;
            }
            else
            {
                Log.Information("Operation {OperationId}: Job application not found for update", operationId);
                return false;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Operation {OperationId}: An error occurred while updating job application with ID {JobApplicationId}. New data: {@JobApplication}", operationId, id, application);
            throw;
        }
    }
    public async Task<bool> DeleteJobApplicationAsync(string id)
    {
        var objectId = new ObjectId(id);
        var operationId = Guid.NewGuid();

        try
        {
            Log.Information("Operation {OperationId}: Deleting job application with ID {JobApplicationId}", operationId, id);

            var result = await _mongoCollection.DeleteOneAsync(app => app.Id == objectId);

            if (result.DeletedCount > 0)
            {
                Log.Information("Operation {OperationId}: Job application deleted successfully", operationId);
                return true;
            }
            else
            {
                Log.Information("Operation {OperationId}: Job application not found for delete", operationId);
                return false;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Operation {OperationId}: An error occurred while deleting job application with ID {JobApplicationId}", operationId, id);
            throw;
        }
    }

}