using FluentValidation;
using Orbito.Application.Common.Models;

namespace Orbito.Application.SubscriptionPlans.Queries.GetSubscriptionPlansByProvider;

/// <summary>
/// Validator for GetSubscriptionPlansByProviderQuery.
/// Ensures pagination parameters are within valid bounds.
/// </summary>
public class GetSubscriptionPlansByProviderQueryValidator : AbstractValidator<GetSubscriptionPlansByProviderQuery>
{

    public GetSubscriptionPlansByProviderQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page size must be at least 1")
            .LessThanOrEqualTo(PaginationParams.MaxPageSize)
            .WithMessage($"Page size cannot exceed {PaginationParams.MaxPageSize}");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.SearchTerm))
            .WithMessage("Search term cannot exceed 200 characters");
    }
}
