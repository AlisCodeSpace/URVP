using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.DTOs.Matching;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Challenges;

namespace RICHConnect.Backend.Application.Commands.InviteFacultySpecialists
{
    public class InviteFacultySpecialistsCommandHandler : BaseCommandHandler<InviteFacultySpecialistsCommand, List<MatchInviteDto>>
    {
        private readonly IChallengeRepository _repository;
        private readonly IEventBus _eventBus;
        private readonly List<FacultySpecialistInvitedEvent> _pendingDomainEvents = new();
        public InviteFacultySpecialistsCommandHandler(
            IChallengeRepository repository,
            IEventBus eventBus,
            ILogger<InviteFacultySpecialistsCommandHandler> logger,
            AppDbContext context) : base(logger, context)
        {
            _repository = repository;
            _eventBus = eventBus;
        }
        
        // Enable transaction support for creating invites
        protected override bool UseTransaction => true;

        protected override async Task<List<MatchInviteDto>> HandleInternal(InviteFacultySpecialistsCommand command, CancellationToken cancellationToken)
        {
            _pendingDomainEvents.Clear();
            // Validation is handled by ValidationBehavior and InviteFacultySpecialistsCommandValidator
            var challenge = await _repository.GetByIdAsync(command.ChallengeId);
            if (challenge == null)
            {
                throw new InvalidOperationException($"Challenge with ID {command.ChallengeId} not found.");
            }

            // All faculty specialist IDs are validated by the validator
            var validFacultySpecialistIds = command.FacultySpecialistIds;

            // Get existing invites to avoid duplicates
            var existingInvites = await _repository.GetInvitesByChallengeAsync(command.ChallengeId);
            var existingFacultySpecialistIds = existingInvites.Select(i => i.FacultySpecialistUserId).ToHashSet();
            var newFacultySpecialistIds = validFacultySpecialistIds.Where(id => !existingFacultySpecialistIds.Contains(id)).ToList();

            // Create new invites
            var newInvites = new List<ChallengeMatchInvite>();
            foreach (var facultySpecialistId in newFacultySpecialistIds)
            {
                var invite = new ChallengeMatchInvite
                {
                    ChallengeId = command.ChallengeId,
                    FacultySpecialistUserId = facultySpecialistId,
                    Status = InviteStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                newInvites.Add(invite);
            }

            // Save new invites
            foreach (var invite in newInvites)
            {
                await _repository.CreateInviteAsync(invite);
            }

            // Update challenge matching status
            if (newInvites.Count > 0)
            {
                challenge.MatchingStatus = ChallengeMatchingStatus.Pending;
                challenge.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(challenge);
            }

            // Publish events for invited faculty specialists
            foreach (var invite in newInvites)
            {
                // Get faculty specialist and challenge details for event
                var facultySpecialist = await _context.Users
                    .Include(u => u.FacultySpecialist)
                    .FirstOrDefaultAsync(u => u.Id == invite.FacultySpecialistUserId, cancellationToken);
                
                var challengeWithDetails = await _context.Challenges
                    .Include(c => c.ResearchField)
                    .Include(c => c.UserSubmitted)
                    .FirstOrDefaultAsync(c => c.Id == invite.ChallengeId, cancellationToken);
                
                if (facultySpecialist != null && challengeWithDetails != null)
                {
                    _pendingDomainEvents.Add(new FacultySpecialistInvitedEvent(
                        invite.Id,
                        invite.ChallengeId,
                        challengeWithDetails.Title,
                        invite.FacultySpecialistUserId,
                        facultySpecialist.Name,
                        challengeWithDetails.ResearchField?.Name,
                        challengeWithDetails.UserSubmitted?.Name,
                        challengeWithDetails.Description
                    ));
                }
            }

            // Return all invites for the challenge
            var allInvites = await _repository.GetInvitesByChallengeAsync(command.ChallengeId);
            return allInvites.Select(MapToInviteDto).ToList();
        }

        public override async Task<List<MatchInviteDto>> Handle(InviteFacultySpecialistsCommand request, CancellationToken cancellationToken)
        {
            _pendingDomainEvents.Clear();
            try
            {
                var response = await base.Handle(request, cancellationToken);
                return response;
            }
            finally
            {
                foreach (var domainEvent in _pendingDomainEvents.ToList())
                {
                    try
                    {
                        await _eventBus.PublishAsync(domainEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish FacultySpecialistInvitedEvent for invite {InviteId}", domainEvent.InviteId);
                    }
                }
                _pendingDomainEvents.Clear();
            }
        }

        private MatchInviteDto MapToInviteDto(ChallengeMatchInvite invite)
        {
            return new MatchInviteDto
            {
                Id = invite.Id,
                ChallengeId = invite.ChallengeId,
                FacultySpecialistUserId = invite.FacultySpecialistUserId,
                Status = invite.Status,
                CreatedAt = invite.CreatedAt,
                UpdatedAt = invite.UpdatedAt
            };
        }
    }
}
