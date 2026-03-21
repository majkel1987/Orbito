using MediatR;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.PlatformPlans.Queries.GetPlatformPlans;

public record GetPlatformPlansQuery() : IRequest<Result<IEnumerable<PlatformPlanDto>>>;

public record PlatformPlanDto(
    Guid Id,
    string Name,
    string? Description,
    decimal PriceAmount,
    string PriceCurrency,
    int TrialDays,
    bool IsActive,
    string? FeaturesJson,
    int SortOrder);
