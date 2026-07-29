using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.GetEditRequestStatus
{
    public class GetEditRequestStatusQueryValidator : AbstractValidator<GetEditRequestStatusQuery>
    {
        public GetEditRequestStatusQueryValidator()
        {
            RuleFor(x => x.ChallengeId)
                .NotEmpty()
                .WithMessage("Challenge ID is required.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.");
        }
    }
}
