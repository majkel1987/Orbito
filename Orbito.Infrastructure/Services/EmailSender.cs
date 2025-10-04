using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;

namespace Orbito.Infrastructure.Services
{
    /// <summary>
    /// Simple email sender implementation for development/testing
    /// In production, this should be replaced with a real email service (SendGrid, SMTP, etc.)
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isHtml = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending email to {To} with subject '{Subject}'", to, subject);
            _logger.LogDebug("Email body: {Body}", body);
            
            // In development, just log the email
            // In production, this would send the actual email
            await Task.Delay(100, cancellationToken); // Simulate email sending
        }

        public async Task SendEmailAsync(
            IEnumerable<string> to,
            string subject,
            string body,
            bool isHtml = false,
            CancellationToken cancellationToken = default)
        {
            var recipients = to.ToList();
            _logger.LogInformation("Sending email to {Count} recipients with subject '{Subject}'", 
                recipients.Count, subject);
            _logger.LogDebug("Recipients: {Recipients}", string.Join(", ", recipients));
            _logger.LogDebug("Email body: {Body}", body);
            
            // In development, just log the email
            // In production, this would send the actual email
            await Task.Delay(100, cancellationToken); // Simulate email sending
        }

        public async Task SendEmailAsync(
            string to,
            string subject,
            string body,
            string? from = null,
            IEnumerable<string>? cc = null,
            IEnumerable<string>? bcc = null,
            IEnumerable<EmailAttachment>? attachments = null,
            bool isHtml = false,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending email to {To} with subject '{Subject}' from {From}", 
                to, subject, from ?? "default");
            
            if (cc?.Any() == true)
            {
                _logger.LogDebug("CC: {CC}", string.Join(", ", cc));
            }
            
            if (bcc?.Any() == true)
            {
                _logger.LogDebug("BCC: {BCC}", string.Join(", ", bcc));
            }
            
            if (attachments?.Any() == true)
            {
                _logger.LogDebug("Attachments: {Attachments}", 
                    string.Join(", ", attachments.Select(a => a.FileName)));
            }
            
            _logger.LogDebug("Email body: {Body}", body);
            
            // In development, just log the email
            // In production, this would send the actual email
            await Task.Delay(100, cancellationToken); // Simulate email sending
        }

        public async Task SendEmailAsync(
            IEnumerable<string> to,
            string subject,
            string body,
            string? from = null,
            IEnumerable<string>? cc = null,
            IEnumerable<string>? bcc = null,
            IEnumerable<EmailAttachment>? attachments = null,
            bool isHtml = false,
            CancellationToken cancellationToken = default)
        {
            var recipients = to.ToList();
            _logger.LogInformation("Sending email to {Count} recipients with subject '{Subject}' from {From}", 
                recipients.Count, subject, from ?? "default");
            
            _logger.LogDebug("Recipients: {Recipients}", string.Join(", ", recipients));
            
            if (cc?.Any() == true)
            {
                _logger.LogDebug("CC: {CC}", string.Join(", ", cc));
            }
            
            if (bcc?.Any() == true)
            {
                _logger.LogDebug("BCC: {BCC}", string.Join(", ", bcc));
            }
            
            if (attachments?.Any() == true)
            {
                _logger.LogDebug("Attachments: {Attachments}", 
                    string.Join(", ", attachments.Select(a => a.FileName)));
            }
            
            _logger.LogDebug("Email body: {Body}", body);
            
            // In development, just log the email
            // In production, this would send the actual email
            await Task.Delay(100, cancellationToken); // Simulate email sending
        }
    }
}
