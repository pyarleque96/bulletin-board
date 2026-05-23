using Bulletin.Board.Application.DTOs;
using MediatR;

namespace Bulletin.Board.Application.Commands.Auth;

/// <summary>
/// Autentica al usuario con email y password.
/// DeviceInfo (User-Agent truncado) se captura en el controlador y se almacena
/// en el refresh token para auditoría de sesiones.
/// </summary>
public record LoginCommand(string Email, string Password, string? DeviceInfo = null) : IRequest<LoginResultDto>;
