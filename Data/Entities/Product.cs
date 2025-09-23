using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RbacApi.Data.Entities;

public class Product
{
    [BsonId]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal BasePrice { get; set; }
}
