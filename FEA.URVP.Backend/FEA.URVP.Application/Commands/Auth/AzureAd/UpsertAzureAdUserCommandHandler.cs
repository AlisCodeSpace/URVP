using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Auth.AzureAd;

public sealed class UpsertAzureAdUserCommandHandler
    : BaseCommandHandler<UpsertAzureAdUserCommand, User>
{
    private readonly IUserRepository _users;
    private readonly IConfiguration _configuration;

    public UpsertAzureAdUserCommandHandler(
        ILogger<UpsertAzureAdUserCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IUserRepository users,
        IConfiguration configuration)
        : base(logger, unitOfWork)
    {
        _users = users;
        _configuration = configuration;
    }

    protected override async Task<User> HandleInternal(
        UpsertAzureAdUserCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.ToLowerInvariant().Trim();
        var userName = request.UserName.Trim();
        var affiliation = request.Affiliation.Trim();

        var adminEmails = _configuration.GetSection("AdminEmails").Get<string[]>() ?? [];
        var isConfiguredAdmin = adminEmails.Any(e =>
            string.Equals(e.Trim(), normalizedEmail, StringComparison.OrdinalIgnoreCase));

        var user = await _users.FindByEmailAsync(normalizedEmail, cancellationToken);
        var isStoredAdmin = user?.Role == UserRole.Admin;

        // TokenValidated role: explicit override (dev) > AdminEmails / stored Admin >
        // AD groups (Student / Faculty). When groups cannot be resolved, keep the stored
        // role (or Faculty for a first-time user) instead of guessing.
        UserRole? resolvedRole = request.RoleOverride;
        if (!resolvedRole.HasValue && (isConfiguredAdmin || isStoredAdmin))
        {
            resolvedRole = UserRole.Admin;
        }

        resolvedRole ??= request.DirectoryGroupRole;
        var userRole = resolvedRole ?? UserRole.Faculty;

        if (user is null)
        {
            user = new User
            {
                Email = normalizedEmail,
                Name = request.Name,
                UserName = userName,
                Affiliation = affiliation,
                ProfileImageUrl = request.ProfileImageUrl,
                Role = userRole,
                RegisteredAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _users.Add(user);
            await UnitOfWork.SaveChangesAsync(cancellationToken);

            Logger.LogInformation(
                "Created user from Azure AD: {UserId} ({Email}), Role: {Role}",
                user.Id,
                normalizedEmail,
                user.Role);

            return user;
        }

        var modified = false;

        if (resolvedRole.HasValue && user.Role != resolvedRole.Value)
        {
            Logger.LogInformation(
                "Updating role for {Email} from {OldRole} to {NewRole}",
                normalizedEmail,
                user.Role,
                resolvedRole.Value);
            user.Role = resolvedRole.Value;
            modified = true;
        }

        if (user.Name != request.Name)
        {
            user.Name = request.Name;
            modified = true;
        }

        if (user.UserName != userName)
        {
            user.UserName = userName;
            modified = true;
        }

        if (user.Affiliation != affiliation)
        {
            user.Affiliation = affiliation;
            modified = true;
        }

        if (request.ProfileImageUrl is not null && user.ProfileImageUrl != request.ProfileImageUrl)
        {
            user.ProfileImageUrl = request.ProfileImageUrl;
            modified = true;
        }

        if (modified)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await UnitOfWork.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Updated user from Azure AD: {UserId}", user.Id);
        }

        return user;
    }
}
