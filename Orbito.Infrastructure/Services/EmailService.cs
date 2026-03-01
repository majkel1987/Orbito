using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;

namespace Orbito.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IEmailSender emailSender, ILogger<EmailService> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<Result> SendClientInvitationAsync(
        string toEmail,
        string clientName,
        string providerName,
        string invitationLink,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Sending client invitation email to {Email} for provider {ProviderName}",
                toEmail,
                providerName);

            var subject = $"Zaproszenie do portalu {providerName}";
            var body = BuildInvitationEmailBody(clientName, providerName, invitationLink);

            await _emailSender.SendEmailAsync(toEmail, subject, body, isHtml: true, cancellationToken);

            _logger.LogInformation(
                "Successfully sent invitation email to {Email}",
                toEmail);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send invitation email to {Email} for provider {ProviderName}",
                toEmail,
                providerName);

            return Result.Failure("Email.SendFailed", "Failed to send invitation email");
        }
    }

    private static string BuildInvitationEmailBody(string clientName, string providerName, string invitationLink)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="pl">
            <head>
                <meta charset="UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>Zaproszenie</title>
            </head>
            <body style="font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;">
                <div style="max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; padding: 40px;">
                    <h2 style="color: #333333;">Witaj, {clientName}!</h2>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        Zostałeś zaproszony do portalu klienta <strong>{providerName}</strong>.
                    </p>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        Kliknij poniższy przycisk, aby aktywować swoje konto:
                    </p>
                    <div style="text-align: center; margin: 32px 0;">
                        <a href="{invitationLink}"
                           style="background-color: #4F46E5; color: #ffffff; padding: 14px 28px;
                                  text-decoration: none; border-radius: 6px; font-size: 16px;
                                  display: inline-block;">
                            Aktywuj konto w {providerName}
                        </a>
                    </div>
                    <p style="color: #888888; font-size: 14px;">
                        Link jest ważny przez 7 dni. Jeśli nie spodziewałeś się tego zaproszenia, możesz zignorować tę wiadomość.
                    </p>
                    <hr style="border: none; border-top: 1px solid #eeeeee; margin: 24px 0;" />
                    <p style="color: #aaaaaa; font-size: 12px; text-align: center;">
                        © {DateTime.UtcNow.Year} {providerName}. Wszelkie prawa zastrzeżone.
                    </p>
                </div>
            </body>
            </html>
            """;
    }
}
