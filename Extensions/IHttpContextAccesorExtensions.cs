using RbacApi.Data.Entities;

namespace RbacApi.Extensions;

public static class IHttpContextAccesorExtensions
{
    public static void AddAuditExtraItems(this IHttpContextAccessor accessor,
        IEnumerable<KeyValuePair<string, object?>> items)
    {
        var extras = accessor.HttpContext!.Items["AuditExtra"] as Dictionary<string, object?>;
        extras ??= [];

        foreach (var item in items)
        {
            extras.Add(item.Key, item.Value);
        }
        accessor.HttpContext.Items["AuditExtra"] = extras;
    }

    public static void AddAuditChangeItems(this IHttpContextAccessor accessor,
        IEnumerable<AuditChange> auditChanges)
    {
        var changes = new Dictionary<string, object?>();
        foreach (var item in auditChanges)
        {
            changes.Add(Guid.NewGuid().ToString(), item);
        }
        accessor.HttpContext!.Items["AuditChanges"] = changes;
    }
}