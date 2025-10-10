using FluentValidation;
using Orbito.Application.Common.Constants;
using Orbito.Application.Features.Payments.Queries.GetRevenueReport;

namespace Orbito.Application.Features.Payments.Validators;

/// <summary>
/// Validator for GetRevenueReportQuery
/// </summary>
public class GetRevenueReportQueryValidator : AbstractValidator<GetRevenueReportQuery>
{
    private const int MaxDateRangeDays = 365; // Maximum 1 year
    private const int MinDateRangeDays = 1;   // Minimum 1 day

    public GetRevenueReportQueryValidator()
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
            .WithMessage($"Date range must be between {MinDateRangeDays} and {MaxDateRangeDays} days")
            .Must(StartDateNotInFuture)
            .WithMessage("Start date cannot be in the future")
            .Must(EndDateNotInFuture)
            .WithMessage("End date cannot be in the future");

        // Provider ID validation (required and must be valid GUID)
        RuleFor(x => x.ProviderId)
            .NotEmpty()
            .WithMessage("Provider ID is required")
            .Must(BeValidGuid)
            .WithMessage("Provider ID must be a valid GUID");
    }

    private static bool BeValidDate(DateTime date)
    {
        return date != default && date != DateTime.MinValue && date != DateTime.MaxValue;
    }

    private static bool HaveValidDateRange(GetRevenueReportQuery query)
    {
        if (!BeValidDate(query.StartDate) || !BeValidDate(query.EndDate))
            return false;

        var dateRange = (query.EndDate - query.StartDate).TotalDays;
        return dateRange >= MinDateRangeDays && dateRange <= MaxDateRangeDays;
    }

    private static bool StartDateNotInFuture(GetRevenueReportQuery query)
    {
        return query.StartDate <= DateTime.UtcNow.AddDays(1); // Allow 1 day buffer for timezone differences
    }

    private static bool EndDateNotInFuture(GetRevenueReportQuery query)
    {
        return query.EndDate <= DateTime.UtcNow.AddDays(1); // Allow 1 day buffer for timezone differences
    }

    private static bool BeValidGuid(Guid guid)
    {
        return guid != Guid.Empty;
    }
}
