using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Commands.Contacts;

public record ContactProviderCommand(Guid ListingId, bool WaiverAccepted) : IRequest<ContactResultDto>;
