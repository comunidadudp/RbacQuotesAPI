using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RbacApi.Enums;

namespace RbacApi.Data.Entities;

public class AuditActor
{
    [BsonElement("actorId")]
    public string? ActorId { get; set; }

    [BsonElement("actorType")]
    [BsonRepresentation(BsonType.String)]
    public ActorType? ActorType { get; set; }

    [BsonElement("username")]
    public string? Username { get; set; }

    [BsonElement("permissions")]
    public List<string>? Permissions { get; set; }

    [BsonElement("extra")]
    public Dictionary<string, object>? Extra { get; set; }
}
