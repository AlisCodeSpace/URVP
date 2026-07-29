using FluentValidation;
using System.Security.Claims;

namespace RICHConnect.Backend.Application.Queries.Auth.GetUserProfile
{
    /// <summary>
    /// Validator for GetUserProfileQuery
    /// </summary>
    public class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
    {
        public GetUserProfileQueryValidator()
        {
            RuleFor(x => x.User)
                .NotNull()
                .WithMessage("User claims principal is required.");

            RuleFor(x => x.User.Identity)
                .NotNull()
                .WithMessage("User identity is required.");

            RuleFor(x => x.User.Identity!.IsAuthenticated)
                .Must(isAuthenticated => isAuthenticated)
                .WithMessage("User must be authenticated to retrieve profile.");

            // Validate that user has a valid user ID claim
            RuleFor(x => x.User)
                .Must(user => 
                {
                    var userIdClaim = user.FindFirst("nameid")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    return !string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out _);
                })
                .WithMessage("User must have a valid user ID claim.");
        }
    }
}
