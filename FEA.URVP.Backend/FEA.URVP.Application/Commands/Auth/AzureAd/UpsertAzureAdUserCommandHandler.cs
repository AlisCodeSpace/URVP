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

        UserRole userRole;
        if (request.RoleOverride.HasValue)
        {
            userRole = request.RoleOverride.Value;
        }
        else
        {
            var adminEmails = _configuration.GetSection("AdminEmails").Get<string[]>() ?? [];
            var isAdmin = adminEmails.Any(e =>
                string.Equals(e.Trim(), normalizedEmail, StringComparison.OrdinalIgnoreCase));
            userRole = isAdmin ? UserRole.Admin : UserRole.Faculty;
        }

        var user = await _users.FindByEmailAsync(normalizedEmail, cancellationToken);

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

        // RoleOverride (e.g. dev sign-in) may update role; otherwise preserve admin-assigned roles.
        if (request.RoleOverride.HasValue && user.Role != userRole)
        {
            user.Role = userRole;
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
