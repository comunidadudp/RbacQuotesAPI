namespace RbacApi.Infrastructure.Storage.AWS;

public class CloudFrontConfig
{
    public string Resource { get; init; } = string.Empty;
    public string KeyPairId { get; init; } = string.Empty;
    public string PrivateKeyRoute { get; init; } = string.Empty;
    public int UrlExpirationTime { get; init; }
    public UrlExpirationTimeUnits UrlExpirationTimeUnit { get; init; }
}