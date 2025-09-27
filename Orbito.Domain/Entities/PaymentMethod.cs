using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    public class PaymentMethod : IMustHaveTenant
    {
        public Guid Id { get; set; }
        public TenantId TenantId { get; set; }
        public Guid ClientId { get; set; }

        // Payment Method Details
        public PaymentMethodType Type { get; set; }
        public string Token { get; private set; } = string.Empty; // Encrypted payment method token
        public string? LastFourDigits { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsDefault { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public Client Client { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = [];

        private PaymentMethod() { } // EF Core

        public static PaymentMethod Create(
            TenantId tenantId,
            Guid clientId,
            PaymentMethodType type,
            string token,
            string? lastFourDigits = null,
            DateTime? expiryDate = null,
            bool isDefault = false)
        {
            // Walidacja parametrów
            if (tenantId == null)
                throw new ArgumentNullException(nameof(tenantId));
            
            if (clientId == Guid.Empty)
                throw new ArgumentException("Client ID cannot be empty", nameof(clientId));
            
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be null or empty", nameof(token));
            
            if (token.Length < 10)
                throw new ArgumentException("Token must be at least 10 characters long", nameof(token));

            return new PaymentMethod
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ClientId = clientId,
                Type = type,
                Token = token, // W rzeczywistej implementacji token powinien być zaszyfrowany
                LastFourDigits = lastFourDigits,
                ExpiryDate = expiryDate,
                IsDefault = isDefault,
                CreatedAt = DateTime.UtcNow
            };
        }

        // Business Operations
        public void UpdateToken(string newToken)
        {
            if (string.IsNullOrWhiteSpace(newToken))
                throw new ArgumentException("Token cannot be null or empty", nameof(newToken));
            
            if (newToken.Length < 10)
                throw new ArgumentException("Token must be at least 10 characters long", nameof(newToken));
            
            Token = newToken; // W rzeczywistej implementacji token powinien być zaszyfrowany
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAsDefault()
        {
            IsDefault = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveAsDefault()
        {
            IsDefault = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateExpiryDate(DateTime? expiryDate)
        {
            ExpiryDate = expiryDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsExpired(int defaultExpiryYears = 2)
        {
            // Jeśli nie ma daty wygaśnięcia, sprawdź czy metoda płatności ma domyślny okres ważności
            if (!ExpiryDate.HasValue)
            {
                // Dla metod płatności bez daty wygaśnięcia (np. bank transfer, PayPal)
                // sprawdź czy nie minęło więcej niż określona liczba lat od utworzenia
                return CreatedAt.AddYears(defaultExpiryYears) < DateTime.UtcNow;
            }
            
            return ExpiryDate.Value < DateTime.UtcNow;
        }

        public bool CanBeUsed()
        {
            return !IsExpired();
        }
    }
}
