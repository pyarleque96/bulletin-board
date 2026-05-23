using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Commands.Listings;

/// <summary>
/// Updates the editable fields of a listing. Regla #5: calling Listing.Edit() puts
/// the listing back to Pending status automatically.
/// Returns the updated ListingDetailDto.
/// </summary>
public record UpdateOwnListingCommand(
    Guid ListingId,
    string TitleEn,
    string TitleEs,
    string? DescriptionEn,
    string? DescriptionEs,
    decimal? Price,
    string? Location,
    string? WhatsAppNumber) : IRequest<ListingDetailDto>;
