using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using System.Security.Claims;

namespace Orbito.Infrastructure.Services
{
    /// <summary>
    /// Service for getting user context information from claims
    /// </summary>
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<UserContextService> _logger;

        public UserContextService(
            IHttpContextAccessor httpContextAccessor,
            IClientRepository clientRepository,
            ILogger<UserContextService> logger)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Guid? GetCurrentUserId()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }

                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("User ID not found in claims or invalid format");
                    return null;
                }

                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user ID from claims");
                return null;
            }
        }

        public async Task<Guid?> GetCurrentClientIdAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning("Cannot get client ID - user not authenticated");
                    return null;
                }

                var client = await _clientRepository.GetByUserIdAsync(userId.Value, cancellationToken);
                if (client == null)
                {
                    _logger.LogWarning("No client found for user ID {UserId}", userId);
                    return null;
                }

                _logger.LogDebug("Found client ID {ClientId} for user ID {UserId}", client.Id, userId);
                return client.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current client ID for user");
                return null;
            }
        }

        public string? GetCurrentUserEmail()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }

                return user.FindFirst(ClaimTypes.Email)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user email from claims");
                return null;
            }
        }

        public string? GetCurrentUserName()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }

                // Try GivenName + Surname first (standard claims)
                var givenName = user.FindFirst(ClaimTypes.GivenName)?.Value;
                var surname = user.FindFirst(ClaimTypes.Surname)?.Value;

                if (!string.IsNullOrWhiteSpace(givenName))
                {
                    return string.IsNullOrWhiteSpace(surname)
                        ? givenName
                        : $"{givenName} {surname}";
                }

                // Fallback to Name claim
                var name = user.FindFirst(ClaimTypes.Name)?.Value;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }

                // Final fallback to email
                return GetCurrentUserEmail();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user name from claims");
                return null;
            }
        }

        public string? GetCurrentUserRole()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }

                return user.FindFirst(ClaimTypes.Role)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user role from claims");
                return null;
            }
        }

        public bool IsAuthenticated()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                return user?.Identity?.IsAuthenticated == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authentication status");
                return false;
            }
        }
    }
}
