using Orbito.Domain.Enums;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;

namespace Orbito.Domain.Entities
{
    /// <summary>
    /// Entity for payment methods
    /// </summary>
    public class PaymentMethod : IMustHaveTenant
    {
        /// <summary>
        /// Unique identifier for the payment method
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tenant ID for multi-tenancy
        /// </summary>
        public TenantId TenantId { get; set; }

        /// <summary>
        /// Client ID
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// Navigation property to Client
        /// </summary>
        public Client? Client { get; set; }

        /// <summary>
        /// Payment method type
        /// </summary>
        public PaymentMethodType Type { get; set; }

        /// <summary>
        /// Encrypted payment method token
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Last four digits of the card
        /// </summary>
        public string? LastFourDigits { get; set; }

        /// <summary>
        /// Expiry date of the payment method
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Whether this is the default payment method
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Created date
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Updated date
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Creates a new payment method
        /// </summary>
        public static PaymentMethod Create(
            TenantId tenantId,
            Guid clientId,
            PaymentMethodType type,
            string token,
            string? lastFourDigits = null,
            DateTime? expiryDate = null,
            bool isDefault = false)
        {
            return new PaymentMethod
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ClientId = clientId,
                Type = type,
                Token = token,
                LastFourDigits = lastFourDigits,
                ExpiryDate = expiryDate,
                IsDefault = isDefault,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Updates the payment method token
        /// </summary>
        public void UpdateToken(string newToken)
        {
            if (string.IsNullOrWhiteSpace(newToken))
                throw new ArgumentException("Token cannot be null or empty", nameof(newToken));

            Token = newToken;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets this payment method as default
        /// </summary>
        public void SetAsDefault()
        {
            IsDefault = true;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Removes this payment method as default
        /// </summary>
        public void RemoveAsDefault()
        {
            IsDefault = false;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the payment method is expired
        /// Cards expire at the end of the month specified in ExpiryDate
        /// </summary>
        public bool IsExpired()
        {
            if (ExpiryDate == null)
                return false;

            // Cards expire on the last day of the month
            var lastDayOfMonth = new DateTime(
                ExpiryDate.Value.Year,
                ExpiryDate.Value.Month,
                DateTime.DaysInMonth(ExpiryDate.Value.Year, ExpiryDate.Value.Month));

            return lastDayOfMonth.Date < DateTime.UtcNow.Date;
        }

        /// <summary>
        /// Checks if the payment method can be used
        /// </summary>
        public bool CanBeUsed()
        {
            return !IsExpired() && !string.IsNullOrEmpty(Token);
        }
    }
}