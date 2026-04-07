using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Providers.Queries.GetProviderByUserId;

/// <summary>
/// Query for retrieving a provider by its associated user identifier.
/// </summary>
public record GetProviderByUserIdQuery(Guid UserId) : IRequest<Result<ProviderDto>>;
