using Microsoft.AspNetCore.Http;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.ValueObjects;
using System.Security.Claims;

namespace Orbito.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
        {
            try
            {
                _logger.LogDebug("TenantMiddleware: Sprawdzanie kontekstu tenanta dla {Path}", context.Request.Path);
                
                // Wyczyść poprzedni kontekst tenanta
                tenantContext.ClearTenant();

                // Sprawdź czy użytkownik jest zalogowany
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    _logger.LogDebug("TenantMiddleware: Użytkownik jest zalogowany: {UserId}", 
                        context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    
                    // Pobierz TenantId z JWT claims
                    var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value;
                    _logger.LogDebug("TenantMiddleware: TenantId claim: {TenantIdClaim}", tenantIdClaim);
                    
                    if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantIdGuid))
                    {
                        var tenantId = TenantId.Create(tenantIdGuid);
                        tenantContext.SetTenant(tenantId);
                        
                        _logger.LogInformation("Tenant context ustawiony: {TenantId} dla użytkownika {UserId}", 
                            tenantId.Value, context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    }
                    else
                    {
                        _logger.LogWarning("Brak TenantId w claims dla użytkownika {UserId}", 
                            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    }
                }
                else
                {
                    _logger.LogDebug("TenantMiddleware: Użytkownik nie jest zalogowany");
                }

                // Sprawdź czy TenantId jest w headerze (dla API calls)
                var tenantHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
                if (!string.IsNullOrEmpty(tenantHeader) && Guid.TryParse(tenantHeader, out var headerTenantId))
                {
                    var tenantId = TenantId.Create(headerTenantId);
                    tenantContext.SetTenant(tenantId);
                    
                    _logger.LogDebug("Tenant context ustawiony z header: {TenantId}", tenantId.Value);
                }

                // Sprawdź czy TenantId jest w query string (dla webhooks)
                var tenantQuery = context.Request.Query["tenantId"].FirstOrDefault();
                if (!string.IsNullOrEmpty(tenantQuery) && Guid.TryParse(tenantQuery, out var queryTenantId))
                {
                    var tenantId = TenantId.Create(queryTenantId);
                    tenantContext.SetTenant(tenantId);
                    
                    _logger.LogDebug("Tenant context ustawiony z query: {TenantId}", tenantId.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas ustawiania kontekstu tenanta");
                // Nie przerywamy requestu, tylko logujemy błąd
            }

            await _next(context);
        }
    }
}
