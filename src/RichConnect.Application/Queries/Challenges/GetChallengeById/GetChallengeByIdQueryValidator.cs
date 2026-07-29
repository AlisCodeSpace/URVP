using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.GetChallengeById
{
    public class GetChallengeByIdQueryValidator : AbstractValidator<GetChallengeByIdQuery>
    {
        public GetChallengeByIdQueryValidator()
        {
            RuleFor(x => x.ChallengeId)
                .NotEmpty().WithMessage("Challenge ID is required");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.UserRole)
                .NotEmpty().WithMessage("User role is required");
        }
    }
}
