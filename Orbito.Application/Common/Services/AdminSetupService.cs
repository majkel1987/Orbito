using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Enums;
using Orbito.Domain.Identity;

namespace Orbito.Application.Common.Services
{

    public class AdminSetupService : IAdminSetupService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ITenantValidationBypass _tenantValidationBypass;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminSetupService> _logger;

        public AdminSetupService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ITenantValidationBypass tenantValidationBypass,
            IConfiguration configuration,
            ILogger<AdminSetupService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tenantValidationBypass = tenantValidationBypass;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> IsAdminSetupRequiredAsync()
        {
            try
            {
                // Sprawdź czy istnieje jakikolwiek użytkownik z rolą PlatformAdmin
                var adminUsers = await _userManager.GetUsersInRoleAsync(UserRole.PlatformAdmin.ToString());
                return !adminUsers.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas sprawdzania czy setup admina jest wymagany");
                return false;
            }
        }

        public async Task<bool> CreateInitialAdminAsync(string email, string password, string firstName, string lastName)
        {
            try
            {
                // Sprawdź czy setup jest włączony
                if (!await IsAdminSetupEnabledAsync())
                {
                    _logger.LogWarning("Próba utworzenia admina gdy setup jest wyłączony");
                    return false;
                }

                // Sprawdź czy admin już istnieje
                if (!await IsAdminSetupRequiredAsync())
                {
                    _logger.LogWarning("Próba utworzenia admina gdy już istnieje");
                    return false;
                }

                // Sprawdź czy użytkownik już istnieje
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Użytkownik z emailem {Email} już istnieje", email);
                    return false;
                }

                // CRITICAL: Disable tenant validation for admin setup operations
                // This is necessary because admin setup happens before any tenant context exists
                // UserManager uses the same ApplicationDbContext instance, so this flag will be respected
                _tenantValidationBypass.SkipTenantValidation();

                try
                {
                    // Utwórz nowego użytkownika
                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        UserName = email,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        TenantId = null, // PlatformAdmin nie ma TenantId
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await _userManager.CreateAsync(user, password);
                    if (!result.Succeeded)
                    {
                        _logger.LogError("Błąd podczas tworzenia użytkownika admina: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                        return false;
                    }

                    // Dodaj rolę PlatformAdmin
                    await _userManager.AddToRoleAsync(user, UserRole.PlatformAdmin.ToString());
                }
                finally
                {
                    // Re-enable tenant validation after admin setup operations
                    // This ensures security checks are restored even if an error occurs
                    _tenantValidationBypass.ResetTenantValidation();
                }

                _logger.LogInformation("Początkowy administrator został utworzony: {Email}", email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas tworzenia początkowego administratora");
                return false;
            }
        }

        public Task<bool> IsAdminSetupEnabledAsync()
        {
            try
            {
                // Sprawdź zmienną środowiskową lub konfigurację
                var setupEnabled = _configuration.GetValue<bool>("AdminSetup:Enabled", false);
                var environment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT", "Production");

                // W Development zawsze pozwól na setup
                if (environment == "Development")
                {
                    return Task.FromResult(true);
                }

                // W Production tylko jeśli jest włączone w konfiguracji
                return Task.FromResult(setupEnabled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas sprawdzania czy setup admina jest włączony");
                return Task.FromResult(false);
            }
        }
    }
}
