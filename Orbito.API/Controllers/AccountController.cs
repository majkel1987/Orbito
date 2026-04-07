using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Providers.Commands.RegisterProvider;
using Orbito.Domain.Enums;
using Orbito.Domain.Errors;
using Orbito.Domain.Identity;
using Orbito.Domain.Interfaces;
using Orbito.Domain.ValueObjects;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Orbito.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;
        private readonly IAdminSetupService _adminSetupService;
        private readonly IMediator _mediator;
        private readonly ITeamMemberRepository _teamMemberRepository;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AccountController> logger,
            IAdminSetupService adminSetupService,
            IMediator mediator,
            ITeamMemberRepository teamMemberRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _adminSetupService = adminSetupService;
            _mediator = mediator;
            _teamMemberRepository = teamMemberRepository;
        }

        /// <summary>
        /// Sprawdza czy setup administratora jest wymagany
        /// </summary>
        [HttpGet("admin-setup-status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAdminSetupStatus()
        {
            try
            {
                var isSetupRequired = await _adminSetupService.IsAdminSetupRequiredAsync();
                var isSetupEnabled = await _adminSetupService.IsAdminSetupEnabledAsync();

                return Ok(new
                {
                    isSetupRequired,
                    isSetupEnabled,
                    message = isSetupRequired ? "Setup administratora jest wymagany" : "Administrator już istnieje"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas sprawdzania statusu setup administratora");
                return StatusCode(500, new { message = "Wystąpił błąd podczas sprawdzania statusu" });
            }
        }

        /// <summary>
        /// Bezpieczna rejestracja początkowego administratora platformy (tylko przy pierwszym uruchomieniu)
        /// </summary>
        [HttpPost("setup-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> SetupAdmin([FromBody] SetupAdminRequest request)
        {
            try
            {
                // Sprawdź czy setup jest włączony
                var isSetupEnabled = await _adminSetupService.IsAdminSetupEnabledAsync();
                if (!isSetupEnabled)
                {
                    return BadRequest(new { message = "Setup administratora jest wyłączony" });
                }

                // Sprawdź czy setup jest wymagany
                var isSetupRequired = await _adminSetupService.IsAdminSetupRequiredAsync();
                if (!isSetupRequired)
                {
                    return Conflict(new
                    {
                        error = DomainErrors.Admin.AlreadyExists.Code,
                        message = DomainErrors.Admin.AlreadyExists.Message
                    });
                }

                // Utwórz administratora
                var success = await _adminSetupService.CreateInitialAdminAsync(
                    request.Email, 
                    request.Password, 
                    request.FirstName, 
                    request.LastName);

                if (!success)
                {
                    return BadRequest(new { message = "Nie udało się utworzyć administratora" });
                }

                _logger.LogInformation("Początkowy administrator został utworzony: {Email}", request.Email);

                return Ok(new { 
                    message = "Administrator platformy został pomyślnie utworzony",
                    email = request.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas setup administratora: {Email}", request.Email);
                return StatusCode(500, new { message = "Wystąpił błąd podczas tworzenia administratora" });
            }
        }

        /// <summary>
        /// Register a new provider
        /// </summary>
        [HttpPost("register-provider")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterProvider([FromBody] RegisterProviderRequest request)
        {
            var command = new RegisterProviderCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.BusinessName,
                request.SubdomainSlug,
                request.SelectedPlatformPlanId,
                request.Description,
                request.Avatar,
                request.CustomDomain);

            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                return BadRequest(new
                {
                    code = result.Error.Code,
                    message = result.Error.Message
                });
            }

            _logger.LogInformation("Provider registered: {Email} (ProviderId: {ProviderId})",
                request.Email, result.Value.ProviderId);

            return Ok(new
            {
                message = "Provider registered successfully",
                userId = result.Value.UserId,
                providerId = result.Value.ProviderId,
                businessName = result.Value.BusinessName,
                subdomainSlug = result.Value.SubdomainSlug
            });
        }

        /// <summary>
        /// Logowanie użytkownika
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null || !user.IsActive)
                {
                    return Unauthorized(new { message = "Nieprawidłowe dane logowania" });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized(new { message = "Nieprawidłowe dane logowania" });
                }

                // Aktualizuj ostatnie logowanie
                user.UpdateLastLogin();
                await _userManager.UpdateAsync(user);

                // Generuj token JWT
                var token = await GenerateJwtTokenAsync(user);

                _logger.LogInformation("Użytkownik zalogowany: {Email}", request.Email);

                return Ok(new
                {
                    token,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        tenantId = user.TenantId?.Value,
                        roles = await _userManager.GetRolesAsync(user)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas logowania: {Email}", request.Email);
                return StatusCode(500, new { message = "Wystąpił błąd podczas logowania" });
            }
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? UserRole.Client.ToString();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Role, role),
                new("tenant_id", user.TenantId?.Value.ToString() ?? string.Empty),
                new("user_role", role),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            if (user.TenantId != null)
            {
                var teamMember = await _teamMemberRepository.GetByUserIdForTenantAsync(user.Id, user.TenantId, CancellationToken.None);
                if (teamMember != null)
                {
                    claims.Add(new Claim("team_role", teamMember.Role.ToString()));
                    claims.Add(new Claim("team_member_id", teamMember.Id.ToString()));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class SetupAdminRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class RegisterProviderRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string SubdomainSlug { get; set; } = string.Empty;
        public Guid? SelectedPlatformPlanId { get; set; }
        public string? Description { get; set; }
        public string? Avatar { get; set; }
        public string? CustomDomain { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
