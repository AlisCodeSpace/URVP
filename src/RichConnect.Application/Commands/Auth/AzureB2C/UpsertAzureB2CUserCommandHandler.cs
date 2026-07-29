using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Users;

namespace RICHConnect.Backend.Application.Commands.Auth.AzureB2C
{
    /// <summary>
    /// Handler for UpsertAzureB2CUserCommand. Creates or updates a user from Azure B2C authentication.
    /// </summary>
    public class UpsertAzureB2CUserCommandHandler : BaseCommandHandler<UpsertAzureB2CUserCommand, User>
    {
        private readonly IEventBus _eventBus;
        
        public UpsertAzureB2CUserCommandHandler(
            ILogger<UpsertAzureB2CUserCommandHandler> logger, 
            AppDbContext context,
            IEventBus eventBus)
            : base(logger, context)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }
        
        /// <summary>
        /// Implementation of the command handling logic
        /// </summary>
        protected override async Task<User> HandleInternal(UpsertAzureB2CUserCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing UpsertAzureB2CUserCommand for email: {Email}", request.Email);
            
            // Normalize email to lowercase for consistent lookup
            var normalizedEmail = request.Email.ToLower().Trim();
            
            // B2C users are always CommunityPartner role
            var userRole = UserRole.CommunityPartner;
            
            // Try to find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
            
            if (user == null)
            {
                _logger.LogInformation("Creating new user from Azure B2C - Email: {Email}, Name: {Name}, Role: {Role}", 
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
                
                _logger.LogInformation("Successfully created new user from Azure B2C: {UserId}", user.Id);
                
                // Generate correlation ID for tracking this event
                var correlationId = Guid.NewGuid().ToString("N");
                
                // Publish UserRegisteredEvent
                await _eventBus.PublishAsync(new UserRegisteredEvent(
                    user.Id,
                    normalizedEmail,
                    user.Name,
                    user.Role,
                    "AzureB2C",
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
                
                // Ensure B2C users maintain CommunityPartner role
                if (user.Role != UserRole.CommunityPartner)
                {
                    _logger.LogInformation("Updating existing user {UserId} role from {OldRole} to CommunityPartner", user.Id, user.Role);
                    user.Role = UserRole.CommunityPartner;
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
                        "AzureB2C",
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
