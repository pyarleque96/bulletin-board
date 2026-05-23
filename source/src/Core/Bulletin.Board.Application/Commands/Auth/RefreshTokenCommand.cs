using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Commands.Auth;

public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResultDto>;
