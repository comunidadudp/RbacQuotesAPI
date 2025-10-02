using Amazon.CloudFront;
using Microsoft.Extensions.Options;
using RbacApi.Infrastructure.Interfaces;

namespace RbacApi.Infrastructure.Storage.AWS;

public class CloudFrontUrlSigner(IOptions<CloudFrontConfig> options) : ISigner
{
    private readonly CloudFrontConfig _config = options.Value;

    public string GetSignedUrl(string key)
    {
        using var reader = new StreamReader(_config.PrivateKeyRoute);
        string signedUrl = AmazonCloudFrontUrlSigner.GetCannedSignedURL(
            $"https://{_config.Resource}/{key}",
            reader,
            _config.KeyPairId,
            AddExpiryDate(_config.UrlExpirationTimeUnit, _config.UrlExpirationTime)
        );
        return signedUrl;
    }

    private static DateTime AddExpiryDate(UrlExpirationTimeUnits unit, int urlExpirationTime)
    {
        return unit switch
        {
            UrlExpirationTimeUnits.Year => DateTime.UtcNow.AddYears(urlExpirationTime),
            UrlExpirationTimeUnits.Month => DateTime.UtcNow.AddMonths(urlExpirationTime),
            UrlExpirationTimeUnits.Day => DateTime.UtcNow.AddDays(urlExpirationTime),
            UrlExpirationTimeUnits.Hour => DateTime.UtcNow.AddHours(urlExpirationTime),
            UrlExpirationTimeUnits.Minute => DateTime.UtcNow.AddMinutes(urlExpirationTime),
            _ => DateTime.UtcNow
        };
    }
}