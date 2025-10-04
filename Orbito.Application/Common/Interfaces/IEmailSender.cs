namespace Orbito.Application.Common.Interfaces;

/// <summary>
/// Service interface for sending emails
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (plain text or HTML)</param>
    /// <param name="isHtml">Whether the body is HTML</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email to multiple recipients
    /// </summary>
    /// <param name="to">List of recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (plain text or HTML)</param>
    /// <param name="isHtml">Whether the body is HTML</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendEmailAsync(
        IEnumerable<string> to,
        string subject,
        string body,
        bool isHtml = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email with advanced options
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (plain text or HTML)</param>
    /// <param name="from">Sender email address (optional)</param>
    /// <param name="cc">CC recipients (optional)</param>
    /// <param name="bcc">BCC recipients (optional)</param>
    /// <param name="attachments">Email attachments (optional)</param>
    /// <param name="isHtml">Whether the body is HTML</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendEmailAsync(
        string to,
        string subject,
        string body,
        string? from = null,
        IEnumerable<string>? cc = null,
        IEnumerable<string>? bcc = null,
        IEnumerable<EmailAttachment>? attachments = null,
        bool isHtml = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email to multiple recipients with advanced options
    /// </summary>
    /// <param name="to">List of recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (plain text or HTML)</param>
    /// <param name="from">Sender email address (optional)</param>
    /// <param name="cc">CC recipients (optional)</param>
    /// <param name="bcc">BCC recipients (optional)</param>
    /// <param name="attachments">Email attachments (optional)</param>
    /// <param name="isHtml">Whether the body is HTML</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendEmailAsync(
        IEnumerable<string> to,
        string subject,
        string body,
        string? from = null,
        IEnumerable<string>? cc = null,
        IEnumerable<string>? bcc = null,
        IEnumerable<EmailAttachment>? attachments = null,
        bool isHtml = false,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an email attachment
/// </summary>
public class EmailAttachment
{
    /// <summary>
    /// File name with extension
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File content as byte array
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// MIME type of the file (e.g., "application/pdf", "image/png")
    /// </summary>
    public string ContentType { get; set; } = "application/octet-stream";
}