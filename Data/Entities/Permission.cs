using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RbacApi.Data.Entities;

public class Permission
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Code { get; set; } = default!;

    [BsonElement("description")]
    public string Description { get; set; } = default!;
}