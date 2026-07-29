using MediatR;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Data;

namespace RICHConnect.Backend.Application.Commands.Notifications.MarkAsRead;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, bool>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly NotificationBusinessRulesService _businessRulesService;
    private readonly IEventBus _eventBus;
    private readonly ILogger<MarkAsReadCommandHandler> _logger;
    private readonly AppDbContext _context;

    public MarkAsReadCommandHandler(
        INotificationRepository notificationRepository,
        NotificationBusinessRulesService businessRulesService,
        IEventBus eventBus,
        AppDbContext context,
        ILogger<MarkAsReadCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _businessRulesService = businessRulesService;
        _eventBus = eventBus;
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Marking notification {NotificationId} as read for user {UserId}", 
            request.NotificationId, request.UserId);

        // Validation is handled by ValidationBehavior and MarkAsReadCommandValidator

        NotificationReadEvent? domainEventToPublish = null;

        // Start a database transaction
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Mark notification as read
            await _notificationRepository.MarkAsReadAsync(request.NotificationId, request.UserId);
            
            // Commit the transaction
            await transaction.CommitAsync(cancellationToken);

            // IMPORTANT: Publish AFTER commit to avoid lock-wait/deadlock issues in handlers that query using
            // a new DbContext/connection (e.g., unread count cache refresh).
            domainEventToPublish = new NotificationReadEvent(
                request.NotificationId,
                request.UserId,
                DateTime.UtcNow);
            
            _logger.LogInformation("Successfully marked notification {NotificationId} as read for user {UserId}", 
                request.NotificationId, request.UserId);

            return true;
        }
        catch (Exception ex)
        {
            // Rollback the transaction in case of any error
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", 
                request.NotificationId, request.UserId);
            throw;
        }
        finally
        {
            if (domainEventToPublish != null)
            {
                try
                {
                    await _eventBus.PublishAsync(domainEventToPublish);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish NotificationReadEvent for notification {NotificationId}", domainEventToPublish.NotificationId);
                }
            }
        }
    }
}

