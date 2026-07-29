using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;

public class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required")
            .MaximumLength(1000)
            .WithMessage("Message cannot exceed 1000 characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Type must be a valid NotificationType");

        RuleFor(x => x.Link)
            .MaximumLength(500)
            .WithMessage("Link cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Link));

        RuleFor(x => x.Priority)
            .Must(BeValidPriority)
            .WithMessage("Priority must be 'low', 'medium', or 'high'")
            .When(x => !string.IsNullOrEmpty(x.Priority));
            
        RuleFor(x => x.ReferenceType)
            .MaximumLength(50)
            .WithMessage("ReferenceType cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ReferenceType));
            
        RuleFor(x => x.ReferenceType)
            .NotEmpty()
            .WithMessage("ReferenceType is required when ReferenceId is provided")
            .When(x => x.ReferenceId.HasValue);
            
        RuleFor(x => x.ReferenceId)
            .NotEmpty()
            .WithMessage("ReferenceId is required when ReferenceType is provided")
            .When(x => !string.IsNullOrEmpty(x.ReferenceType));
    }

    private static bool BeValidPriority(string? priority)
    {
        if (string.IsNullOrEmpty(priority))
            return true;
            
        return priority.ToLower() switch
        {
            "low" or "medium" or "high" => true,
            _ => false
        };
    }
}

