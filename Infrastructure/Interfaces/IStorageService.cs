using RbacApi.Infrastructure.Storage.Models;

namespace RbacApi.Infrastructure.Interfaces;

public interface IStorageService
{
    Task CreateFileInStorageAsync(PutStorageObject storageObject);
    Task<string> GenerateFileUrlAsync(string key);
}