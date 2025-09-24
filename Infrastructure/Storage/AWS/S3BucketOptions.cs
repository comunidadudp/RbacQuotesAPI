namespace RbacApi.Infrastructure.Storage.AWS;

public class S3BucketOptions
{
    public string Name { get; init; } = string.Empty;
    public int UrlExpirationTime { get; init; }
    public UrlExpirationTimeUnits UrlExpirationTimeUnit { get; init; }
}