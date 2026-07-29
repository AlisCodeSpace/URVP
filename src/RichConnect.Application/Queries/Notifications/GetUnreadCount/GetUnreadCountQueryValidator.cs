using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.Notifications.GetUnreadCount;

public class GetUnreadCountQueryValidator : AbstractValidator<GetUnreadCountQuery>
{
    public GetUnreadCountQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}

