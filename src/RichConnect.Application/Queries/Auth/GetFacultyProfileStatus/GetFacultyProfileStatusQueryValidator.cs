using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.Auth.GetFacultyProfileStatus
{
    /// <summary>
    /// Validator for GetFacultyProfileStatusQuery
    /// </summary>
    public class GetFacultyProfileStatusQueryValidator : AbstractValidator<GetFacultyProfileStatusQuery>
    {
        public GetFacultyProfileStatusQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User ID is required.")
                .NotEqual(Guid.Empty)
                .WithMessage("User ID cannot be empty.");
        }
    }
}
