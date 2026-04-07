namespace Orbito.Domain.ValueObjects
{
    /// <summary>
    /// Currency value object with validation and common operations
    /// </summary>
    public sealed record Currency : IEquatable<Currency>
    {
        /// <summary>
        /// Currency code (ISO 4217)
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Currency symbol
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Number of decimal places for this currency
        /// </summary>
        public int DecimalPlaces { get; }

        private Currency(string code, string symbol, int decimalPlaces)
        {
            Code = code;
            Symbol = symbol;
            DecimalPlaces = decimalPlaces;
        }

        /// <summary>
        /// Create a currency with validation
        /// </summary>
        /// <param name="code">Currency code (e.g., "USD", "PLN")</param>
        /// <param name="symbol">Currency symbol (e.g., "$", "zł")</param>
        /// <param name="decimalPlaces">Number of decimal places</param>
        /// <returns>Currency instance</returns>
        public static Currency Create(string code, string symbol, int decimalPlaces = 2)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Currency code cannot be null or empty", nameof(code));

            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("Currency symbol cannot be null or empty", nameof(symbol));

            if (decimalPlaces < 0)
                throw new ArgumentException("Decimal places cannot be negative", nameof(decimalPlaces));

            return new Currency(code.ToUpperInvariant(), symbol, decimalPlaces);
        }

        /// <summary>
        /// Common currencies
        /// </summary>
        public static Currency PLN => Create("PLN", "zł", 2);
        public static Currency USD => Create("USD", "$", 2);
        public static Currency EUR => Create("EUR", "€", 2);
        public static Currency GBP => Create("GBP", "£", 2);

        /// <summary>
        /// Supported currency codes (ISO 4217)
        /// </summary>
        private static readonly HashSet<string> SupportedCurrencyCodes = new()
        {
            "USD", "EUR", "GBP", "PLN", "CAD", "AUD", "JPY", "CHF",
            "SEK", "NOK", "DKK", "CZK", "HUF", "RON", "BGN"
        };

        /// <summary>
        /// Check if a currency code is supported
        /// </summary>
        /// <param name="code">Currency code to validate</param>
        /// <returns>True if currency is supported, false otherwise</returns>
        public static bool IsSupported(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            return SupportedCurrencyCodes.Contains(code.ToUpperInvariant());
        }

        /// <summary>
        /// Check if two currencies are the same
        /// </summary>
        public bool Equals(Currency? other)
        {
            if (other is null) return false;
            return Code == other.Code;
        }

        public override int GetHashCode() => Code.GetHashCode();

        public override string ToString() => Code;

        /// <summary>
        /// Implicit conversion to string for backward compatibility
        /// </summary>
        public static implicit operator string(Currency currency) => currency.Code;

        /// <summary>
        /// Format amount with currency symbol
        /// </summary>
        /// <param name="amount">Amount to format</param>
        /// <returns>Formatted string</returns>
        public string FormatAmount(decimal amount)
        {
            return string.Format("{0:F{1}} {2}", amount, DecimalPlaces, Symbol);
        }
    }
}
