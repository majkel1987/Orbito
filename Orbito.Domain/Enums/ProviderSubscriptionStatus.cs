namespace Orbito.Domain.Enums;

/// <summary>
/// Status subskrypcji platformowej Providera (Provider płaci Orbito).
/// Oddzielone od SubscriptionStatus (klienci płacą Providerowi).
/// </summary>
public enum ProviderSubscriptionStatus
{
    Trial = 0,
    Active = 1,
    Expired = 2,
    Cancelled = 3
}
