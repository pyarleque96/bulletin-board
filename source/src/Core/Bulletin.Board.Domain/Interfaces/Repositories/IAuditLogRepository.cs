using Bulletin.Board.Domain.Entities;

namespace Bulletin.Board.Domain.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task AppendAsync(AuditLog log, CancellationToken ct = default);
}
