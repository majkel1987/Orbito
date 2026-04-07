using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Clients.Commands.UpdateClient
{
    /// <summary>
    /// Command to update an existing client.
    /// Only updates provided non-null fields.
    /// Cannot change UserId (immutable once set).
    ///
    /// BUSINESS RULE: DirectEmail, DirectFirstName, DirectLastName are mutually dependent.
    /// If you update one, you must provide all three (XOR logic enforced by validator).
    /// </summary>
    /// <param name="Id">Client ID to update.</param>
    /// <param name="CompanyName">Optional company name.</param>
    /// <param name="Phone">Optional phone number.</param>
    /// <param name="DirectEmail">Direct email (required if DirectFirstName/DirectLastName are provided).</param>
    /// <param name="DirectFirstName">Direct first name (required if DirectEmail is provided).</param>
    /// <param name="DirectLastName">Direct last name (required if DirectEmail is provided).</param>
    public record UpdateClientCommand(
        Guid Id,
        string? CompanyName,
        string? Phone,
        string? DirectEmail,
        string? DirectFirstName,
        string? DirectLastName
    ) : IRequest<Result<ClientDto>>;
}
