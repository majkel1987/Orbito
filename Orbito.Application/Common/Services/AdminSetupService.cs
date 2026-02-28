using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Identity;
using Orbito.Domain.Interfaces;

namespace Orbito.Application.Common.Services
{

    public class AdminSetupService : IAdminSetupService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ITenantValidationBypass _tenantValidationBypass;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminSetupService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITeamMemberRepository _teamMemberRepository;

        public AdminSetupService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ITenantValidationBypass tenantValidationBypass,
            IConfiguration configuration,
            ILogger<AdminSetupService> logger,
            IUnitOfWork unitOfWork,
            ITeamMemberRepository teamMemberRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tenantValidationBypass = tenantValidationBypass;
            _configuration = configuration;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _teamMemberRepository = teamMemberRepository;
        }

        public async Task<bool> IsAdminSetupRequiredAsync()
        {
            try
            {
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
                if (!await IsAdminSetupEnabledAsync())
                {
                    _logger.LogWarning("Próba utworzenia admina gdy setup jest wyłączony");
                    return false;
                }

                if (!await IsAdminSetupRequiredAsync())
                {
                    _logger.LogWarning("Próba utworzenia admina gdy już istnieje");
                    return false;
                }

                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Użytkownik z emailem {Email} już istnieje", email);
                    return false;
                }

                // CRITICAL: Disable tenant validation for admin setup operations
                // This is necessary because admin setup happens before any tenant context exists
                _tenantValidationBypass.SkipTenantValidation();

                try
                {
                    // Step 1: Create the admin user (TenantId will be assigned after Provider creation)
                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        UserName = email,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        TenantId = null,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createResult = await _userManager.CreateAsync(user, password);
                    if (!createResult.Succeeded)
                    {
                        _logger.LogError("Błąd podczas tworzenia użytkownika admina: {Errors}",
                            string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        return false;
                    }

                    // Step 2: Assign PlatformAdmin role
                    await _userManager.AddToRoleAsync(user, UserRole.PlatformAdmin.ToString());

                    // Step 3: Create a dedicated Provider (tenant) for PlatformAdmin
                    // PlatformAdmin has their own isolated tenant - they see ONLY their own clients,
                    // NOT clients belonging to other Providers. Tenant isolation is enforced automatically
                    // by ClientRepository.ApplyTenantFilter() via TenantId.
                    var provider = Provider.Create(user.Id, "Platform Admin", "admin");

                    // Step 4: Assign TenantId to user and persist
                    user.TenantId = provider.TenantId;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        await _userManager.DeleteAsync(user);
                        _logger.LogError("Błąd podczas aktualizacji TenantId admina: {Errors}",
                            string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                        return false;
                    }

                    // Step 5: Save Provider to DB
                    await _unitOfWork.Providers.AddAsync(provider);
                    var saveProviderResult = await _unitOfWork.SaveChangesAsync();
                    if (!saveProviderResult.IsSuccess)
                    {
                        await _userManager.DeleteAsync(user);
                        _logger.LogError("Błąd podczas zapisu Provider dla admina");
                        return false;
                    }

                    // Step 6: Create TeamMember (Owner) so ProviderTeamAccessHandler grants access
                    var teamMember = new TeamMember(
                        provider.TenantId,
                        user.Id,
                        TeamMemberRole.Owner,
                        email,
                        firstName,
                        lastName);

                    await _teamMemberRepository.AddAsync(teamMember);

                    _logger.LogInformation(
                        "Początkowy administrator został utworzony: {Email} (TenantId: {TenantId})",
                        email, provider.TenantId.Value);

                    return true;
                }
                finally
                {
                    // Re-enable tenant validation after admin setup operations
                    _tenantValidationBypass.ResetTenantValidation();
                }
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
                var setupEnabled = _configuration.GetValue<bool>("AdminSetup:Enabled", false);
                var environment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT", "Production");

                if (environment == "Development")
                {
                    return Task.FromResult(true);
                }

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
