using System.Threading.Channels;
using RbacApi.Data.Entities;

namespace RbacApi.Infrastructure.Interfaces;

public interface IAuditQueue
{
    ChannelReader<AuditLog> Reader { get; }
    ValueTask EnqueueAsync(AuditLog log);
}
