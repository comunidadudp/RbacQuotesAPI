using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RbacApi.Data.Entities;

public class RefreshToken
{
    [BsonId]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = default!;

    [BsonElement("token")]
    public string Token { get; set; } = default!;

    [BsonElement("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("revoked")]
    public bool Revoked { get; set; } = false;

    [BsonElement("replacedByToken")]
    public string? ReplacedByToken { get; set; } = default!;
}
