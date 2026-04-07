namespace Orbito.Domain.ValueObjects;

/// <summary>
/// Value Object representing credit/debit card details
/// </summary>
public sealed class CardDetails : IEquatable<CardDetails>
{
    private static readonly HashSet<string> ValidBrands = new(StringComparer.OrdinalIgnoreCase)
    {
        "Visa",
        "Mastercard",
        "American Express",
        "Discover",
        "JCB",
        "Diners Club",
        "UnionPay"
    };

    /// <summary>
    /// Card brand (Visa, Mastercard, Amex, etc.)
    /// </summary>
    public string Brand { get; private init; }

    /// <summary>
    /// Expiry month (1-12)
    /// </summary>
    public int ExpiryMonth { get; private init; }

    /// <summary>
    /// Expiry year (4 digits)
    /// </summary>
    public int ExpiryYear { get; private init; }

    /// <summary>
    /// Last four digits of the card
    /// </summary>
    public string LastFourDigits { get; private init; }

    private CardDetails()
    {
        Brand = string.Empty;
        LastFourDigits = string.Empty;
    }

    private CardDetails(string brand, int expiryMonth, int expiryYear, string lastFourDigits)
    {
        Brand = brand;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        LastFourDigits = lastFourDigits;
    }

    /// <summary>
    /// Creates a new CardDetails value object
    /// </summary>
    public static CardDetails Create(string brand, int expiryMonth, int expiryYear, string lastFourDigits)
    {
        // 1. Walidacja brand
        if (string.IsNullOrWhiteSpace(brand))
            throw new ArgumentException("Card brand cannot be null or empty", nameof(brand));

        var normalizedBrand = brand.Trim();

        if (!ValidBrands.Contains(normalizedBrand))
            throw new ArgumentException(
                $"Invalid card brand: {normalizedBrand}. Allowed: {string.Join(", ", ValidBrands)}",
                nameof(brand));

        // 2. Walidacja dat
        var currentYear = DateTime.UtcNow.Year;
        var currentMonth = DateTime.UtcNow.Month;

        if (expiryMonth < 1 || expiryMonth > 12)
            throw new ArgumentException("Expiry month must be between 1 and 12", nameof(expiryMonth));

        if (expiryYear < currentYear || expiryYear > currentYear + 20)
            throw new ArgumentException(
                $"Expiry year must be between {currentYear} and {currentYear + 20}",
                nameof(expiryYear));

        if (expiryYear == currentYear && expiryMonth < currentMonth)
            throw new ArgumentException("Card has already expired", nameof(expiryMonth));

        // 3. Walidacja last four digits
        if (string.IsNullOrWhiteSpace(lastFourDigits) ||
            lastFourDigits.Length != 4 ||
            !lastFourDigits.All(char.IsDigit))
            throw new ArgumentException("Last four digits must be exactly 4 digits", nameof(lastFourDigits));

        return new CardDetails(normalizedBrand, expiryMonth, expiryYear, lastFourDigits);
    }

    /// <summary>
    /// Checks if the card is expired
    /// </summary>
    public bool IsExpired()
    {
        var now = DateTime.UtcNow;
        if (ExpiryYear < now.Year)
            return true;
        if (ExpiryYear > now.Year)
            return false;

        return ExpiryMonth < now.Month;
    }

    /// <summary>
    /// Gets expiry date as DateTime (last day of expiry month)
    /// </summary>
    public DateTime GetExpiryDate()
    {
        return new DateTime(ExpiryYear, ExpiryMonth, DateTime.DaysInMonth(ExpiryYear, ExpiryMonth));
    }

    /// <summary>
    /// Gets formatted expiry date (MM/YY)
    /// </summary>
    public string GetFormattedExpiry()
    {
        return $"{ExpiryMonth:D2}/{ExpiryYear % 100:D2}";
    }

    /// <summary>
    /// Gets masked card number (****1234)
    /// </summary>
    public string GetMaskedCardNumber()
    {
        return $"****{LastFourDigits}";
    }

    /// <summary>
    /// Gets months until expiry (negative if expired)
    /// </summary>
    public int GetMonthsUntilExpiry()
    {
        var now = DateTime.UtcNow;
        return (ExpiryYear - now.Year) * 12 + (ExpiryMonth - now.Month);
    }

    public bool Equals(CardDetails? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Brand == other.Brand &&
               ExpiryMonth == other.ExpiryMonth &&
               ExpiryYear == other.ExpiryYear &&
               LastFourDigits == other.LastFourDigits;
    }

    public override bool Equals(object? obj)
    {
        return obj is CardDetails other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Brand, ExpiryMonth, ExpiryYear, LastFourDigits);
    }

    public static bool operator ==(CardDetails? left, CardDetails? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(CardDetails? left, CardDetails? right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Brand) || string.IsNullOrEmpty(LastFourDigits))
            return "Invalid Card";

        return $"{Brand} {GetMaskedCardNumber()} ({GetFormattedExpiry()})";
    }
}