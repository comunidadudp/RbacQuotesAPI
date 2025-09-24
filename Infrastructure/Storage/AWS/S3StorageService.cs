using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using RbacApi.Infrastructure.Interfaces;
using RbacApi.Infrastructure.Storage.Models;

namespace RbacApi.Infrastructure.Storage.AWS;

public sealed class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _amazonS3;
    private readonly S3BucketOptions _bucketOptions;

    public S3StorageService(IAmazonS3 amazonS3, IOptions<S3BucketOptions> options)
    {
        _amazonS3 = amazonS3;
        _bucketOptions = options.Value;
    }

    public async Task CreateFileInStorageAsync(PutStorageObject storageObject)
    {
        var objectRequest = new PutObjectRequest
        {
            BucketName = _bucketOptions.Name,
            ContentType = storageObject.ContentType,
            InputStream = storageObject.File,
            Key = storageObject.FileName,
            CannedACL = S3CannedACL.Private,
            Headers = { CacheControl = "public, max-age=3600" }
        };

        _ = await _amazonS3.PutObjectAsync(objectRequest);
    }

    public async Task<string> GenerateFileUrlAsync(string key)
    {
        var presignedUrlRequest = new GetPreSignedUrlRequest
        {
            BucketName = _bucketOptions.Name,
            Key = key,
            Expires = AddExpiryDate(_bucketOptions.UrlExpirationTimeUnit, _bucketOptions.UrlExpirationTime),
            Verb = HttpVerb.GET
        };

        return await _amazonS3.GetPreSignedURLAsync(presignedUrlRequest);
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