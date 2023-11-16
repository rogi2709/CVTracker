using CvTracker.Models;
using MongoDB.Bson;
using MongoDB.Driver;

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
        return await _mongoCollection.Find(_ => true).ToListAsync();
    }

    public async Task<JobApplication> GetJobApplicationByIdAsync(string id)
    {
        var objectId = new ObjectId(id);
        return await _mongoCollection.Find(app => app.Id == objectId).FirstOrDefaultAsync();
    }
    public async Task<JobApplication> CreateJobApplicationAsync(JobApplication application)
    {
        await _mongoCollection.InsertOneAsync(application);
        return application;
    }

    public async Task<bool> UpdateJobApplicationAsync(string id, JobApplication application)
    {
        var objectId = new ObjectId(id);
        var result = await _mongoCollection.ReplaceOneAsync(app => app.Id == objectId, application);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteJobApplicationAsync(string id)
    {
        var objectId = new ObjectId(id);
        var result = await _mongoCollection.DeleteOneAsync(app => app.Id == objectId);
        return result.DeletedCount > 0;
    }
}