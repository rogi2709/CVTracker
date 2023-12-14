namespace CvTracker.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class JobApplication
{
    [BsonId]
    public ObjectId Id { get; set; }
    public string JobTitle { get; set; }
    public string Notes { get; set; }
    public string Location { get; set; }
    public string Link { get; set; }
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }
}
