namespace Orbito.Domain.ValueObjects
{
    public sealed class BillingPeriod : IEquatable<BillingPeriod>
    {
        public int Value { get; }
        public BillingPeriodType Type { get; }
        public int DaysCount => Type switch
        {
            BillingPeriodType.Daily => Value,
            BillingPeriodType.Weekly => Value * 7,
            BillingPeriodType.Monthly => Value * 30,
            BillingPeriodType.Yearly => Value * 365,
            _ => throw new InvalidOperationException($"Unknown billing period type: {Type}")
        };

        private BillingPeriod(int value, BillingPeriodType type)
        {
            Value = value;
            Type = type;
        }

        public static BillingPeriod Create(int value, BillingPeriodType type)
        {
            if (value <= 0)
                throw new ArgumentException("Value must be positive", nameof(value));

            return new BillingPeriod(value, type);
        }

        public static BillingPeriod Monthly() => new(1, BillingPeriodType.Monthly);
        public static BillingPeriod Yearly() => new(1, BillingPeriodType.Yearly);
        public static BillingPeriod Weekly() => new(1, BillingPeriodType.Weekly);
        public static BillingPeriod Days(int days) => new(days, BillingPeriodType.Daily);

        public DateTime GetNextBillingDate(DateTime startDate)
        {
            return Type switch
            {
                BillingPeriodType.Daily => startDate.AddDays(Value),
                BillingPeriodType.Weekly => startDate.AddDays(Value * 7),
                BillingPeriodType.Monthly => startDate.AddMonths(Value),
                BillingPeriodType.Yearly => startDate.AddYears(Value),
                _ => throw new InvalidOperationException($"Unknown billing period type: {Type}")
            };
        }

        public bool Equals(BillingPeriod? other)
        {
            if (other is null) return false;
            return Value == other.Value && Type == other.Type;
        }

        public override bool Equals(object? obj) => obj is BillingPeriod other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Value, Type);
        public override string ToString() => $"{Value} {Type}";
    }

    public enum BillingPeriodType
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Yearly = 4
    }
}
