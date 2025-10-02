namespace RbacApi.Infrastructure.Interfaces;

public interface ISigner
{
    string GetSignedUrl(string key);
}