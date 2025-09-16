using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RbacApi.Enums;

namespace RbacApi.Data.Entities;

public class Role
{
    [BsonId]
    [BsonElement("_id")]
    [BsonRepresentation(BsonType.String)]
    public RoleType Id { get; set; }

    [BsonElement("Name")]
    public string Name { get; set; } = default!;

    public List<string> Permissions { get; set; } = [];
}