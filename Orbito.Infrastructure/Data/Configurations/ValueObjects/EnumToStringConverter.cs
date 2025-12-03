using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Orbito.Infrastructure.Data.Configurations.ValueObjects
{
    /// <summary>
    /// Generic Value Converter for enum to string conversion
    /// Prevents InvalidCastException by properly handling type conversion
    /// </summary>
    /// <typeparam name="TEnum">The enum type to convert</typeparam>
    public class EnumToStringConverter<TEnum> : ValueConverter<TEnum, string>
        where TEnum : struct, Enum
    {
        /// <summary>
        /// Creates a new instance of EnumToStringConverter
        /// </summary>
        public EnumToStringConverter()
            : base(
                v => v.ToString(),                                    // Enum → String (to database)
                v => Enum.Parse<TEnum>(v, true))                     // String → Enum (from database)
        {
        }

        /// <summary>
        /// Creates a new instance of EnumToStringConverter with custom mapping expressions
        /// </summary>
        /// <param name="convertToProviderExpression">Expression to convert enum to string</param>
        /// <param name="convertFromProviderExpression">Expression to convert string to enum</param>
        public EnumToStringConverter(
            System.Linq.Expressions.Expression<System.Func<TEnum, string>> convertToProviderExpression,
            System.Linq.Expressions.Expression<System.Func<string, TEnum>> convertFromProviderExpression)
            : base(convertToProviderExpression, convertFromProviderExpression)
        {
        }
    }
}
