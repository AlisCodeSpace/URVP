using FEA.URVP.Application.Commands.Users.AssignRole;
using FluentValidation;

namespace FEA.URVP.Application.Validators.Users;

public sealed class AssignUserRoleCommandValidator : AbstractValidator<AssignUserRoleCommand>
{
    public AssignUserRoleCommandValidator()
    {
        RuleFor(x => x.Role).IsInEnum();
    }
}
