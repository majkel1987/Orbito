using MediatR;
using Orbito.Application.DTOs;
using Orbito.Domain.Common;

namespace Orbito.Application.Clients.Commands.CreateClient
{
    /// <summary>
    /// Command to create a new client.
    /// 
    /// BUSINESS RULE: UserId and DirectEmail are mutually exclusive (XOR).
    /// You must provide EITHER UserId OR DirectEmail, but NOT both.
    /// 
    /// SCENARIO A - Client with existing User account:
    ///   - Provide: userId (required)
    ///   - Optional: companyName, phone
    ///   - DO NOT provide: directEmail, directFirstName, directLastName
    ///   Example: { "userId": "5fa7c148-7bd9-4ba9-8ac0-211db8c04d46", "companyName": "Acme Corp" }
    /// 
    /// SCENARIO B - Direct client (without User account):
    ///   - Provide: directEmail (required), directFirstName (required), directLastName (required)
    ///   - Optional: companyName, phone
    ///   - DO NOT provide: userId
    ///   Example: { "directEmail": "coyote@acme.com", "directFirstName": "Willy", "directLastName": "Coyote" }
    /// </summary>
    public record CreateClientCommand(
        /// <summary>
        /// User ID for clients with existing Identity account.
        /// Required for Scenario A (withAccount).
        /// Must be a valid GUID of an existing ApplicationUser.
        /// Mutually exclusive with DirectEmail.
        /// </summary>
        Guid? UserId,
        
        /// <summary>
        /// Company name (optional for both scenarios).
        /// Maximum length: 200 characters.
        /// </summary>
        string? CompanyName,
        
        /// <summary>
        /// Phone number (optional for both scenarios).
        /// Format: ^[\+]?[1-9][\d]{0,15}$ (e.g., +48123456789 or 1234567890)
        /// Maximum length: 20 characters.
        /// </summary>
        string? Phone,
        
        /// <summary>
        /// Direct email for clients without Identity account.
        /// Required for Scenario B (direct client).
        /// Must be a valid email address.
        /// Mutually exclusive with UserId.
        /// </summary>
        string? DirectEmail,
        
        /// <summary>
        /// First name for direct clients (without Identity account).
        /// Required for Scenario B when DirectEmail is provided.
        /// Maximum length: 100 characters.
        /// </summary>
        string? DirectFirstName,
        
        /// <summary>
        /// Last name for direct clients (without Identity account).
        /// Required for Scenario B when DirectEmail is provided.
        /// Maximum length: 100 characters.
        /// </summary>
        string? DirectLastName
    ) : IRequest<Result<ClientDto>>;
}
