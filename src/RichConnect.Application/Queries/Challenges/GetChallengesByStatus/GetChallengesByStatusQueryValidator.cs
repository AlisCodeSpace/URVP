using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.GetChallengesByStatus
{
    public class GetChallengesByStatusQueryValidator : AbstractValidator<GetChallengesByStatusQuery>
    {
        public GetChallengesByStatusQueryValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid challenge status");
        }
    }
}
