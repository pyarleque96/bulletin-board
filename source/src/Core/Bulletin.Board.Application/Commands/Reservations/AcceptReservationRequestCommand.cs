using MediatR;

namespace Bulletin.Board.Application.Commands.Reservations;

public record AcceptReservationRequestCommand(Guid ReservationId, string? Note) : IRequest;
