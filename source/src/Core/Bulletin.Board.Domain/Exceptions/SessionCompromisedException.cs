namespace Bulletin.Board.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se detecta reuse de un refresh token ya revocado,
/// lo que indica que la familia de sesión está comprometida.
/// Toda la familia debe ser revocada.
/// </summary>
public sealed class SessionCompromisedException(Guid familyId)
    : DomainException($"Session family {familyId} is compromised: refresh token reuse detected. All sessions in the family have been revoked.")
{
    public Guid FamilyId { get; } = familyId;
    public string ErrorCode => "SESSION_COMPROMISED";
}
