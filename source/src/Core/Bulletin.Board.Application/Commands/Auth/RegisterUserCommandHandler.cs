using Bulletin.Board.Application.DTOs;
using Bulletin.Board.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bulletin.Board.Application.Commands.Auth;

public sealed class RegisterUserCommandHandler(
    IUserService userService,
    ILogger<RegisterUserCommandHandler> logger)
    : IRequestHandler<RegisterUserCommand, UserRegisteredDto>
{
    public async Task<UserRegisteredDto> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        logger.LogInformation("Registering new user with role {Role}", request.Role);

        var userId = await userService.CreateUserAsync(
            request.Email, request.Password, request.Role,
            request.FirstName, request.LastName, ct);
        return new UserRegisteredDto(userId);
    }
}
