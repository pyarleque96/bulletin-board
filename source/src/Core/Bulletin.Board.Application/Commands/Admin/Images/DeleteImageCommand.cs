using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Interfaces.Repositories;
using Bulletin.Board.Domain.Interfaces.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Admin.Images;

public record DeleteImageCommand(Guid ListingId, Guid ImageId) : IRequest;

public sealed class DeleteImageCommandHandler(
    IRepository<ListingImage> imageRepository,
    ICurrentUserService currentUserService,
    ILogger<DeleteImageCommandHandler> logger)
    : IRequestHandler<DeleteImageCommand>
{
    public async Task Handle(DeleteImageCommand request, CancellationToken ct)
    {
        var images = await imageRepository.FindAsync(
            i => i.Id == request.ImageId && i.ListingId == request.ListingId, ct);

        var image = images.FirstOrDefault()
            ?? throw new InvalidOperationException($"Image '{request.ImageId}' not found for listing '{request.ListingId}'.");

        var adminId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        imageRepository.Remove(image);
        await imageRepository.SaveChangesAsync(ct);

        logger.LogInformation("Image {ImageId} deleted by {AdminId}", request.ImageId, adminId);
    }
}

public sealed class DeleteImageCommandValidator : AbstractValidator<DeleteImageCommand>
{
    public DeleteImageCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.ImageId).NotEmpty();
    }
}
