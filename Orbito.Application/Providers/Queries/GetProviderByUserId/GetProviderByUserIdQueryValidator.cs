using FluentValidation;

namespace Orbito.Application.Providers.Queries.GetProviderByUserId
{
    public class GetProviderByUserIdQueryValidator : AbstractValidator<GetProviderByUserIdQuery>
    {
        public GetProviderByUserIdQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required");
        }
    }
}
