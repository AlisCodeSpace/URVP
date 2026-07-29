using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.Interfaces.Notifications;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Handlers
{
    /// <summary>
    /// Event handler for ChallengeMatchedEvent
    /// </summary>
    public class ChallengeMatchedEventHandler : IEventHandler<ChallengeMatchedEvent>
    {
        private readonly INotificationApplicationService _notificationService;
        private readonly AppDbContext _context;
        private readonly ILogger<ChallengeMatchedEventHandler> _logger;

        public ChallengeMatchedEventHandler(
            INotificationApplicationService notificationService,
            AppDbContext context,
            ILogger<ChallengeMatchedEventHandler> logger)
        {
            _notificationService = notificationService;
            _context = context;
            _logger = logger;
        }

        public async Task HandleAsync(ChallengeMatchedEvent domainEvent)
        {
            _logger.LogInformation("Handling ChallengeMatchedEvent for challenge {ChallengeId} with {ProfessorCount} professors", 
                domainEvent.ChallengeId, domainEvent.TotalMatchesCreated);

            try
            {
                // Get all admin users
                var adminUsers = await _context.Users
                    .Where(u => u.Role == UserRole.Admin)
                    .Select(u => u.Id)
                    .ToListAsync();

                var FacultySpecialistNames = string.Join(", ", domainEvent.MatchedFacultySpecialistNames);

                // Send in-app notifications to admins
                foreach (var adminId in adminUsers)
                {
                    var adminRequest = new CreateNotificationRequest
                    {
                        UserId = adminId,
                        Title = NotificationMessages.Challenge.MatchedTitle(),
                        Message = NotificationMessages.Challenge.MatchedMessageAdmin(domainEvent.ChallengeTitle, domainEvent.TotalMatchesCreated, FacultySpecialistNames),
                        Type = NotificationType.ChallengeMatched,
                        Link = $"/challenges/{domainEvent.ChallengeId}",
                        Priority = "medium"
                    };

                    await _notificationService.CreateNotificationAsync(adminRequest);
                }

                // Send in-app notification to partner
                var partnerRequest = new CreateNotificationRequest
                {
                    UserId = domainEvent.SubmittedByUserId,
                    Title = NotificationMessages.Challenge.MatchedTitle(),
                    Message = NotificationMessages.Challenge.MatchedMessagePartner(domainEvent.ChallengeTitle, domainEvent.TotalMatchesCreated, FacultySpecialistNames),
                    Type = NotificationType.ChallengeMatched,
                    Link = $"/challenges/{domainEvent.ChallengeId}",
                    Priority = "high"
                };

                await _notificationService.CreateNotificationAsync(partnerRequest);

                // NOTE: Email sending is now handled by the NotificationCreatedEventHandler
                // which queues the email in the NotificationOutbox for reliable delivery.
                // The direct email sending has been removed to prevent duplicate emails.

                _logger.LogInformation("Successfully processed ChallengeMatchedEvent for challenge {ChallengeId}. " +
                    "Notified {AdminCount} admins and partner {PartnerName}", 
                    domainEvent.ChallengeId, adminUsers.Count, domainEvent.SubmittedByName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ChallengeMatchedEvent for challenge {ChallengeId}", 
                    domainEvent.ChallengeId);
                throw;
            }
        }
    }
}
