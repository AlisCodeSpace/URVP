using RICHConnect.Backend.Domain.Entities.Notifications;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data;

namespace RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;

public class CreateNotificationCommandHandler : BaseCommandHandler<CreateNotificationCommand, Guid>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly NotificationBusinessRulesService _businessRulesService;
    private readonly IEventBus _eventBus;
    private new readonly AppDbContext _context;

    public CreateNotificationCommandHandler(
        INotificationRepository notificationRepository,
        NotificationBusinessRulesService businessRulesService,
        IEventBus eventBus,
        AppDbContext context,
        ILogger<CreateNotificationCommandHandler> logger)
        : base(logger, context)
    {
        _notificationRepository = notificationRepository;
        _businessRulesService = businessRulesService;
        _eventBus = eventBus;
        _context = context;
    }

        protected override async Task<Guid> HandleInternal(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating notification for user {UserId}", request.UserId);

            NotificationCreatedEvent? domainEventToPublish = null;

            // Start a database transaction
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                // Check for duplicate notification if ReferenceId is provided
                if (request.ReferenceId.HasValue)
                {
                    _logger.LogDebug("Checking for duplicate notification for user {UserId}, type {Type}, reference {ReferenceId}",
                        request.UserId, request.Type, request.ReferenceId);
                    
                    var existing = await _notificationRepository.FindByReferenceAsync(
                        request.UserId,
                        request.Type,
                        request.ReferenceId.Value);
                    
                    if (existing != null)
                    {
                        _logger.LogInformation("Duplicate notification detected for user {UserId}, reference {ReferenceId}. Returning existing notification ID {NotificationId}",
                            request.UserId, request.ReferenceId, existing.Id);
                        
                        await transaction.CommitAsync(cancellationToken);
                        return existing.Id;
                    }
                }
                
                // Note: Business rules validation is now handled by the calling service layer
                // This handler focuses on the core notification creation logic

                // Create notification entity
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Title = request.Title,
                    Message = request.Message,
                    Type = request.Type.ToString(),
                    Data = request.Link, // Store link in Data field
                    ReferenceId = request.ReferenceId,
                    ReferenceType = request.ReferenceType,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

            // Save to database
            var createdNotification = await _notificationRepository.CreateAsync(notification);
            
            // Commit the transaction
            await transaction.CommitAsync(cancellationToken);

            // IMPORTANT:
            // Publish the domain event AFTER commit to avoid lock-wait/deadlock scenarios where
            // handlers (often via a new DbContext/connection) query data that is still uncommitted.
            domainEventToPublish = new NotificationCreatedEvent(
                createdNotification.Id,
                request.UserId,
                request.Title,
                request.Message,
                request.Type,
                request.Link,
                request.Priority ?? "low");
            
            _logger.LogInformation("Successfully created notification {NotificationId} for user {UserId}", 
                createdNotification.Id, request.UserId);

            return createdNotification.Id;
        }
        catch (Exception ex)
        {
            // Rollback the transaction in case of any error
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error creating notification for user {UserId}", request.UserId);
            throw;
        }
        finally
        {
            // Publish outside the transaction lifecycle (best effort).
            if (domainEventToPublish != null)
            {
                try
                {
                    await _eventBus.PublishAsync(domainEventToPublish);
                }
                catch (Exception ex)
                {
                    // Notification is already committed; don't fail the request due to side-effect failures.
                    _logger.LogError(ex, "Failed to publish NotificationCreatedEvent for notification {NotificationId}", domainEventToPublish.NotificationId);
                }
            }
        }
    }
}

