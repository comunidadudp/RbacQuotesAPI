using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RbacApi.Enums;

namespace RbacApi.Data.Entities;

public class User
{
    [BsonId]
    [BsonElement("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("fullname")]
    public string Fullname { get; set; } = default!;

    [BsonElement("email")]
    public string Email { get; set; } = default!;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = default!;

    [BsonElement("roleId")]
    [BsonRepresentation(BsonType.String)]
    public RoleType RoleId { get; set; }

    [BsonElement("permissions")]
    public List<string> Permissions { get; set; } = [];

    [BsonElement("revokedPermissions")]
    public List<string> RevokedPermissions { get; set; } = [];

    [BsonElement("active")]
    public bool Active { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
}
