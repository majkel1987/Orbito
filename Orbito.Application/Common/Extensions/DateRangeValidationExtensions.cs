using FluentValidation;

namespace Orbito.Application.Common.Extensions;

/// <summary>
/// FluentValidation extension methods for date range validation.
/// Centralizes common date range validation rules used across payment analytics queries.
/// </summary>
public static class DateRangeValidationExtensions
{
    /// <summary>
    /// Maximum allowed date range in days (1 year)
    /// </summary>
    public const int MaxDateRangeDays = 365;

    /// <summary>
    /// Adds standard date range validation rules for StartDate property.
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder for StartDate</param>
    /// <param name="getEndDate">Function to get the EndDate for comparison</param>
    /// <returns>The rule builder with validation rules applied</returns>
    public static IRuleBuilderOptions<T, DateTime> IsValidStartDate<T>(
        this IRuleBuilder<T, DateTime> ruleBuilder,
        Func<T, DateTime> getEndDate)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Start date is required")
            .GreaterThan(DateTime.MinValue)
            .WithMessage("Start date must be a valid date")
            .Must((instance, startDate) => startDate <= getEndDate(instance))
            .WithMessage("Start date must be less than or equal to end date");
    }

    /// <summary>
    /// Adds standard date range validation rules for EndDate property.
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder for EndDate</param>
    /// <param name="getStartDate">Function to get the StartDate for comparison</param>
    /// <returns>The rule builder with validation rules applied</returns>
    public static IRuleBuilderOptions<T, DateTime> IsValidEndDate<T>(
        this IRuleBuilder<T, DateTime> ruleBuilder,
        Func<T, DateTime> getStartDate)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("End date is required")
            .Must((instance, endDate) => endDate >= getStartDate(instance))
            .WithMessage("End date must be greater than or equal to start date")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("End date cannot be in the future");
    }

    /// <summary>
    /// Adds validation rule to ensure date range does not exceed maximum allowed days.
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder for StartDate</param>
    /// <param name="getEndDate">Function to get the EndDate</param>
    /// <param name="maxDays">Maximum allowed days (defaults to 365)</param>
    /// <returns>The rule builder with validation rule applied</returns>
    public static IRuleBuilderOptions<T, DateTime> WithinMaxDateRange<T>(
        this IRuleBuilder<T, DateTime> ruleBuilder,
        Func<T, DateTime> getEndDate,
        int maxDays = MaxDateRangeDays)
    {
        return ruleBuilder
            .Must((instance, startDate) => (getEndDate(instance) - startDate).TotalDays <= maxDays)
            .WithMessage($"Date range cannot exceed {maxDays} days");
    }

    /// <summary>
    /// Adds standard optional ProviderId validation.
    /// Validates that if a ProviderId is provided, it must be a valid non-empty GUID.
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder for ProviderId</param>
    /// <returns>The rule builder with validation rules applied</returns>
    public static IRuleBuilderOptions<T, Guid?> IsValidOptionalProviderId<T>(
        this IRuleBuilder<T, Guid?> ruleBuilder)
    {
        return ruleBuilder
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("Provider ID must be a valid GUID when provided");
    }

    /// <summary>
    /// Adds standard required ProviderId validation.
    /// </summary>
    /// <typeparam name="T">The type being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder for ProviderId</param>
    /// <returns>The rule builder with validation rules applied</returns>
    public static IRuleBuilderOptions<T, Guid> IsValidRequiredProviderId<T>(
        this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Provider ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Provider ID must be a valid GUID");
    }
}
