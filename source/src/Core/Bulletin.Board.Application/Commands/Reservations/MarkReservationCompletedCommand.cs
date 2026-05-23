using MediatR;

namespace Bulletin.Board.Application.Commands.Reservations;

/// <summary>
/// Marks a reservation as completed. Only valid from Accepted state.
/// Owner-only: only the listing owner can confirm service was rendered.
/// </summary>
public record MarkReservationCompletedCommand(Guid ReservationId) : IRequest;
