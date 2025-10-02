using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RbacApi.Enums;

namespace RbacApi.Data.Entities;

public class AuditLog
{
    [BsonId]
    [BsonElement("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("correlationId")]
    public string? CorrelationId { get; set; }

    [BsonElement("traceParent")]
    public string? TraceParent { get; set; }

     [BsonElement("operationId")]
    public string? OperationId { get; set; }


    [BsonElement("action")]
    [BsonRepresentation(BsonType.String)]
    public ActionType Action { get; set; }

    [BsonElement("actor")]
    public AuditActor? Actor { get; set; }

    [BsonElement("entityType")]
    public string? EntityType { get; set; }

    [BsonElement("entityId")]
    public string? EntityId { get; set; }

    [BsonElement("outcome")]
    [BsonRepresentation(BsonType.String)]
    public OutcomeType Outcome { get; set; }

    [BsonElement("severity")]
    [BsonRepresentation(BsonType.String)]
    public SeverityLevel Severity { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("durationsMs")]
    public long? DurationsMs { get; set; }

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }

    [BsonElement("clientId")]
    public string? ClientId { get; set; }

    [BsonElement("details")]
    public AuditDetail? Details { get; set; }

    [BsonElement("changes")]
    [BsonIgnoreIfNull]
    public List<AuditChange>? Changes { get; set; }

    [BsonElement("tags")]
    public List<string>? Tags { get; set; }

    [BsonElement("metada")]
    public Dictionary<string, object>? Metadata { get; set; }

    [BsonElement("processed")]
    public bool Processed { get; set; } = false;

    [BsonElement("processedAt")]
    public DateTime? ProcessedAt { get; set; }
}
