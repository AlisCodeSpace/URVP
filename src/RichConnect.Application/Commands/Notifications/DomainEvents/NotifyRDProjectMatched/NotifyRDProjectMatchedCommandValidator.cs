using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectMatched
{
    public class NotifyRDProjectMatchedCommandValidator : AbstractValidator<NotifyRDProjectMatchedCommand>
    {
        public NotifyRDProjectMatchedCommandValidator()
        {
            RuleFor(x => x.RDProjectId)
                .NotEmpty().WithMessage("R&D project ID is required");

            RuleFor(x => x.SubmittedByUserId)
                .NotEmpty().WithMessage("Submitted by user ID is required");

            RuleFor(x => x.ProjectTitle)
                .NotEmpty().WithMessage("Project title is required");

            RuleFor(x => x.TotalMatchesCreated)
                .GreaterThan(0).WithMessage("Total matches created must be greater than 0");
        }
    }
}
