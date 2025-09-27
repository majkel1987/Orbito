using Orbito.Domain.Enums;

namespace Orbito.Application.DTOs
{
    public record PaymentMethodDto
    {
        public Guid Id { get; init; }
        public Guid TenantId { get; init; }
        public Guid ClientId { get; init; }
        public PaymentMethodType Type { get; init; }
        public string? LastFourDigits { get; init; }
        public DateTime? ExpiryDate { get; init; }
        public bool IsDefault { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public bool IsExpired { get; init; }
        public bool CanBeUsed { get; init; }
    }
}
