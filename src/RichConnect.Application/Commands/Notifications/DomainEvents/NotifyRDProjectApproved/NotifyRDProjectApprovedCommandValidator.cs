using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectApproved
{
    public class NotifyRDProjectApprovedCommandValidator : AbstractValidator<NotifyRDProjectApprovedCommand>
    {
        public NotifyRDProjectApprovedCommandValidator()
        {
            RuleFor(x => x.RDProjectId)
                .NotEmpty().WithMessage("R&D project ID is required");

            RuleFor(x => x.ApprovedByUserId)
                .NotEmpty().WithMessage("Approved by user ID is required");
        }
    }
}
