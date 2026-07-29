using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Users;

namespace RICHConnect.Backend.Application.Commands.Auth.AzureAd
{
    /// <summary>
    /// Handler for UpsertAzureAdUserCommand. Creates or updates a user from Azure AD authentication.
    /// </summary>
    public class UpsertAzureAdUserCommandHandler : BaseCommandHandler<UpsertAzureAdUserCommand, User>
    {
        private readonly IEventBus _eventBus;
        private readonly IConfiguration _configuration;
        
        public UpsertAzureAdUserCommandHandler(
            ILogger<UpsertAzureAdUserCommandHandler> logger, 
            AppDbContext context,
            IEventBus eventBus,
            IConfiguration configuration)
            : base(logger, context)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        
        /// <summary>
        /// Implementation of the command handling logic
        /// </summary>
        protected override async Task<User> HandleInternal(UpsertAzureAdUserCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing UpsertAzureAdUserCommand for email: {Email}", request.Email);
            
            // Normalize email to lowercase for consistent lookup
            var normalizedEmail = request.Email.ToLower().Trim();
            
            // Determine role with priority: RoleOverride > AdminEmails > FacultySpecialist
            UserRole userRole;
            
            if (request.RoleOverride.HasValue)
            {
                userRole = request.RoleOverride.Value;
                _logger.LogInformation("Using role override for {Email}: {Role}", normalizedEmail, userRole);
            }
            else
            {
                // Check if email is in AdminEmails configuration
                var adminEmails = _configuration.GetSection("AdminEmails").Get<string[]>() ?? Array.Empty<string>();
                var isAdminEmail = adminEmails.Any(email => email.ToLower().Trim() == normalizedEmail);
                
                userRole = isAdminEmail ? UserRole.Admin : UserRole.FacultySpecialist;
                
                if (isAdminEmail)
                {
                    _logger.LogInformation("Email {Email} is in AdminEmails list, assigning Admin role", normalizedEmail);
                }
            }
            
            // Try to find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
            
            if (user == null)
            {
                _logger.LogInformation("Creating new user from Azure AD - Email: {Email}, Name: {Name}, Role: {Role}", 
                    normalizedEmail, request.Name, userRole);
                
                // Create new user
                user = new User
                {
                    Email = normalizedEmail,
                    Name = request.Name,
                    ProfileImageUrl = request.ProfileImageUrl,
                    Role = userRole,
                    RegisteredAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Successfully created new user from Azure AD: {UserId}", user.Id);
                
                // Generate correlation ID for tracking this event
                var correlationId = Guid.NewGuid().ToString("N");
                
                // Publish UserRegisteredEvent
                await _eventBus.PublishAsync(new UserRegisteredEvent(
                    user.Id,
                    normalizedEmail,
                    user.Name,
                    user.Role,
                    "AzureAd",
                    user.ProfileImageUrl,
                    correlationId
                ));
                
                _logger.LogInformation("Published UserRegisteredEvent for user {UserId} with correlation ID {CorrelationId}", 
                    user.Id, correlationId);
            }
            else
            {
                _logger.LogInformation("Found existing user: {UserId}", user.Id);
                
                // Update the user's information if needed
                bool modified = false;
                
                // Update role if it differs from determined role
                if (user.Role != userRole)
                {
                    _logger.LogInformation("Updating existing user {UserId} role from {OldRole} to {NewRole}", user.Id, user.Role, userRole);
                    user.Role = userRole;
                    modified = true;
                }
                
                if (user.Name != request.Name)
                {
                    user.Name = request.Name;
                    modified = true;
                }
                
                if (request.ProfileImageUrl != null && user.ProfileImageUrl != request.ProfileImageUrl)
                {
                    user.ProfileImageUrl = request.ProfileImageUrl;
                    modified = true;
                }
                
                if (modified)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Updated existing user: {UserId}", user.Id);
                    
                    // Generate correlation ID for tracking this event
                    var correlationId = Guid.NewGuid().ToString("N");
                    
                    // Publish UserAuthenticatedEvent for returning user with updated profile
                    await _eventBus.PublishAsync(new UserAuthenticatedEvent(
                        user.Id,
                        normalizedEmail,
                        "AzureAd",
                        "OIDC",
                        false, // Not a new session, just an update
                        correlationId
                    ));
                    
                    _logger.LogInformation("Published UserAuthenticatedEvent for updated user {UserId} with correlation ID {CorrelationId}", 
                        user.Id, correlationId);
                }
            }
            
            return user;
        }
    }
}
