using MediatR;
using RICHConnect.Backend.Domain.Entities.Notifications;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Data;

namespace RICHConnect.Backend.Application.Commands.Notifications.UpdateNotificationSettings;

public class UpdateNotificationSettingsCommandHandler : IRequestHandler<UpdateNotificationSettingsCommand, bool>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<UpdateNotificationSettingsCommandHandler> _logger;
    private readonly AppDbContext _context;

    public UpdateNotificationSettingsCommandHandler(
        INotificationRepository notificationRepository,
        IEventBus eventBus,
        AppDbContext context,
        ILogger<UpdateNotificationSettingsCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _eventBus = eventBus;
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateNotificationSettingsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating notification settings for user {UserId}", request.UserId);

        // Create or update user notification settings
        var settings = new UserNotificationSettings
        {
            UserId = request.UserId,
            EmailNotifications = request.EmailNotifications,
            InAppNotifications = request.InAppNotifications,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Start a database transaction
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            await _notificationRepository.UpdateUserSettingsAsync(settings);
            
            // Publish domain event
            var domainEvent = new NotificationSettingsUpdatedEvent(
                request.UserId,
                request.EmailNotifications,
                request.InAppNotifications);
            
            await _eventBus.PublishAsync(domainEvent);
            
            // Commit the transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Successfully updated notification settings for user {UserId}", request.UserId);

            return true;
        }
        catch (Exception ex)
        {
            // Rollback the transaction in case of any error
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error updating notification settings for user {UserId}", request.UserId);
            throw;
        }
    }
}

