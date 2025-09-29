namespace Orbito.Application.Common.Models.PaymentGateway
{
    /// <summary>
    /// Result of webhook validation with detailed information
    /// </summary>
    public record WebhookValidationResult
    {
        /// <summary>
        /// Whether the webhook is valid
        /// </summary>
        public required bool IsValid { get; init; }

        /// <summary>
        /// Error reason if validation failed
        /// </summary>
        public string? ErrorReason { get; init; }

        /// <summary>
        /// Parsed webhook data if valid
        /// </summary>
        public object? ParsedData { get; init; }

        /// <summary>
        /// Webhook event type
        /// </summary>
        public string? EventType { get; init; }

        /// <summary>
        /// Webhook timestamp
        /// </summary>
        public DateTime? Timestamp { get; init; }

        /// <summary>
        /// Additional metadata from webhook
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();

        /// <summary>
        /// Create successful validation result
        /// </summary>
        /// <param name="parsedData">Parsed webhook data</param>
        /// <param name="eventType">Event type</param>
        /// <param name="timestamp">Webhook timestamp</param>
        /// <param name="metadata">Additional metadata</param>
        /// <returns>Successful validation result</returns>
        public static WebhookValidationResult Success(
            object parsedData,
            string? eventType = null,
            DateTime? timestamp = null,
            Dictionary<string, string>? metadata = null)
        {
            return new WebhookValidationResult
            {
                IsValid = true,
                ParsedData = parsedData,
                EventType = eventType,
                Timestamp = timestamp ?? DateTime.UtcNow,
                Metadata = metadata ?? new Dictionary<string, string>()
            };
        }

        /// <summary>
        /// Create failed validation result
        /// </summary>
        /// <param name="errorReason">Error reason</param>
        /// <param name="metadata">Additional metadata</param>
        /// <returns>Failed validation result</returns>
        public static WebhookValidationResult Failure(
            string errorReason,
            Dictionary<string, string>? metadata = null)
        {
            return new WebhookValidationResult
            {
                IsValid = false,
                ErrorReason = errorReason,
                Timestamp = DateTime.UtcNow,
                Metadata = metadata ?? new Dictionary<string, string>()
            };
        }
    }
}
