namespace Orbito.Domain.Enums;

/// <summary>
/// Types of discrepancies that can be found during payment reconciliation
/// </summary>
public enum DiscrepancyType
{
    /// <summary>
    /// Payment status differs between Orbito and Stripe
    /// </summary>
    StatusMismatch = 1,

    /// <summary>
    /// Payment amount differs between Orbito and Stripe
    /// </summary>
    AmountMismatch = 2,

    /// <summary>
    /// Payment exists in Orbito but not in Stripe
    /// </summary>
    MissingInStripe = 3,

    /// <summary>
    /// Payment exists in Stripe but not in Orbito
    /// </summary>
    MissingInOrbito = 4
}
