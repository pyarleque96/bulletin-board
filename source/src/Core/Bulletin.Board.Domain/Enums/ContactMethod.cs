namespace Bulletin.Board.Domain.Enums;

/// <summary>
/// Canal por el cual el proveedor contactó al cliente. Auditado por regla #8 —
/// el GM siempre recibe email con la interacción.
/// </summary>
public enum ContactMethod
{
    WhatsApp,
    Phone,
    Email,
    Other
}
