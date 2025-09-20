using Microsoft.AspNetCore.Mvc.Filters;

namespace RbacApi.Filters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class AuditAttribute : Attribute, IAsyncActionFilter
{
    public string OperationId { get; set; }
    public string[] Tags { get; set; } = [];

    public AuditAttribute(string operationId) => OperationId = operationId;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!string.IsNullOrEmpty(OperationId))
            context.HttpContext.Request.Headers["X-Operation-Id"] = OperationId;

        context.HttpContext.Items["AuditExtra"] = new Dictionary<string, object?>
        {
            { "controller", context.Controller?.GetType().Name },
            { "action", context.ActionDescriptor.DisplayName },
            { "auditTags", Tags },
        };

        await next();
    }
}
