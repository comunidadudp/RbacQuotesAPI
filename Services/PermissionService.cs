using RbacApi.Data.Entities;
using RbacApi.Data.Interfaces;
using RbacApi.Services.Interfaces;

namespace RbacApi.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;

    public PermissionService(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<IReadOnlyList<Permission>> GetAllAsync()
        => await _permissionRepository.GetAllAsync();

    public async Task<Permission?> GetByIdAsync(string id)
        => await _permissionRepository.GetByIdAsync(id);
}
