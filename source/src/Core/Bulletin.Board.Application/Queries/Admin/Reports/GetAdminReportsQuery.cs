using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Application.DTOs.Admin;
using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Queries.Admin.Reports;

public record GetAdminReportsQuery(string? Status, int Page, int PageSize) : IRequest<PagedResult<AdminReportDto>>;

public sealed class GetAdminReportsQueryHandler(
    IRepository<Report> reportRepository,
    ILogger<GetAdminReportsQueryHandler> logger)
    : IRequestHandler<GetAdminReportsQuery, PagedResult<AdminReportDto>>
{
    public async Task<PagedResult<AdminReportDto>> Handle(GetAdminReportsQuery request, CancellationToken ct)
    {
        logger.LogInformation("Admin fetching reports with status={Status}", request.Status);

        var all = await reportRepository.GetAllAsync(ct);
        var filtered = all.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<ReportStatus>(request.Status, ignoreCase: true, out var statusFilter))
            filtered = filtered.Where(r => r.Status == statusFilter);

        var sorted = filtered.OrderByDescending(r => r.CreatedAt).ToList();
        var total = sorted.Count;

        var paged = sorted
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new AdminReportDto(
                Id: r.Id,
                Reason: r.Reason,
                Status: r.Status.ToString(),
                ReportedById: r.ReportedById,
                ProviderId: r.ProviderId,
                ListingId: r.ListingId,
                CreatedAt: r.CreatedAt))
            .ToList();

        return new PagedResult<AdminReportDto>(paged, total, request.Page, request.PageSize);
    }
}

public sealed class GetAdminReportsQueryValidator : AbstractValidator<GetAdminReportsQuery>
{
    public GetAdminReportsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
