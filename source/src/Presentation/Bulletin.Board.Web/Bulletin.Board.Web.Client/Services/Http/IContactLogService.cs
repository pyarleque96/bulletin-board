namespace Bulletin.Board.Web.Client.Services.Http;

/// <summary>
/// Fire-and-forget contact log service.
/// Per Regla #8: click on WhatsApp button → POST /contact-log with 2s timeout.
/// NEVER blocks the WhatsApp deep-link from opening. NEVER throws.
/// </summary>
public interface IContactLogService
{
    /// <summary>
    /// Logs a contact interaction. Fire-and-forget: returns immediately.
    /// Failures are logged to the console but never surface to the caller.
    /// </summary>
    /// <param name="reservationId">The reservation associated with this contact.</param>
    /// <param name="channel">Contact channel; defaults to "WhatsApp".</param>
    void LogContact(Guid reservationId, string channel = "WhatsApp");
}
