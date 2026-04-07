using FluentValidation;
using Orbito.Application.Common.Extensions;

namespace Orbito.Application.Features.Payments.Queries.GetRevenueReport;

/// <summary>
/// Validator for get revenue report query.
/// Uses shared date range validation extensions for consistency.
/// </summary>
public class GetRevenueReportQueryValidator : AbstractValidator<GetRevenueReportQuery>
{
    public GetRevenueReportQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .IsValidStartDate(x => x.EndDate)
            .WithinMaxDateRange(x => x.EndDate);

        RuleFor(x => x.EndDate)
            .IsValidEndDate(x => x.StartDate);

        RuleFor(x => x.ProviderId)
            .IsValidRequiredProviderId();
    }
}
