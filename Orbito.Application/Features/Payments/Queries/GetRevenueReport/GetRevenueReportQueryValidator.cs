using FluentValidation;
using Orbito.Application.Features.Payments.Queries.GetRevenueReport;

namespace Orbito.Application.Features.Payments.Queries.GetRevenueReport;

/// <summary>
/// Validator for get revenue report query
/// </summary>
public class GetRevenueReportQueryValidator : AbstractValidator<GetRevenueReportQuery>
{
    public GetRevenueReportQueryValidator()
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
            .WithMessage("Provider ID is required")
            .NotEqual(Guid.Empty)
            .WithMessage("Provider ID must be a valid GUID");
    }
}
