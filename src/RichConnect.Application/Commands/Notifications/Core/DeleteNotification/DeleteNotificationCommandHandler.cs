using MediatR;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Data;

namespace RICHConnect.Backend.Application.Commands.Notifications.DeleteNotification;

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, bool>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly NotificationBusinessRulesService _businessRulesService;
    private readonly IEventBus _eventBus;
    private readonly ILogger<DeleteNotificationCommandHandler> _logger;
    private readonly AppDbContext _context;

    public DeleteNotificationCommandHandler(
        INotificationRepository notificationRepository,
        NotificationBusinessRulesService businessRulesService,
        IEventBus eventBus,
        AppDbContext context,
        ILogger<DeleteNotificationCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _businessRulesService = businessRulesService;
        _eventBus = eventBus;
        _context = context;
        _logger = logger;
    }

        public async Task<bool> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting notification {NotificationId} for user {UserId}", 
                request.NotificationId, request.UserId);

            // Validation is handled by ValidationBehavior and DeleteNotificationCommandValidator

            // Start a database transaction
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                // Delete notification - enforces ownership check
                var deleted = await _notificationRepository.DeleteAsync(request.NotificationId, request.UserId);
                
                if (!deleted)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogWarning("Notification {NotificationId} not found for user {UserId} or access denied", 
                        request.NotificationId, request.UserId);
                    return false;
                }
                
                // Publish domain event
                var domainEvent = new NotificationDeletedEvent(
                    request.NotificationId,
                    request.UserId);
                
                await _eventBus.PublishAsync(domainEvent);
                
                // Commit the transaction
                await transaction.CommitAsync(cancellationToken);
                
                _logger.LogInformation("Successfully deleted notification {NotificationId} for user {UserId}", 
                    request.NotificationId, request.UserId);

                return true;
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of any error
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error deleting notification {NotificationId} for user {UserId}", 
                    request.NotificationId, request.UserId);
                throw;
            }
        }
}

