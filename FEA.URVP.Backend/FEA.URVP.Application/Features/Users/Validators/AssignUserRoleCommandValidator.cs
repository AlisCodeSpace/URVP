using FEA.URVP.Application.Features.Users.Commands.AssignRole;
using FluentValidation;

namespace FEA.URVP.Application.Features.Users.Validators;

public sealed class AssignUserRoleCommandValidator : AbstractValidator<AssignUserRoleCommand>
{
    public AssignUserRoleCommandValidator()
    {
        RuleFor(x => x.Role).IsInEnum();
    }
}
