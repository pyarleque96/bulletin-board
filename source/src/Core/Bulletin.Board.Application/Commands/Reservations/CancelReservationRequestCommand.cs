using MediatR;

namespace Bulletin.Board.Application.Commands.Reservations;

public record CancelReservationRequestCommand(Guid ReservationId, string? Note) : IRequest;
