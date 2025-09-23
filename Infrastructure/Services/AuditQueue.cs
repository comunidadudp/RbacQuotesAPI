using System.Threading.Channels;
using RbacApi.Data.Entities;
using RbacApi.Infrastructure.Interfaces;

namespace RbacApi.Infrastructure.Services;

public class AuditQueue : IAuditQueue, IDisposable
{
    private readonly Channel<AuditLog> _channel;
    private readonly ILogger<AuditQueue> _logger;

    public AuditQueue(ILogger<AuditQueue> logger,int capacity = 1000)
    {
        _logger = logger;
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        };
        _channel = Channel.CreateBounded<AuditLog>(options);
    }


    public async ValueTask EnqueueAsync(AuditLog log)
    {
        if (log == null)
            return;

        try
        {
            await _channel.Writer.WriteAsync(log);
            _logger.LogInformation($"--> Added to queue '{log.CorrelationId}'");
        }
        catch
        {
            // si el canal estáa cerrado, ignora el error o hacer log.
        }
    }

    public ChannelReader<AuditLog> Reader => _channel.Reader;

    public void Dispose() => _channel.Writer.TryComplete();
}
