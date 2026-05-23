using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace Bulletin.Board.Application.Queries.Admin.Audit;

public record GetVipChangeLogsQuery(
    string EntityType,
    Guid EntityId,
    int Page,
    int PageSize) : IRequest<IReadOnlyList<VipChangeLogDto>>;

public sealed class GetVipChangeLogsQueryHandler(
    IRepository<VipChangeLog> logRepository)
    : IRequestHandler<GetVipChangeLogsQuery, IReadOnlyList<VipChangeLogDto>>
{
    public async Task<IReadOnlyList<VipChangeLogDto>> Handle(GetVipChangeLogsQuery request, CancellationToken ct)
    {
        if (!Enum.TryParse<VipEntityType>(request.EntityType, ignoreCase: true, out var et))
            throw new InvalidOperationException($"Invalid entityType '{request.EntityType}'.");

        var rows = await logRepository.FindAsync(
            l => l.EntityType == et && l.EntityId == request.EntityId, ct);

        return rows
            .OrderByDescending(l => l.ChangedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new VipChangeLogDto(
                l.Id,
                l.EntityType.ToString(),
                l.EntityId,
                l.Action.ToString(),
                l.PreviousVipUntil,
                l.PreviousIsIndefinite,
                l.NewVipUntil,
                l.NewIsIndefinite,
                l.ChangedByUserId,
                l.ChangedAt,
                l.IpAddress,
                l.Reason))
            .ToList();
    }
}

public sealed class GetVipChangeLogsQueryValidator : AbstractValidator<GetVipChangeLogsQuery>
{
    public GetVipChangeLogsQueryValidator()
    {
        RuleFor(x => x.EntityType).NotEmpty();
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
