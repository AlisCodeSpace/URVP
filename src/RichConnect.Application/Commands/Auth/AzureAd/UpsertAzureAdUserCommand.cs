using MediatR;
using RICHConnect.Backend.Domain.Entities.Users;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Auth.AzureAd
{
    /// <summary>
    /// Command to create or update a user from Azure AD authentication.
    /// </summary>
    public class UpsertAzureAdUserCommand : IRequest<User>
    {
        /// <summary>
        /// Email address of the user (required)
        /// </summary>
        public string Email { get; }
        
        /// <summary>
        /// Display name of the user (required)
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Optional URL to the user's profile image
        /// </summary>
        public string? ProfileImageUrl { get; }
        
        /// <summary>
        /// Optional role override from FMIS/special allowed list
        /// </summary>
        public UserRole? RoleOverride { get; }
        
        public UpsertAzureAdUserCommand(string email, string name, string? profileImageUrl = null, UserRole? roleOverride = null)
        {
            // Validation is handled by ValidationBehavior and UpsertAzureAdUserCommandValidator
            Email = email;
            Name = name;
            ProfileImageUrl = profileImageUrl;
            RoleOverride = roleOverride;
        }
    }
}
