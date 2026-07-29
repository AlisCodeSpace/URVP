using FluentValidation;
using RICHConnect.Backend.Application.DTOs.Matching;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Validators.Challenges
{
    /// <summary>
    /// Validator for responding to challenge invites
    /// </summary>
    public class RespondToInviteDtoValidator : AbstractValidator<RespondToInviteDto>
    {
        public RespondToInviteDtoValidator()
        {
            RuleFor(x => x.Decision)
                .IsInEnum().WithMessage("Decision must be a valid invite status")
                .NotEqual(InviteStatus.Pending).WithMessage("Decision cannot be pending - you must accept or reject the invite");
        }
    }
}
