using MongoDB.Driver;
using RbacApi.Data;
using RbacApi.Data.Entities;
using RbacApi.Services.Interfaces;

namespace RbacApi.Services;

public class PermissionService : IPermissionService
{
    private readonly CollectionsProvider _collections;

    public PermissionService(CollectionsProvider collections)
    {
        _collections = collections;
    }

    public async Task<IReadOnlyList<Permission>> GetAllAsync()
        => await _collections.Permissions.Find(FilterDefinition<Permission>.Empty).ToListAsync();

    public async Task<Permission?> GetByIdAsync(string id)
        => await _collections.Permissions.Find(p => p.Code == id).FirstOrDefaultAsync();
}
