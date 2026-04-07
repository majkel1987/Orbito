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

    public async Task<Result> SendTrialExpiringAsync(
        string toEmail,
        string providerName,
        int daysRemaining,
        string planName,
        string billingLink,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Sending trial expiring email to {Email} for provider {ProviderName}, {DaysRemaining} days remaining",
                toEmail,
                providerName,
                daysRemaining);

            var (subject, urgency) = daysRemaining switch
            {
                <= 1 => ("⚠️ OSTATNI DZIEŃ TRIALA - Opłać teraz!", "critical"),
                <= 3 => ($"Zostały Ci tylko {daysRemaining} dni triala!", "warning"),
                _ => ($"Twój okres próbny kończy się za {daysRemaining} dni", "info")
            };

            var body = BuildTrialExpiringEmailBody(providerName, daysRemaining, planName, billingLink, urgency);

            await _emailSender.SendEmailAsync(toEmail, subject, body, isHtml: true, cancellationToken);

            _logger.LogInformation(
                "Successfully sent trial expiring email to {Email}, urgency: {Urgency}",
                toEmail,
                urgency);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send trial expiring email to {Email} for provider {ProviderName}",
                toEmail,
                providerName);

            return Result.Failure("Email.SendFailed", "Failed to send trial expiring email");
        }
    }

    public async Task<Result> SendTrialExpiredAsync(
        string toEmail,
        string providerName,
        string planName,
        string billingLink,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Sending trial expired email to {Email} for provider {ProviderName}",
                toEmail,
                providerName);

            var subject = "Twój okres próbny dobiegł końca - Aktywuj pełną subskrypcję";
            var body = BuildTrialExpiredEmailBody(providerName, planName, billingLink);

            await _emailSender.SendEmailAsync(toEmail, subject, body, isHtml: true, cancellationToken);

            _logger.LogInformation(
                "Successfully sent trial expired email to {Email}",
                toEmail);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send trial expired email to {Email} for provider {ProviderName}",
                toEmail,
                providerName);

            return Result.Failure("Email.SendFailed", "Failed to send trial expired email");
        }
    }

    private static string BuildTrialExpiringEmailBody(
        string providerName,
        int daysRemaining,
        string planName,
        string billingLink,
        string urgency)
    {
        var (headerColor, headerBgColor, headerIcon, headerText) = urgency switch
        {
            "critical" => ("#991b1b", "#fef2f2", "⚠️", "Ostatni dzień triala!"),
            "warning" => ("#92400e", "#fffbeb", "⏰", $"Zostały {daysRemaining} dni triala!"),
            _ => ("#1e40af", "#eff6ff", "ℹ️", $"Okres próbny kończy się za {daysRemaining} dni")
        };

        var buttonColor = urgency == "critical" ? "#DC2626" : "#4F46E5";

        return $"""
            <!DOCTYPE html>
            <html lang="pl">
            <head>
                <meta charset="UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>Okres próbny wygasa</title>
            </head>
            <body style="font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;">
                <div style="max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; padding: 40px;">
                    <div style="background-color: {headerBgColor}; border-radius: 8px; padding: 16px; margin-bottom: 24px; text-align: center;">
                        <span style="font-size: 32px;">{headerIcon}</span>
                        <h2 style="color: {headerColor}; margin: 8px 0 0 0;">{headerText}</h2>
                    </div>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        Cześć,
                    </p>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        Twój okres próbny planu <strong>{planName}</strong> dla <strong>{providerName}</strong>
                        {(daysRemaining <= 1 ? "kończy się dzisiaj" : $"kończy się za {daysRemaining} dni")}.
                    </p>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        {(urgency == "critical"
                            ? "Opłać subskrypcję teraz, aby nie stracić dostępu do wszystkich funkcji platformy!"
                            : "Opłać subskrypcję, aby zachować pełny dostęp do wszystkich funkcji platformy.")}
                    </p>
                    <div style="text-align: center; margin: 32px 0;">
                        <a href="{billingLink}"
                           style="background-color: {buttonColor}; color: #ffffff; padding: 14px 28px;
                                  text-decoration: none; border-radius: 6px; font-size: 16px;
                                  display: inline-block; font-weight: bold;">
                            {(urgency == "critical" ? "Opłać teraz →" : "Przejdź do płatności")}
                        </a>
                    </div>
                    <div style="background-color: #f9fafb; border-radius: 8px; padding: 16px; margin: 24px 0;">
                        <p style="color: #6b7280; margin: 0; font-size: 14px;">
                            <strong>Plan:</strong> {planName}<br/>
                            <strong>Pozostało dni:</strong> {daysRemaining}
                        </p>
                    </div>
                    <hr style="border: none; border-top: 1px solid #eeeeee; margin: 24px 0;" />
                    <p style="color: #aaaaaa; font-size: 12px; text-align: center;">
                        © {DateTime.UtcNow.Year} Orbito. Wszelkie prawa zastrzeżone.
                    </p>
                </div>
            </body>
            </html>
            """;
    }

    public async Task<Result> SendTeamMemberInvitationAsync(
        string toEmail,
        string inviteeName,
        string providerName,
        string inviterName,
        string roleName,
        string invitationLink,
        string? personalMessage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Sending team member invitation email to {Email} for provider {ProviderName}",
                toEmail,
                providerName);

            var subject = $"Zaproszenie do zespołu {providerName}";
            var body = BuildTeamMemberInvitationEmailBody(
                inviteeName,
                providerName,
                inviterName,
                roleName,
                invitationLink,
                personalMessage);

            await _emailSender.SendEmailAsync(toEmail, subject, body, isHtml: true, cancellationToken);

            _logger.LogInformation(
                "Successfully sent team member invitation email to {Email}",
                toEmail);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send team member invitation email to {Email} for provider {ProviderName}",
                toEmail,
                providerName);

            return Result.Failure("Email.SendFailed", "Failed to send team member invitation email");
        }
    }

    private static string BuildTeamMemberInvitationEmailBody(
        string inviteeName,
        string providerName,
        string inviterName,
        string roleName,
        string invitationLink,
        string? personalMessage)
    {
        var personalMessageHtml = string.IsNullOrWhiteSpace(personalMessage)
            ? ""
            : $"""
                <div style="background-color: #f9fafb; border-left: 4px solid #4F46E5; padding: 16px; margin: 24px 0; border-radius: 0 8px 8px 0;">
                    <p style="color: #6b7280; margin: 0 0 8px 0; font-size: 12px;">Wiadomość od {inviterName}:</p>
                    <p style="color: #374151; margin: 0; font-style: italic;">"{personalMessage}"</p>
                </div>
            """;

        return $"""
            <!DOCTYPE html>
            <html lang="pl">
            <head>
                <meta charset="UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>Zaproszenie do zespołu</title>
            </head>
            <body style="font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;">
                <div style="max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; padding: 40px;">
                    <div style="text-align: center; margin-bottom: 24px;">
                        <span style="font-size: 48px;">👋</span>
                    </div>
                    <h2 style="color: #333333; text-align: center;">Zostałeś zaproszony do zespołu!</h2>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        Cześć{(string.IsNullOrWhiteSpace(inviteeName) || inviteeName.Contains('@') ? "" : $" {inviteeName}")},
                    </p>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        <strong>{inviterName}</strong> zaprasza Cię do dołączenia do zespołu <strong>{providerName}</strong> jako <strong>{roleName}</strong>.
                    </p>
                    {personalMessageHtml}
                    <div style="text-align: center; margin: 32px 0;">
                        <a href="{invitationLink}"
                           style="background-color: #4F46E5; color: #ffffff; padding: 14px 28px;
                                  text-decoration: none; border-radius: 6px; font-size: 16px;
                                  display: inline-block; font-weight: bold;">
                            Dołącz do zespołu
                        </a>
                    </div>
                    <div style="background-color: #f9fafb; border-radius: 8px; padding: 16px; margin: 24px 0;">
                        <p style="color: #6b7280; margin: 0; font-size: 14px;">
                            <strong>Organizacja:</strong> {providerName}<br/>
                            <strong>Rola:</strong> {roleName}<br/>
                            <strong>Zaproszenie od:</strong> {inviterName}
                        </p>
                    </div>
                    <p style="color: #888888; font-size: 14px;">
                        Ten link zaproszenia wygaśnie za 7 dni. Jeśli nie spodziewałeś się tego zaproszenia, możesz zignorować tę wiadomość.
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

    private static string BuildTrialExpiredEmailBody(
        string providerName,
        string planName,
        string billingLink)
    {
        return $"""
            <!DOCTYPE html>
            <html lang="pl">
            <head>
                <meta charset="UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>Okres próbny wygasł</title>
            </head>
            <body style="font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px;">
                <div style="max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; padding: 40px;">
                    <div style="background-color: #fef2f2; border-radius: 8px; padding: 16px; margin-bottom: 24px; text-align: center;">
                        <span style="font-size: 32px;">🔒</span>
                        <h2 style="color: #991b1b; margin: 8px 0 0 0;">Okres próbny dobiegł końca</h2>
                    </div>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        Cześć,
                    </p>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        Twój 14-dniowy okres próbny planu <strong>{planName}</strong> dla <strong>{providerName}</strong> właśnie się zakończył.
                    </p>
                    <p style="color: #555555; font-size: 16px; line-height: 1.6;">
                        Twój dostęp do platformy jest teraz ograniczony. Aby odblokować wszystkie funkcje, opłać subskrypcję.
                    </p>
                    <div style="text-align: center; margin: 32px 0;">
                        <a href="{billingLink}"
                           style="background-color: #4F46E5; color: #ffffff; padding: 14px 28px;
                                  text-decoration: none; border-radius: 6px; font-size: 16px;
                                  display: inline-block; font-weight: bold;">
                            Aktywuj subskrypcję
                        </a>
                    </div>
                    <div style="background-color: #f9fafb; border-radius: 8px; padding: 16px; margin: 24px 0;">
                        <p style="color: #6b7280; margin: 0; font-size: 14px;">
                            <strong>Plan:</strong> {planName}<br/>
                            <strong>Status:</strong> Wygasły
                        </p>
                    </div>
                    <p style="color: #888888; font-size: 14px;">
                        Masz pytania? Skontaktuj się z naszym zespołem wsparcia.
                    </p>
                    <hr style="border: none; border-top: 1px solid #eeeeee; margin: 24px 0;" />
                    <p style="color: #aaaaaa; font-size: 12px; text-align: center;">
                        © {DateTime.UtcNow.Year} Orbito. Wszelkie prawa zastrzeżone.
                    </p>
                </div>
            </body>
            </html>
            """;
    }
}
