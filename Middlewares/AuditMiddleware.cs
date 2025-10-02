using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Primitives;
using RbacApi.Data.Entities;
using RbacApi.Enums;
using RbacApi.Infrastructure.Interfaces;
using RbacApi.Infrastructure.Services;

namespace RbacApi.Middlewares;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuditQueue _queue;
    private readonly ILogger<AuditMiddleware> _logger;
    private const int MAX_BODY_SIZE = 1024 * 8;

    public AuditMiddleware(RequestDelegate next, IAuditQueue queue, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _queue = queue;
        _logger = logger;
    }


    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        var traceParent = context.Request.Headers["traceparent"].FirstOrDefault();
        var ip = context.Connection.RemoteIpAddress?.ToString();
        var ua = context.Request.Headers.UserAgent.ToString();

        string requestBody = string.Empty;
        if (context.Request.ContentLength > 0 && context.Request.Body.CanSeek == false)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var buffer = new char[Math.Min((int)context.Request.ContentLength, MAX_BODY_SIZE)];
            var read = await reader.ReadBlockAsync(buffer, 0, buffer.Length);
            requestBody = new string(buffer, 0, read);

            if (context.Request.ContentLength > MAX_BODY_SIZE)
                requestBody += "...(truncated)";

            context.Request.Body.Position = 0;
        }

        // Captura la respuesta para poder leer el cuerpo antes de enviarlo al cliente
        var originalBody = context.Response.Body;
        await using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        var sw = Stopwatch.StartNew();
        Exception? exCaught = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exCaught = ex;
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
        finally
        {
            sw.Stop();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string responseBody = string.Empty;

            if (context.Response.Body.Length > 0)
            {
                var toRead = (int)Math.Min(context.Response.Body.Length, MAX_BODY_SIZE);
                var buffer = new byte[toRead];
                await context.Response.Body.ReadAsync(buffer, 0, toRead);
                responseBody = Encoding.UTF8.GetString(buffer);

                if (context.Response.Body.Length > MAX_BODY_SIZE)
                    responseBody += "...(truncated)";
            }

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            var user = context.User?.Identity?.IsAuthenticated == true ? context.User.Identity?.Name : null;
            var actorId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var operation = context.Request.Headers["X-Operation-Id"].FirstOrDefault()
                ?? $"{context.Request.Method} {context.Request.Path}";

            var log = new AuditLog
            {
                CorrelationId = correlationId,
                TraceParent = traceParent,
                OperationId = operation,
                Actor = new AuditActor
                {
                    ActorId = actorId,
                    Username = user,
                    ActorType = actorId != null ? ActorType.User : ActorType.Service,
                },
                Action = ActionType.Custom,
                EntityId = null,
                EntityType = context.Request.Path,
                Outcome = exCaught == null ? OutcomeType.Success : OutcomeType.Failure,
                Severity = exCaught == null ? SeverityLevel.Info : SeverityLevel.Error,
                IpAddress = ip,
                UserAgent = ua,
                Tags = ["http", context.Request.Method.ToLower()],
                Details = new AuditDetail
                {
                    Message = exCaught == null ? "Request processed" : "Unhandled exception",
                    Error = exCaught?.ToString(),
                    Request = SanitizeRequest(context, requestBody),
                    Response = new Dictionary<string, object>
                    {
                        { "statusCode", context.Response.StatusCode },
                        { "body", SanitizeBody(responseBody) }
                    }
                }
            };

            if (context.Items.TryGetValue("AuditExtra", out var extraObj) && extraObj is Dictionary<string, object?> extraDict)
            {
                log.Metadata ??= [];
                foreach (var kv in extraDict)
                    log.Metadata[kv.Key] = kv.Value ?? "NULL";
            }

            if (context.Items.TryGetValue("AuditChanges", out var changeObj) && changeObj is Dictionary<string, object?> changeDict)
            {
                log.Changes ??= [];
                foreach (var kv in changeDict)
                    log.Changes.Add((AuditChange)kv.Value ?? new());
            }

            try
            {
                await _queue.EnqueueAsync(log);
            }
            catch (Exception qex)
            {
                _logger.LogError(qex, "Failed to enqueue audit log");
            }

            if (exCaught != null) throw exCaught;

        }
    }

    private string GetOrCreateCorrelationId(HttpContext ctx)
    {
        if (ctx.Request.Headers.TryGetValue("X-Correlation-ID", out var cid) && !StringValues.IsNullOrEmpty(cid))
            return cid.First() ?? "Unknown X-Correlation-ID";
        var newId = Guid.NewGuid().ToString("N");
        ctx.Response.Headers["X-Correlation-ID"] = newId;
        return newId;
    }

    // Sanitiza la información de la solicitud para evitar guardar datos sensibles.
    private static Dictionary<string, object>? SanitizeRequest(HttpContext ctx, string rawBody)
    {
        var res = new Dictionary<string, object?>();

        res["method"] = ctx.Request.Method;
        res["path"] = AuditSanitizer.ToSerializable(ctx.Request.Path);
        res["pathBase"] = AuditSanitizer.ToSerializable(ctx.Request.PathBase);
        res["queryString"] = AuditSanitizer.ToSerializable(ctx.Request.QueryString);
        res["query"] = AuditSanitizer.ToSerializable(ctx.Request.Query);
        res["headers"] = AuditSanitizer.ToSerializable(ctx.Request.Headers);
        res["contentType"] = ctx.Request.ContentType;

        if (!string.IsNullOrWhiteSpace(rawBody))
        {
            // Intenta parsear el cuerpo como JSON, si falla lo guarda como string truncado.
            try
            {
                var doc = JsonDocument.Parse(rawBody);
                res["body"] = SanitizeBody(rawBody);
            }
            catch
            {
                var sanitized = rawBody.Length > 2000 ? rawBody.Substring(0, 2000) + "...(truncated)" : rawBody;
                res["body"] = sanitized;
            }
        }

        // No guarda las cookies completas, solo las claves.
        res["cookies"] = ctx.Request.Cookies?.Keys?.ToList();

        return res!;
    }

    // Sanitiza el cuerpo de la solicitud/respuesta, redactando claves sensibles.
    private static object SanitizeBody(string body)
    {
        try
        {
            // Intenta parsear el cuerpo como JSON.
            var doc = JsonDocument.Parse(body);
            var dict = new Dictionary<string, object?>();

            foreach (var p in doc.RootElement.EnumerateObject())
            {
                var name = p.Name.ToLowerInvariant();

                // Si la propiedad es "data" y es un objeto, sanitiza sus propiedades internas.
                if (name == "data" && p.Value.ValueKind == JsonValueKind.Object)
                {
                    var dataDict = new Dictionary<string, object?>();
                    foreach (var dp in p.Value.EnumerateObject())
                    {
                        var dname = dp.Name.ToLowerInvariant();
                        if (dname.Contains("password") || dname.Contains("token") || dname.Contains("credit") || dname.Contains("cc"))
                            dataDict[dp.Name] = "***REDACTED***";
                        else
                            dataDict[dp.Name] = dp.Value.ToString();
                    }
                    dict[p.Name] = dataDict;
                }
                // Sanitiza propiedades sensibles en el nivel raíz.
                else if (name.Contains("password") || name.Contains("token") || name.Contains("credit") || name.Contains("cc"))
                {
                    dict[p.Name] = "***REDACTED***";
                }
                else
                {
                    dict[p.Name] = p.Value.ToString();
                }
            }
            return dict;
        }
        catch
        {
            // Si no es JSON, lo trunca si es muy largo.
            return body.Length > 200 ? body.Substring(0, 200) + "...(truncated)" : body;
        }
    }
}
