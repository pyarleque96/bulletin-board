using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Queries.Reservations;

public record GetMyReservationsQuery() : IRequest<IReadOnlyList<MyReservationDto>>;
