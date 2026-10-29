using FEA.URVP.Application.Commands.Projects.ResearchAssistants.Add;
using FEA.URVP.Application.Commands.Projects.ResearchAssistants.Remove;
using FluentValidation;

namespace FEA.URVP.Application.Validators.Projects;

public sealed class AddProjectResearchAssistantCommandValidator
    : AbstractValidator<AddProjectResearchAssistantCommand>
{
    public AddProjectResearchAssistantCommandValidator()
    {
        RuleFor(command => command.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(command => command.CurrentUserId)
            .NotEmpty().WithMessage("CurrentUserId is required.");

        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(256).WithMessage("Email must be at most 256 characters.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}

public sealed class RemoveProjectResearchAssistantCommandValidator
    : AbstractValidator<RemoveProjectResearchAssistantCommand>
{
    public RemoveProjectResearchAssistantCommandValidator()
    {
        RuleFor(command => command.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(command => command.AssistantId)
            .NotEmpty().WithMessage("AssistantId is required.");

        RuleFor(command => command.CurrentUserId)
            .NotEmpty().WithMessage("CurrentUserId is required.");
    }
}
