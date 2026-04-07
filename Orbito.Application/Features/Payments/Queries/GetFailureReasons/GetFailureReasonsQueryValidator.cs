using FluentValidation;
using Orbito.Application.Common.Extensions;

namespace Orbito.Application.Features.Payments.Queries.GetFailureReasons;

/// <summary>
/// Validator for get failure reasons query.
/// Uses shared date range validation extensions for consistency.
/// </summary>
public class GetFailureReasonsQueryValidator : AbstractValidator<GetFailureReasonsQuery>
{
    /// <summary>
    /// Maximum allowed top N results
    /// </summary>
    public const int MaxTopN = 100;

    public GetFailureReasonsQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .IsValidStartDate(x => x.EndDate)
            .WithinMaxDateRange(x => x.EndDate);

        RuleFor(x => x.EndDate)
            .IsValidEndDate(x => x.StartDate);

        RuleFor(x => x.ProviderId)
            .IsValidOptionalProviderId();

        RuleFor(x => x.TopN)
            .GreaterThan(0)
            .WithMessage("TopN must be greater than 0 when specified")
            .LessThanOrEqualTo(MaxTopN)
            .WithMessage($"TopN cannot exceed {MaxTopN}")
            .When(x => x.TopN.HasValue);
    }
}
