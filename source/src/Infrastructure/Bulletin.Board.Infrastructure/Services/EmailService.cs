using Bulletin.Board.Domain.Interfaces.Services;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Bulletin.Board.Infrastructure.Services;

public sealed class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(configuration["Email:From"] ?? "noreply@bulletindells.com"));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(
                configuration["Email:SmtpHost"],
                int.Parse(configuration["Email:SmtpPort"] ?? "587"),
                MailKit.Security.SecureSocketOptions.StartTls, ct);

            await client.AuthenticateAsync(
                configuration["Email:Username"],
                configuration["Email:Password"], ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
}
