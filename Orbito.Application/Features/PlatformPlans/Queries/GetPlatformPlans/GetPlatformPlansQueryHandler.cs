using MediatR;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Common;

namespace Orbito.Application.Features.PlatformPlans.Queries.GetPlatformPlans;

public class GetPlatformPlansQueryHandler : IRequestHandler<GetPlatformPlansQuery, Result<IEnumerable<PlatformPlanDto>>>
{
    private readonly IPlatformPlanRepository _platformPlanRepository;
    private readonly ILogger<GetPlatformPlansQueryHandler> _logger;

    public GetPlatformPlansQueryHandler(
        IPlatformPlanRepository platformPlanRepository,
        ILogger<GetPlatformPlansQueryHandler> logger)
    {
        _platformPlanRepository = platformPlanRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<PlatformPlanDto>>> Handle(
        GetPlatformPlansQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var plans = await _platformPlanRepository.GetAllActiveAsync(cancellationToken);

            var dtos = plans.Select(p => new PlatformPlanDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price.Amount,
                p.Price.Currency.Code,
                p.TrialDays,
                p.IsActive,
                p.FeaturesJson,
                p.SortOrder));

            _logger.LogDebug("Retrieved {Count} active platform plans", dtos.Count());

            return Result.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving platform plans");
            return Result.Failure<IEnumerable<PlatformPlanDto>>(
                Orbito.Domain.Errors.DomainErrors.General.UnexpectedError);
        }
    }
}
