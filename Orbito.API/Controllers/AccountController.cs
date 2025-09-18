using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Orbito.Domain.Enums;
using Orbito.Domain.Identity;
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

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Rejestracja administratora platformy
        /// </summary>
        [HttpPost("register-platform-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterPlatformAdmin([FromBody] RegisterPlatformAdminRequest request)
        {
            try
            {
                // Sprawdź czy użytkownik już istnieje
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Użytkownik z tym adresem email już istnieje" });
                }

                // Utwórz nowego użytkownika
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    TenantId = null, // PlatformAdmin nie ma TenantId
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(new { 
                        message = "Błąd podczas tworzenia użytkownika", 
                        errors = result.Errors.Select(e => e.Description) 
                    });
                }

                // Dodaj rolę PlatformAdmin
                await _userManager.AddToRoleAsync(user, UserRole.PlatformAdmin.ToString());

                _logger.LogInformation("PlatformAdmin zarejestrowany: {Email}", request.Email);

                return Ok(new { 
                    message = "Administrator platformy został pomyślnie zarejestrowany",
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas rejestracji PlatformAdmin: {Email}", request.Email);
                return StatusCode(500, new { message = "Wystąpił błąd podczas rejestracji" });
            }
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

    public class RegisterPlatformAdminRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
