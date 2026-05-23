using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Commands.Auth;

public record RegisterUserCommand(
    string Email,
    string Password,
    string Role,
    string? FirstName = null,
    string? LastName = null) : IRequest<UserRegisteredDto>;
