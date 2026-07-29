using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectRejected
{
    public class NotifyRDProjectRejectedCommandValidator : AbstractValidator<NotifyRDProjectRejectedCommand>
    {
        public NotifyRDProjectRejectedCommandValidator()
        {
            RuleFor(x => x.RDProjectId)
                .NotEmpty().WithMessage("R&D project ID is required");

            RuleFor(x => x.RejectedByUserId)
                .NotEmpty().WithMessage("Rejected by user ID is required");

            RuleFor(x => x.RejectionReason)
                .NotEmpty().WithMessage("Rejection reason is required")
                .MaximumLength(1000).WithMessage("Rejection reason cannot exceed 1000 characters");
        }
    }
}
