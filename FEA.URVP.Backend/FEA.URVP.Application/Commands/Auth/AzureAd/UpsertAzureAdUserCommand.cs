using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Commands.Auth.AzureAd;

/// <summary>
/// Creates or updates a user from Azure AD (AUB SSO) authentication.
/// </summary>
public sealed class UpsertAzureAdUserCommand : IRequest<User>
{
    public string Email { get; }
    public string Name { get; }
    public string UserName { get; }
    public string Affiliation { get; }
    public string? ProfileImageUrl { get; }
    public UserRole? RoleOverride { get; }

    public UpsertAzureAdUserCommand(
        string email,
        string name,
        string userName,
        string affiliation,
        string? profileImageUrl = null,
        UserRole? roleOverride = null)
    {
        Email = email;
        Name = name;
        UserName = userName;
        Affiliation = affiliation;
        ProfileImageUrl = profileImageUrl;
        RoleOverride = roleOverride;
    }
}
