using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orbito.Application.Common.Interfaces;
using Orbito.Application.Providers.Commands.CreateProvider;
using Orbito.Domain.Entities;
using Orbito.Domain.Enums;
using Orbito.Domain.Identity;
using Orbito.Domain.ValueObjects;

namespace Orbito.Application.Providers.Commands.RegisterProvider
{
    public class RegisterProviderCommandHandler : IRequestHandler<RegisterProviderCommand, RegisterProviderResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMediator _mediator;
        private readonly ILogger<RegisterProviderCommandHandler> _logger;

        public RegisterProviderCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMediator mediator,
            ILogger<RegisterProviderCommandHandler> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<RegisterProviderResult> Handle(RegisterProviderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Sprawdź czy użytkownik już istnieje
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return RegisterProviderResult.FailureResult(
                        "Użytkownik z tym adresem email już istnieje",
                        new List<string> { "Email już jest w użyciu" });
                }

                // Sprawdź czy subdomain już istnieje
                var existingProvider = await _mediator.Send(new CheckSubdomainAvailabilityQuery(request.SubdomainSlug), cancellationToken);
                if (!existingProvider.IsAvailable)
                {
                    return RegisterProviderResult.FailureResult(
                        "Subdomain już jest w użyciu",
                        new List<string> { "Subdomain już jest zajęty" });
                }

                // Utwórz nowego użytkownika
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    TenantId = null, // Będzie ustawiony po utworzeniu providera
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createUserResult = await _userManager.CreateAsync(user, request.Password);
                if (!createUserResult.Succeeded)
                {
                    var errors = createUserResult.Errors.Select(e => e.Description).ToList();
                    return RegisterProviderResult.FailureResult(
                        "Błąd podczas tworzenia użytkownika",
                        errors);
                }

                // NOTE: Rola Provider jest dodawana w CreateProviderCommandHandler
                // Nie dodajemy jej tutaj, aby uniknąć osobnego SaveChanges przed utworzeniem Providera

                // Utwórz providera używając istniejącej komendy
                var createProviderCommand = new CreateProviderCommand(
                    user.Id,
                    request.BusinessName,
                    request.SubdomainSlug,
                    request.Description,
                    request.Avatar,
                    request.CustomDomain);

                var createProviderResult = await _mediator.Send(createProviderCommand, cancellationToken);

                // Jeśli utworzenie providera się nie powiodło, usuń użytkownika
                if (createProviderResult.IsFailure)
                {
                    await _userManager.DeleteAsync(user);
                    return RegisterProviderResult.FailureResult(
                        createProviderResult.Error.Message,
                        new List<string> { createProviderResult.Error.Code });
                }

                // NOTE: TenantId użytkownika jest aktualizowany w CreateProviderCommandHandler
                // Nie aktualizujemy go tutaj, aby uniknąć dodatkowego SaveChanges

                _logger.LogInformation("Provider zarejestrowany: {Email} (ID: {ProviderId})",
                    request.Email, createProviderResult.Value.ProviderId);

                return RegisterProviderResult.SuccessResult(
                    user.Id,
                    createProviderResult.Value.ProviderId,
                    request.BusinessName,
                    request.SubdomainSlug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas rejestracji providera: {Email}", request.Email);
                return RegisterProviderResult.FailureResult(
                    "Wystąpił błąd podczas rejestracji providera",
                    new List<string> { ex.Message });
            }
        }
    }

    // Query do sprawdzania dostępności subdomain
    public record CheckSubdomainAvailabilityQuery(string SubdomainSlug) : IRequest<SubdomainAvailabilityResult>;

    public record SubdomainAvailabilityResult
    {
        public bool IsAvailable { get; init; }
        public string? Message { get; init; }
    }

    public class CheckSubdomainAvailabilityQueryHandler : IRequestHandler<CheckSubdomainAvailabilityQuery, SubdomainAvailabilityResult>
    {
        private readonly IProviderRepository _providerRepository;
        private readonly ILogger<CheckSubdomainAvailabilityQueryHandler> _logger;

        public CheckSubdomainAvailabilityQueryHandler(
            IProviderRepository providerRepository,
            ILogger<CheckSubdomainAvailabilityQueryHandler> logger)
        {
            _providerRepository = providerRepository;
            _logger = logger;
        }

        public async Task<SubdomainAvailabilityResult> Handle(CheckSubdomainAvailabilityQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var existingProvider = await _providerRepository.GetBySubdomainSlugAsync(request.SubdomainSlug, cancellationToken);
                
                if (existingProvider != null)
                {
                    return new SubdomainAvailabilityResult
                    {
                        IsAvailable = false,
                        Message = "Subdomain już jest w użyciu"
                    };
                }

                return new SubdomainAvailabilityResult
                {
                    IsAvailable = true,
                    Message = "Subdomain jest dostępny"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas sprawdzania dostępności subdomain: {SubdomainSlug}", request.SubdomainSlug);
                return new SubdomainAvailabilityResult
                {
                    IsAvailable = false,
                    Message = "Błąd podczas sprawdzania dostępności subdomain"
                };
            }
        }
    }
}
