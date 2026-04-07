using FluentValidation;
using Orbito.Application.Common.Extensions;

namespace Orbito.Application.Features.Payments.Queries.GetPaymentStatistics;

/// <summary>
/// Validator for get payment statistics query.
/// Uses shared date range validation extensions for consistency.
/// </summary>
public class GetPaymentStatisticsQueryValidator : AbstractValidator<GetPaymentStatisticsQuery>
{
    public GetPaymentStatisticsQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .IsValidStartDate(x => x.EndDate)
            .WithinMaxDateRange(x => x.EndDate);

        RuleFor(x => x.EndDate)
            .IsValidEndDate(x => x.StartDate);

        RuleFor(x => x.ProviderId)
            .IsValidOptionalProviderId();
    }
}
