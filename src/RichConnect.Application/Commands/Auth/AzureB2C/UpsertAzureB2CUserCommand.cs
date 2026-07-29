using MediatR;
using RICHConnect.Backend.Domain.Entities.Users;

namespace RICHConnect.Backend.Application.Commands.Auth.AzureB2C
{
    /// <summary>
    /// Command to create or update a user from Azure B2C authentication.
    /// </summary>
    public class UpsertAzureB2CUserCommand : IRequest<User>
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
        
        public UpsertAzureB2CUserCommand(string email, string name, string? profileImageUrl = null)
        {
            // Validation is handled by ValidationBehavior and UpsertAzureB2CUserCommandValidator
            Email = email;
            Name = name;
            ProfileImageUrl = profileImageUrl;
        }
    }
}
