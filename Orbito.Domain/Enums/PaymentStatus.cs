namespace Orbito.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4,
        Refunded = 5,
        PartiallyRefunded = 6,
        Cancelled = 7
    }
}

