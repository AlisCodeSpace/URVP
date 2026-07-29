using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.Auth.GetAuthStatus
{
    /// <summary>
    /// Validator for GetAuthStatusQuery
    /// </summary>
    public class GetAuthStatusQueryValidator : AbstractValidator<GetAuthStatusQuery>
    {
        public GetAuthStatusQueryValidator()
        {
            RuleFor(x => x.User)
                .NotNull()
                .WithMessage("User claims principal is required.");

            RuleFor(x => x.User.Identity)
                .NotNull()
                .WithMessage("User identity is required.");

            // Note: We don't require authentication here because the handler
            // gracefully handles unauthenticated users by returning IsAuthenticated = false
            // This allows the endpoint to be accessible to check auth status
        }
    }
}
