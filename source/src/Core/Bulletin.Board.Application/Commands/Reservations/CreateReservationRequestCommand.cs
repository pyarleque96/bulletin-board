using MediatR;

namespace Bulletin.Board.Application.Commands.Reservations;

public record CreateReservationRequestCommand(
    Guid VehicleId,
    DateOnly StartDate,
    DateOnly EndDate,
    string RequesterName,
    string RequesterEmail,
    string? RequesterPhone,
    string? Comment
) : IRequest<Guid>;
