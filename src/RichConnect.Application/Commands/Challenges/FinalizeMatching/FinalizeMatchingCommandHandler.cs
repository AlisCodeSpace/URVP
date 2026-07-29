using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.DTOs.Matching;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Challenges;
using RICHConnect.Backend.Application.Services.Challenges;

namespace RICHConnect.Backend.Application.Commands.FinalizeMatching
{
    /// <summary>
    /// Handler for FinalizeMatchingCommand
    /// </summary>
    public class FinalizeMatchingCommandHandler : BaseCommandHandler<FinalizeMatchingCommand, MatchFinalizeDto>
    {
        private readonly IChallengeRepository _repository;
        private readonly IEventBus _eventBus;
        private readonly ChallengeBusinessRulesService _businessRulesService;
        private readonly List<IDomainEvent> _pendingDomainEvents = new();
        
        public FinalizeMatchingCommandHandler(
            IChallengeRepository repository,
            IEventBus eventBus,
            ChallengeBusinessRulesService businessRulesService,
            ILogger<FinalizeMatchingCommandHandler> logger,
            AppDbContext context) : base(logger, context)
        {
            _repository = repository;
            _eventBus = eventBus;
            _businessRulesService = businessRulesService;
        }
        
        // Enable transaction support for matching finalization
        protected override bool UseTransaction => true;

        protected override async Task<MatchFinalizeDto> HandleInternal(FinalizeMatchingCommand command, CancellationToken cancellationToken)
        {
            _pendingDomainEvents.Clear();
            // Validation is handled by ValidationBehavior and FinalizeMatchingCommandValidator
            var challenge = await _repository.GetByIdAsync(command.ChallengeId);
            if (challenge == null)
            {
                throw new InvalidOperationException($"Challenge with ID {command.ChallengeId} not found.");
            }

            // Validate challenge is ready for finalization
            var finalizationValidation = await _businessRulesService.ValidateFinalizationAsync(command.ChallengeId);
            if (!finalizationValidation.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", finalizationValidation.Errors));
            }

            // Get accepted invites
            var invites = await _repository.GetInvitesByChallengeAsync(command.ChallengeId);
            var acceptedFacultySpecialistIds = invites.Where(i => i.Status == InviteStatus.Accepted).Select(i => i.FacultySpecialistUserId).ToList();

            // Clear existing matches and create new ones
            await _repository.ClearMatchedFacultySpecialistsAsync(command.ChallengeId);

            foreach (var facultySpecialistId in acceptedFacultySpecialistIds)
            {
                var match = new ChallengeMatchedFacultySpecialist
                {
                    ChallengeId = command.ChallengeId,
                    FacultySpecialistUserId = facultySpecialistId,
                    MatchedByUserId = command.AdminId,
                    MatchedAt = DateTime.UtcNow
                };
                await _repository.AddMatchedFacultySpecialistAsync(match);
            }

            // Update challenge matching status but keep it in the approved list
            var previousStatus = challenge.Status;
            // Don't change the challenge status to Matched, keep it as Approved
            // challenge.Status = ChallengeStatus.Matched;
            challenge.MatchingStatus = ChallengeMatchingStatus.Complete;
            challenge.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(challenge);
            
            // Publish status change event
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == command.AdminId, cancellationToken);
            _pendingDomainEvents.Add(new ChallengeStatusChangedEvent(
                command.ChallengeId,
                challenge.Title,
                previousStatus,
                challenge.Status, // Use current status instead of hardcoded Matched
                command.AdminId,
                adminUser?.Name ?? "Admin",
                "Challenge matched with faculty specialists"
            ));

            // Get challenge and facultySpecialist details for event
            var challengeDetails = await _context.Challenges
                .Include(c => c.ResearchField)
                .Include(c => c.UserSubmitted)
                .FirstOrDefaultAsync(c => c.Id == command.ChallengeId, cancellationToken);
            
            var facultySpecialists = await _context.Users
                .Where(u => acceptedFacultySpecialistIds.Contains(u.Id))
                .ToListAsync(cancellationToken);
            
            if (challengeDetails != null && facultySpecialists.Any())
            {
                _pendingDomainEvents.Add(new ChallengeMatchedEvent(
                    command.ChallengeId,
                    challengeDetails.Title,
                    acceptedFacultySpecialistIds,
                    facultySpecialists.Select(p => p.Name).ToList(),
                    challengeDetails.SubmittedBy,
                    challengeDetails.UserSubmitted?.Name ?? "Unknown User",
                    challengeDetails.ResearchField?.Name
                ));
            }

            return new MatchFinalizeDto
            {
                ChallengeId = command.ChallengeId,
                MatchedFacultySpecialistIds = acceptedFacultySpecialistIds,
                TotalMatchesCreated = acceptedFacultySpecialistIds.Count,
                Message = "Matching finalized successfully."
            };
        }

        public override async Task<MatchFinalizeDto> Handle(FinalizeMatchingCommand request, CancellationToken cancellationToken)
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
                        _logger.LogError(ex, "Failed to publish deferred domain event {EventType}", domainEvent.EventType);
                    }
                }
                _pendingDomainEvents.Clear();
            }
        }
    }
}
