using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Admin.Audit;

public record GetAuditLogsQuery(
    string? Action,
    string? ResourceType,
    DateTimeOffset? From,
    DateTimeOffset? To,
    int Page,
    int PageSize) : IRequest<PagedResult<AuditLogDto>>;

public sealed class GetAuditLogsQueryHandler(
    IRepository<AuditLog> auditLogRepository,
    ILogger<GetAuditLogsQueryHandler> logger)
    : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    public async Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken ct)
    {
        logger.LogInformation("Admin fetching audit logs action={Action} resource={Resource}",
            request.Action, request.ResourceType);

        var all = await auditLogRepository.GetAllAsync(ct);
        var filtered = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Action))
            filtered = filtered.Where(l => l.Action.Contains(request.Action, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(request.ResourceType))
            filtered = filtered.Where(l => l.ResourceType.Equals(request.ResourceType, StringComparison.OrdinalIgnoreCase));

        if (request.From.HasValue)
            filtered = filtered.Where(l => l.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            filtered = filtered.Where(l => l.CreatedAt <= request.To.Value);

        var sorted = filtered.OrderByDescending(l => l.CreatedAt).ToList();
        var total = sorted.Count;

        var paged = sorted
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new AuditLogDto(
                Id: l.Id,
                Action: l.Action,
                ResourceType: l.ResourceType,
                ResourceId: l.ResourceId,
                ActorUserId: l.ActorUserId,
                CreatedAt: l.CreatedAt))
            .ToList();

        return new PagedResult<AuditLogDto>(paged, total, request.Page, request.PageSize);
    }
}

public sealed class GetAuditLogsQueryValidator : AbstractValidator<GetAuditLogsQuery>
{
    public GetAuditLogsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
