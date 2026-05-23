using MediatR;

namespace Bulletin.Board.Application.Commands.Providers;

public record SetProviderHierarchyCommand(Guid ProviderId, int Hierarchy) : IRequest;
