using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orbito.Application.Common.Authorization;
using Orbito.Domain.Identity;

namespace Orbito.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = PolicyNames.ActiveProviderSubscription)]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Pobiera listę użytkowników dostępnych do powiązania z klientem
        /// </summary>
        /// <remarks>
        /// Zwraca użytkowników którzy:
        /// - Nie mają jeszcze powiązanego ClientProfile
        /// - Są aktywni (IsActive = true)
        /// </remarks>
        [HttpGet("available-for-client")]
        [Authorize(Policy = PolicyNames.ProviderTeamAccess)]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAvailableForClient()
        {
            var users = await _userManager.Users
                .Where(u => u.IsActive && u.ClientProfile == null)
                .OrderBy(u => u.Email)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email!,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    FullName = u.FullName
                })
                .ToListAsync();

            return Ok(users);
        }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
