using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.GetUserChallenges
{
    public class GetUserChallengesQueryValidator : AbstractValidator<GetUserChallengesQuery>
    {
        public GetUserChallengesQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");
        }
    }
}
