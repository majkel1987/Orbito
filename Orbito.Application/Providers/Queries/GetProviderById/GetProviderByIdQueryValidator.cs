using FluentValidation;

namespace Orbito.Application.Providers.Queries.GetProviderById;

public class GetProviderByIdQueryValidator : AbstractValidator<GetProviderByIdQuery>
{
    public GetProviderByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Provider ID is required");
    }
}
