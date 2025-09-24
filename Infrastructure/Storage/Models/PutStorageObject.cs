namespace RbacApi.Infrastructure.Storage.Models;

public record PutStorageObject(
    string FileName,
    Stream File,
    string ContentType
);