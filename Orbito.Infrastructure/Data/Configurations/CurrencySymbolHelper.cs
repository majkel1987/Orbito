namespace Orbito.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Helper utility for currency symbol resolution
    /// Centralized to avoid duplication across EF configuration classes
    /// </summary>
    public static class CurrencySymbolHelper
    {
        /// <summary>
        /// Gets the currency symbol for a given currency code
        /// </summary>
        /// <param name="code">ISO 4217 currency code</param>
        /// <returns>Currency symbol</returns>
        public static string GetSymbolForCode(string code) => code.ToUpperInvariant() switch
        {
            "PLN" => "zł",
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            _ => code
        };
    }
}
