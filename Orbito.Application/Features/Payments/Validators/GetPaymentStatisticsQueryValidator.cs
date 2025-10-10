using FluentValidation;
using Orbito.Application.Common.Constants;
using Orbito.Application.Features.Payments.Queries.GetPaymentStatistics;

namespace Orbito.Application.Features.Payments.Validators;

/// <summary>
/// Validator for GetPaymentStatisticsQuery
/// </summary>
public class GetPaymentStatisticsQueryValidator : AbstractValidator<GetPaymentStatisticsQuery>
{

    public GetPaymentStatisticsQueryValidator()
    {
        // Date validation
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required")
            .Must(BeValidDate)
            .WithMessage("Start date must be a valid date");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .Must(BeValidDate)
            .WithMessage("End date must be a valid date");

        // Date range validation
        RuleFor(x => x)
            .Must(HaveValidDateRange)
            .WithMessage($"Date range must be between {ValidationConstants.MinDateRangeDays} and {ValidationConstants.MaxDateRangeDays} days")
            .Must(StartDateNotInFuture)
            .WithMessage("Start date cannot be in the future")
            .Must(EndDateNotInFuture)
            .WithMessage("End date cannot be in the future");

        // Provider ID validation (optional but if provided, must be valid GUID)
        RuleFor(x => x.ProviderId)
            .Must(BeValidGuid)
            .WithMessage("Provider ID must be a valid GUID")
            .When(x => x.ProviderId.HasValue);
    }

    private static bool BeValidDate(DateTime date)
    {
        return date != default && date != DateTime.MinValue && date != DateTime.MaxValue;
    }

    private static bool HaveValidDateRange(GetPaymentStatisticsQuery query)
    {
        if (!BeValidDate(query.StartDate) || !BeValidDate(query.EndDate))
            return false;

        var dateRange = (query.EndDate - query.StartDate).TotalDays;
        return dateRange >= ValidationConstants.MinDateRangeDays && dateRange <= ValidationConstants.MaxDateRangeDays;
    }

    private static bool StartDateNotInFuture(GetPaymentStatisticsQuery query)
    {
        return query.StartDate <= DateTime.UtcNow.AddDays(1); // Allow 1 day buffer for timezone differences
    }

    private static bool EndDateNotInFuture(GetPaymentStatisticsQuery query)
    {
        return query.EndDate <= DateTime.UtcNow.AddDays(1); // Allow 1 day buffer for timezone differences
    }

    private static bool BeValidGuid(Guid? guid)
    {
        return !guid.HasValue || guid.Value != Guid.Empty;
    }
}
