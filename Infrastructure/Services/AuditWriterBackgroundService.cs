using System.Diagnostics;
using MongoDB.Driver;
using RbacApi.Data;
using RbacApi.Data.Entities;
using RbacApi.Infrastructure.Interfaces;

namespace RbacApi.Infrastructure.Services;

public class AuditWriterBackgroundService : BackgroundService
{
    private readonly IAuditQueue _queue;
    private IMongoCollection<AuditLog> _collection;
    private readonly ILogger<AuditWriterBackgroundService> _logger;
    private readonly int _batchSize = 50;
    private readonly TimeSpan _flushInterval = TimeSpan.FromSeconds(2);


    public AuditWriterBackgroundService(IAuditQueue queue, CollectionsProvider provider, ILogger<AuditWriterBackgroundService> logger)
    {
        _queue = queue;
        _collection = provider.AuditLogs;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var buffer = new List<AuditLog>(_batchSize);

        var reader = _queue.Reader;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var hasItem = await reader.WaitToReadAsync(stoppingToken);

                if (!hasItem) break;

                var sw = Stopwatch.StartNew();

                while (sw.Elapsed < _flushInterval && buffer.Count < _batchSize && reader.TryRead(out var item))
                {
                    buffer.Add(item);
                    _logger.LogInformation($"Added to buffer {item.CorrelationId}");
                }

                if (buffer.Count > 0)
                {
                    await _collection.InsertManyAsync(buffer, cancellationToken: stoppingToken);
                    _logger.LogInformation($"--> Send {buffer.Count} to database");
                    buffer.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing audit logs, Retrying...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
