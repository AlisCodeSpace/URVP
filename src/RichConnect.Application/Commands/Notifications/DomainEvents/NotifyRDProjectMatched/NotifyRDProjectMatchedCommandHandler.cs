using Microsoft.EntityFrameworkCore;
using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectMatched
{
    public class NotifyRDProjectMatchedCommandHandler : BaseCommandHandler<NotifyRDProjectMatchedCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;
        private new readonly AppDbContext _context;

        public NotifyRDProjectMatchedCommandHandler(
            IUserRepository userRepository,
            IMediator mediator,
            ILogger<NotifyRDProjectMatchedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _userRepository = userRepository;
            _mediator = mediator;
            _context = context;
        }

        protected override async Task HandleInternal(NotifyRDProjectMatchedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyRDProjectMatchedCommand for R&D project {ProjectId} with {ProfessorCount} professors", 
                request.RDProjectId, request.TotalMatchesCreated);

            try
            {
                // Get all admin users
                var adminUsers = await _context.Users
                    .Where(u => u.Role == UserRole.Admin)
                    .Select(u => u.Id)
                    .ToListAsync(cancellationToken);

                var FacultySpecialistNames = string.Join(", ", request.MatchedFacultySpecialistNames);

                // Send in-app notifications to admins
                foreach (var adminId in adminUsers)
                {
                    var adminCommand = new CreateNotificationCommand
                    {
                        UserId = adminId,
                        Title = NotificationMessages.RDProject.MatchedTitle(),
                        Message = NotificationMessages.RDProject.MatchedMessageAdmin(request.ProjectTitle, request.TotalMatchesCreated, FacultySpecialistNames),
                        Type = NotificationType.RDProjectMatched,
                        Link = $"/rd-projects/{request.RDProjectId}",
                        Priority = "medium"
                    };

                    await _mediator.Send(adminCommand, cancellationToken);
                }

                // Send in-app notification to partner
                var partnerCommand = new CreateNotificationCommand
                {
                    UserId = request.SubmittedByUserId,
                    Title = NotificationMessages.RDProject.MatchedTitle(),
                    Message = NotificationMessages.RDProject.MatchedMessagePartner(request.ProjectTitle, request.TotalMatchesCreated, FacultySpecialistNames),
                    Type = NotificationType.RDProjectMatched,
                    Link = $"/rd-projects/{request.RDProjectId}",
                    Priority = "high"
                };

                await _mediator.Send(partnerCommand, cancellationToken);

                _logger.LogInformation("Successfully processed NotifyRDProjectMatchedCommand for R&D project {ProjectId}. " +
                    "Notified {AdminCount} admins and partner", 
                    request.RDProjectId, adminUsers.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling NotifyRDProjectMatchedCommand for R&D project {ProjectId}", 
                    request.RDProjectId);
                throw;
            }
        }
    }
}
