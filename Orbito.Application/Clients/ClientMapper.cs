using Orbito.Application.DTOs;
using Orbito.Domain.Entities;

namespace Orbito.Application.Clients;

/// <summary>
/// Mapping utilities for Client entity to DTOs.
/// Centralizes mapping logic to avoid duplication across handlers.
/// </summary>
public static class ClientMapper
{
    /// <summary>
    /// Maps a Client entity to ClientDto.
    /// </summary>
    public static ClientDto ToDto(Client client)
    {
        ArgumentNullException.ThrowIfNull(client);

        return new ClientDto
        {
            Id = client.Id,
            TenantId = client.TenantId.Value,
            UserId = client.UserId,
            CompanyName = client.CompanyName,
            Phone = client.Phone,
            DirectEmail = client.DirectEmail,
            DirectFirstName = client.DirectFirstName,
            DirectLastName = client.DirectLastName,
            IsActive = client.IsActive,
            CreatedAt = client.CreatedAt,
            Email = client.Email ?? string.Empty,
            FirstName = client.FirstName ?? string.Empty,
            LastName = client.LastName ?? string.Empty,
            FullName = client.FullName ?? string.Empty,
            UserEmail = client.User?.Email,
            UserFirstName = client.User?.FirstName,
            UserLastName = client.User?.LastName
        };
    }

    /// <summary>
    /// Maps a collection of Client entities to ClientDto list.
    /// </summary>
    public static IReadOnlyList<ClientDto> ToDto(IEnumerable<Client> clients)
    {
        ArgumentNullException.ThrowIfNull(clients);

        return clients.Select(ToDto).ToList();
    }
}
