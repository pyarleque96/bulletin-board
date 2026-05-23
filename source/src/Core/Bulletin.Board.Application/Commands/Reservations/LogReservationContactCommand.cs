using MediatR;

namespace Bulletin.Board.Application.Commands.Reservations;

/// <summary>
/// Registers a contact interaction linked to a reservation (e.g. a WhatsApp click from the
/// reservation detail view). Regla #8: the GM always receives an email notification.
/// </summary>
public record LogReservationContactCommand(Guid ReservationId, string Channel = "WhatsApp") : IRequest;
