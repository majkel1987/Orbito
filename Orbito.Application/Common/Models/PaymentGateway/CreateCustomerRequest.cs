namespace Orbito.Application.Common.Models.PaymentGateway
{
    /// <summary>
    /// Request do tworzenia klienta w payment gateway
    /// </summary>
    public record CreateCustomerRequest
    {
        /// <summary>
        /// ID klienta w systemie
        /// </summary>
        public required Guid ClientId { get; init; }

        /// <summary>
        /// Email klienta
        /// </summary>
        public required string Email { get; init; }

        /// <summary>
        /// Imię klienta
        /// </summary>
        public string? FirstName { get; init; }

        /// <summary>
        /// Nazwisko klienta
        /// </summary>
        public string? LastName { get; init; }

        /// <summary>
        /// Nazwa firmy
        /// </summary>
        public string? CompanyName { get; init; }

        /// <summary>
        /// Telefon klienta
        /// </summary>
        public string? Phone { get; init; }

        /// <summary>
        /// Adres klienta
        /// </summary>
        public Address? Address { get; init; }

        /// <summary>
        /// Metadane klienta
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();

        /// <summary>
        /// ID tenanta dla multi-tenancy
        /// </summary>
        public required Guid TenantId { get; init; }

        /// <summary>
        /// Walidacja request'u
        /// </summary>
        public void Validate()
        {
            if (ClientId == Guid.Empty)
                throw new ArgumentException("ClientId cannot be empty");

            if (string.IsNullOrWhiteSpace(Email))
                throw new ArgumentException("Email is required");

            if (!IsValidEmail(Email))
                throw new ArgumentException("Email format is invalid");

            if (TenantId == Guid.Empty)
                throw new ArgumentException("TenantId cannot be empty");
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Adres klienta (value object)
    /// </summary>
    public record Address(
        string? Line1,
        string? Line2,
        string? City,
        string? PostalCode,
        string? State,
        string? Country
    );
}
