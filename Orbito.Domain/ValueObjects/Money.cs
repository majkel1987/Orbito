namespace Orbito.Domain.ValueObjects
{
    /// <summary>
    /// Money value object with currency context and validation
    /// </summary>
    public sealed record Money : IEquatable<Money>
    {
        /// <summary>
        /// Amount value
        /// </summary>
        public decimal Amount { get; }

        /// <summary>
        /// Currency information
        /// </summary>
        public Currency Currency { get; }

        private Money(decimal amount, Currency currency)
        {
            Amount = amount;
            Currency = currency;
        }

        /// <summary>
        /// Create money with validation
        /// </summary>
        /// <param name="amount">Amount value</param>
        /// <param name="currency">Currency</param>
        /// <returns>Money instance</returns>
        public static Money Create(decimal amount, Currency currency)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative.", nameof(amount));
            
            if (currency is null)
                throw new ArgumentNullException(nameof(currency));

            return new Money(amount, currency);
        }

        /// <summary>
        /// Create money with currency code (for backward compatibility)
        /// </summary>
        /// <param name="amount">Amount value</param>
        /// <param name="currencyCode">Currency code</param>
        /// <returns>Money instance</returns>
        public static Money Create(decimal amount, string currencyCode)
        {
            var currency = Currency.Create(currencyCode, GetSymbolForCode(currencyCode));
            return Create(amount, currency);
        }

        /// <summary>
        /// Zero amount in specified currency
        /// </summary>
        /// <param name="currency">Currency</param>
        /// <returns>Zero money</returns>
        public static Money Zero(Currency currency) => new(0, currency);

        /// <summary>
        /// Zero amount in specified currency code
        /// </summary>
        /// <param name="currencyCode">Currency code</param>
        /// <returns>Zero money</returns>
        public static Money Zero(string currencyCode) => new(0, Currency.Create(currencyCode, GetSymbolForCode(currencyCode)));

        /// <summary>
        /// Common currency factory methods
        /// </summary>
        public static Money PLN(decimal amount) => Create(amount, Currency.PLN);
        public static Money USD(decimal amount) => Create(amount, Currency.USD);
        public static Money EUR(decimal amount) => Create(amount, Currency.EUR);
        public static Money GBP(decimal amount) => Create(amount, Currency.GBP);

        /// <summary>
        /// Add two money amounts (must be same currency)
        /// </summary>
        /// <param name="other">Other money amount</param>
        /// <returns>Sum of money amounts</returns>
        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot add different currencies: {Currency.Code} and {other.Currency.Code}");

            return new Money(Amount + other.Amount, Currency);
        }

        /// <summary>
        /// Subtract two money amounts (must be same currency)
        /// </summary>
        /// <param name="other">Other money amount</param>
        /// <returns>Difference of money amounts</returns>
        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot subtract different currencies: {Currency.Code} and {other.Currency.Code}");

            return new Money(Amount - other.Amount, Currency);
        }

        /// <summary>
        /// Multiply money by a factor
        /// </summary>
        /// <param name="factor">Multiplication factor</param>
        /// <returns>Multiplied money amount</returns>
        public Money Multiply(decimal factor) => new(Amount * factor, Currency);

        /// <summary>
        /// Check if money amounts are equal
        /// </summary>
        /// <param name="other">Other money amount</param>
        /// <returns>True if equal</returns>
        public bool Equals(Money? other)
        {
            if (other is null) return false;
            return Amount == other.Amount && Currency == other.Currency;
        }

        public override int GetHashCode() => HashCode.Combine(Amount, Currency);

        /// <summary>
        /// String representation with currency formatting
        /// </summary>
        /// <returns>Formatted money string</returns>
        public override string ToString() => Currency.FormatAmount(Amount);

        /// <summary>
        /// Get currency code for backward compatibility
        /// </summary>
        public string CurrencyCode => Currency.Code;

        /// <summary>
        /// Helper method to get symbol for currency code
        /// </summary>
        /// <param name="code">Currency code</param>
        /// <returns>Currency symbol</returns>
        private static string GetSymbolForCode(string code) => code.ToUpperInvariant() switch
        {
            "PLN" => "zł",
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            _ => code
        };
    }
}
