using MongoDB.Bson.Serialization.Attributes;

namespace RbacApi.Data.Entities;

public class AuditDetail
{
    [BsonElement("message")]
    public string? Message { get; set; }

    [BsonElement("request")]
    public Dictionary<string, object>? Request { get; set; }

    [BsonElement("response")]
    public  Dictionary<string, object>? Response { get; set; }

    [BsonElement("error")]
    public string? Error { get; set; }
}
