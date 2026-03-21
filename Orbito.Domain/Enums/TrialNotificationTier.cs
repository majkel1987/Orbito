namespace Orbito.Domain.Enums;

/// <summary>
/// Poziom wysłanego powiadomienia o zbliżającym się końcu triala.
/// Używany do deduplikacji powiadomień.
/// </summary>
public enum TrialNotificationTier
{
    None = 0,
    FiveDays = 1,
    ThreeDays = 2,
    OneDay = 3,
    Expired = 4
}
