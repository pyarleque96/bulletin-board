using Bulletin.Board.Application.Common.Models;
using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Exceptions;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;

namespace Bulletin.Board.Application.Queries.Listings;

public sealed class GetManageListingsCursorQueryHandler(
    IListingRepository listings,
    ICurrentUserService currentUser)
    : IRequestHandler<GetManageListingsCursorQuery, CursorPagedResultDto<MyListingDto>>
{
    private const int MaxPageSize = 50;
    private const int DefaultPageSize = 20;

    public async Task<CursorPagedResultDto<MyListingDto>> Handle(
        GetManageListingsCursorQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated.");

        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? DefaultPageSize : request.PageSize,
            1,
            MaxPageSize);

        ManageListingsCursorState? cursorState = null;
        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            if (!ManageListingsCursor.TryDecode(request.Cursor, out var decoded) || decoded is null)
                throw new DomainException("Api_Error_InvalidCursor");

            cursorState = new ManageListingsCursorState(
                decoded.Hierarchy,
                decoded.ProviderTier,
                decoded.AvgRating,
                decoded.CreatedAt,
                decoded.Id);
        }

        // Solo computar total en la PRIMERA llamada (cursor null) — en páginas siguientes el
        // cliente ya tiene el conteo. Caller pasa IncludeTotal=true en la primera request.
        var includeTotal = request.IncludeTotal && cursorState is null;

        var page = await listings.GetManagePageAsync(
            userId, cursorState, pageSize,
            request.Status, request.CategorySlug, request.Search,
            includeTotal, ct);

        var items = page.Items
            .Select(l => new MyListingDto(
                Id: l.Id,
                Slug: l.Slug,
                TitleEn: l.TitleEn,
                TitleEs: l.TitleEs,
                CategorySlug: l.Category?.Slug ?? string.Empty,
                Status: l.Status.ToString(),
                Tier: l.ProviderTier.ToString(),
                HasPublishedSnapshot: !string.IsNullOrEmpty(l.PublishedSnapshot),
                IsPausedByOwner: l.IsPausedByOwner,
                ApprovedAt: l.PublishedAt,
                UpdatedAt: l.UpdatedAt,
                DescriptionEn: l.DescriptionEn,
                DescriptionEs: l.DescriptionEs,
                Price: l.Price,
                Location: l.Location,
                WhatsAppNumber: l.WhatsAppNumber))
            .ToList();

        string? nextCursor = null;
        if (page.HasMore && page.Items.Count > 0)
        {
            var last = page.Items[^1];
            nextCursor = new ManageListingsCursor(
                last.ProviderHierarchy,
                (int)last.ProviderTier,
                last.AvgRating ?? 0m,
                last.CreatedAt,
                last.Id).Encode();
        }

        return new CursorPagedResultDto<MyListingDto>(items, nextCursor, page.HasMore, page.Total);
    }
}
