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

    public async Task<Result> SendPaymentConfirmationAsync(
        string toEmail,
        string clientName,
        string subscriptionName,
        decimal amount,
        string currency,
        string paymentId,
        DateTime paymentDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Sending payment confirmation email to {Email} for payment {PaymentId}",
                toEmail,
                paymentId);

            var subject = $"Potwierdzenie płatności - {subscriptionName}";
            var body = BuildPaymentConfirmationEmailBody(clientName, subscriptionName, amount, currency, paymentId, paymentDate);

            await _emailSender.SendEmailAsync(toEmail, subject, body, isHtml: true, cancellationToken);

            _logger.LogInformation(
                "Successfully sent payment confirmation email to {Email} for payment {PaymentId}",
                toEmail,
                paymentId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send payment confirmation email to {Email} for payment {PaymentId}",
                toEmail,
                paymentId);

            return Result.Failure("Email.SendFailed", "Failed to send payment confirmation email");
        }
    }

    public async Task<Result> SendPaymentFailedAsync(
        string toEmail,
        string clientName,
        string subscriptionName,
        decimal amount,
        string currency,
        string failureReason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Sending payment failed email to {Email} for subscription {SubscriptionName}",
                toEmail,
                subscriptionName);

            var subject = $"Płatność nieudana - {subscriptionName}";
            var body = BuildPaymentFailedEmailBody(clientName, subscriptionName, amount, currency, failureReason);

            await _emailSender.SendEmailAsync(toEmail, subject, body, isHtml: true, cancellationToken);

            _logger.LogInformation(
                "Successfully sent payment failed email to {Email}",
                toEmail);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send payment failed email to {Email}",
                toEmail);

            return Result.Failure("Email.SendFailed", "Failed to send payment failed email");
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

    private static string BuildPaymentConfirmationEmailBody(
        string clientName,
        string subscriptionName,
        decimal amount,
        string currency,
        string paymentId,
        DateTime paymentDate)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="pl">
            <head>
                <meta charset="UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>Potwierdzenie płatności</title>
            </head>
            <body style="font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;">
                <div style="max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; padding: 40px;">
                    <div style="text-align: center; margin-bottom: 24px;">
                        <div style="background-color: #10B981; width: 64px; height: 64px; border-radius: 50%; margin: 0 auto; display: flex; align-items: center; justify-content: center;">
                            <span style="color: white; font-size: 32px;">✓</span>
                        </div>
                    </div>
                    <h2 style="color: #333333; text-align: center;">Płatność zakończona sukcesem!</h2>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        Cześć {clientName},
                    </p>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        Twoja płatność za subskrypcję <strong>{subscriptionName}</strong> została pomyślnie przetworzona.
                    </p>
                    <div style="background-color: #f9fafb; border-radius: 8px; padding: 24px; margin: 24px 0;">
                        <table style="width: 100%; border-collapse: collapse;">
                            <tr>
                                <td style="color: #6b7280; padding: 8px 0;">Kwota:</td>
                                <td style="color: #111827; font-weight: bold; text-align: right;">{amount:N2} {currency}</td>
                            </tr>
                            <tr>
                                <td style="color: #6b7280; padding: 8px 0;">Data płatności:</td>
                                <td style="color: #111827; text-align: right;">{paymentDate:dd.MM.yyyy HH:mm}</td>
                            </tr>
                            <tr>
                                <td style="color: #6b7280; padding: 8px 0;">Numer transakcji:</td>
                                <td style="color: #111827; text-align: right; font-family: monospace; font-size: 12px;">{paymentId}</td>
                            </tr>
                        </table>
                    </div>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        Dziękujemy za dokonanie płatności!
                    </p>
                    <hr style="border: none; border-top: 1px solid #eeeeee; margin: 24px 0;" />
                    <p style="color: #aaaaaa; font-size: 12px; text-align: center;">
                        Ten email został wygenerowany automatycznie. Nie odpowiadaj na tę wiadomość.
                    </p>
                </div>
            </body>
            </html>
            """;
    }

    private static string BuildPaymentFailedEmailBody(
        string clientName,
        string subscriptionName,
        decimal amount,
        string currency,
        string failureReason)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="pl">
            <head>
                <meta charset="UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>Płatność nieudana</title>
            </head>
            <body style="font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;">
                <div style="max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; padding: 40px;">
                    <div style="text-align: center; margin-bottom: 24px;">
                        <div style="background-color: #EF4444; width: 64px; height: 64px; border-radius: 50%; margin: 0 auto; display: flex; align-items: center; justify-content: center;">
                            <span style="color: white; font-size: 32px;">✕</span>
                        </div>
                    </div>
                    <h2 style="color: #333333; text-align: center;">Płatność nieudana</h2>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        Cześć {clientName},
                    </p>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        Niestety, Twoja płatność za subskrypcję <strong>{subscriptionName}</strong> nie została przetworzona.
                    </p>
                    <div style="background-color: #fef2f2; border: 1px solid #fecaca; border-radius: 8px; padding: 16px; margin: 24px 0;">
                        <p style="color: #991b1b; margin: 0; font-size: 14px;">
                            <strong>Powód:</strong> {failureReason}
                        </p>
                    </div>
                    <div style="background-color: #f9fafb; border-radius: 8px; padding: 24px; margin: 24px 0;">
                        <table style="width: 100%; border-collapse: collapse;">
                            <tr>
                                <td style="color: #6b7280; padding: 8px 0;">Kwota:</td>
                                <td style="color: #111827; font-weight: bold; text-align: right;">{amount:N2} {currency}</td>
                            </tr>
                            <tr>
                                <td style="color: #6b7280; padding: 8px 0;">Subskrypcja:</td>
                                <td style="color: #111827; text-align: right;">{subscriptionName}</td>
                            </tr>
                        </table>
                    </div>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        Prosimy o sprawdzenie danych płatności i ponowienie próby. Jeśli problem będzie się powtarzał, skontaktuj się z obsługą klienta.
                    </p>
                    <hr style="border: none; border-top: 1px solid #eeeeee; margin: 24px 0;" />
                    <p style="color: #aaaaaa; font-size: 12px; text-align: center;">
                        Ten email został wygenerowany automatycznie. Nie odpowiadaj na tę wiadomość.
                    </p>
                </div>
            </body>
            </html>
            """;
    }
}
