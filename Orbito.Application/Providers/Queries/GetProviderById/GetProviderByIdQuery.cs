using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Providers.Queries.GetProviderById;

/// <summary>
/// Query for retrieving a provider by its unique identifier.
/// </summary>
public record GetProviderByIdQuery(Guid Id) : IRequest<Result<ProviderDto>>;
