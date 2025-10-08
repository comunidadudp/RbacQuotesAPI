using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RbacApi.Data;
using RbacApi.Handlers.Models;

namespace RbacApi.Providers;

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    private readonly CollectionsProvider _collections;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };


    public PermissionPolicyProvider(
        IOptions<AuthorizationOptions> options,
        CollectionsProvider collections,
        IMemoryCache cache)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        _collections = collections;
        _cache = cache;
    }


    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var fallback = await _fallbackPolicyProvider.GetPolicyAsync(policyName);
        if (fallback != null)
            return fallback;

        var cacheKey = $"perm-policy:{policyName}";
        if (_cache.TryGetValue<AuthorizationPolicy>(cacheKey, out var cachedPolicy))
            return cachedPolicy;

        var permission = await _collections.Permissions.Find(p => p.Code == policyName).FirstOrDefaultAsync();

        if (permission == null)
            return null;

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(permission.Code))
            .Build();

        _cache.Set(cacheKey, policy, _cacheOptions);
        return policy;
    }
}
