using MediatR;

namespace Bulletin.Board.Application.Commands.Reservations;

public record RejectReservationRequestCommand(Guid ReservationId, string? Note) : IRequest;
