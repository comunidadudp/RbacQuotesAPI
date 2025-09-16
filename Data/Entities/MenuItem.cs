using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RbacApi.Data.Entities;

public class MenuItem
{
    [BsonId]
    [BsonElement("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("label")]
    public string Label { get; set; } = default!;

    [BsonElement("icon")]
    public string? Icon { get; set; }

    [BsonElement("route")]
    public string Route { get; set; } = default!;

    [BsonElement("order")]
    public int Order { get; set; }

    [BsonElement("requiredPermissions")]
    public List<string> RequiredPermissions { get; set; } = [];

    [BsonElement("children")]
    public List<MenuItem> Children { get; set; } = [];
}
