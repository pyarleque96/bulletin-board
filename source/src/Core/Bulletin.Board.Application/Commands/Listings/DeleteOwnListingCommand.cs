using MediatR;

namespace Bulletin.Board.Application.Commands.Listings;

/// <summary>
/// Soft-deletes a listing owned by the current user.
/// Distinct from AdminDeleteListingCommand: this does not create an admin audit log.
/// </summary>
public record DeleteOwnListingCommand(Guid ListingId) : IRequest;
