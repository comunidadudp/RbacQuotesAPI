using MongoDB.Bson.Serialization.Attributes;

namespace RbacApi.Data.Entities;

public class AuditChange
{
    [BsonElement("field")]
    public string? Field { get; set; }

    [BsonElement("before")]
    public object? Before { get; set; }

    [BsonElement("after")]
    public object? After { get; set; }

    [BsonElement("note")]
    public string? Note { get; set; }
}
