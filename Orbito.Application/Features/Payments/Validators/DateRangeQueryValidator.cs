using FluentValidation;
using Orbito.Application.Common.Models;

namespace Orbito.Application.Features.Payments.Validators;

/// <summary>
/// Interface for queries that have date range validation requirements
/// </summary>
public interface IDateRangeQuery
{
    DateTime StartDate { get; }
    DateTime EndDate { get; }
    Guid? ProviderId { get; }
}

/// <summary>
/// Base validator for queries with date ranges
/// DRY principle - shared validation logic for all metrics queries
/// </summary>
public abstract class DateRangeQueryValidator<T> : AbstractValidator<T>
    where T : IDateRangeQuery
{
    protected DateRangeQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required")
            .LessThanOrEqualTo(x => x.EndDate)
            .WithMessage("Start date must be less than or equal to end date")
            .GreaterThan(DateTime.MinValue)
            .WithMessage("Start date must be a valid date");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be greater than or equal to start date")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("End date cannot be in the future");

        RuleFor(x => x.StartDate)
            .Must((query, startDate) => (query.EndDate - startDate).TotalDays <= 365)
            .WithMessage("Date range cannot exceed 365 days");

        RuleFor(x => x.ProviderId)
            .NotEmpty()
            .When(x => x.ProviderId.HasValue)
            .WithMessage("Provider ID must be a valid GUID when provided");
    }
}
