using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.DTOs.Challenge;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Entities.Challenges;

namespace RICHConnect.Backend.Application.Commands.UpdateChallenge
{
    public class UpdateChallengeCommandHandler : BaseCommandHandler<UpdateChallengeCommand, ChallengeDto>
    {
        private readonly IChallengeRepository _repository;
        private readonly IFileReadService _fileReadService;
        private readonly IEventBus _eventBus;

        public UpdateChallengeCommandHandler(
            IChallengeRepository repository,
            IFileReadService fileReadService,
            IEventBus eventBus,
            ILogger<UpdateChallengeCommandHandler> logger,
            AppDbContext context) : base(logger, context)
        {
            _repository = repository;
            _fileReadService = fileReadService;
            _eventBus = eventBus;
        }
        
        // Enable transaction support for challenge updates
        protected override bool UseTransaction => true;

        protected override async Task<ChallengeDto> HandleInternal(UpdateChallengeCommand command, CancellationToken cancellationToken)
        {
            // Validation is handled by ValidationBehavior and UpdateChallengeCommandValidator
            var challenge = await _repository.GetByIdAsync(command.ChallengeId);
            if (challenge == null)
            {
                throw new InvalidOperationException($"Challenge with ID {command.ChallengeId} not found.");
            }

            // Track changes for event
            var changedFields = new List<string>();
            if (challenge.Title != command.Title.Trim()) changedFields.Add("Title");
            if (challenge.Description != command.Description?.Trim()) changedFields.Add("Description");
            if (challenge.ResearchFieldId != command.ResearchFieldId) changedFields.Add("ResearchFieldId");
            if (challenge.EstimatedCost != command.EstimatedCost) changedFields.Add("EstimatedCost");
            // Note: SupportingDocumentUrl is obsolete - file is stored in FileStorage table
            // The command.SupportingDocumentUrl should be a file ID (Guid as string) from FileStorage

            // Update challenge
            challenge.Title = command.Title.Trim();
            challenge.Description = command.Description?.Trim();
            challenge.ResearchFieldId = command.ResearchFieldId;
            challenge.EstimatedCost = command.EstimatedCost;
            // Don't update obsolete SupportingDocumentUrl - file is managed via FileStorage
            challenge.UpdatedAt = DateTime.UtcNow;

            var updatedChallenge = await _repository.UpdateAsync(challenge);
            
            // Get user details for event
            var updatedByUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == command.UpdatedBy, cancellationToken);
            
            // Publish domain event
            await _eventBus.PublishAsync(new ChallengeUpdatedEvent(
                updatedChallenge.Id,
                updatedChallenge.Title,
                command.UpdatedBy,
                updatedByUser?.Name ?? "Unknown User",
                changedFields,
                "Challenge updated by user"
            ));
            
            return await MapToDtoAsync(updatedChallenge);
        }

        private async Task<ChallengeDto> MapToDtoAsync(Challenge challenge)
        {
            // Get file ID from FileStorage
            var fileId = await _fileReadService.GetFileIdByEntityAsync("Challenge", challenge.Id, "SupportingDocument");
            var supportingDocumentUrl = fileId?.ToString();

            // Ensure ResearchField is loaded
            if (challenge.ResearchField == null)
            {
                challenge.ResearchField = (await _context.ResearchFields
                    .AsNoTracking()
                    .FirstOrDefaultAsync(rf => rf.Id == challenge.ResearchFieldId))!;
            }

            return new ChallengeDto
            {
                Id = challenge.Id,
                Title = challenge.Title,
                Description = challenge.Description,
                ResearchFieldId = challenge.ResearchFieldId,
                ResearchFieldName = challenge.ResearchField?.Name, // Include research field name (even if pending)
                EstimatedCost = challenge.EstimatedCost,
                SupportingDocumentUrl = supportingDocumentUrl,
                SubmittedBy = challenge.SubmittedBy,
                Status = challenge.Status,
                MatchingStatus = challenge.MatchingStatus,
                ApprovedBy = challenge.ApprovedBy,
                MatchedFacultySpecialistIds = challenge.MatchedFacultySpecialists?.Select(mp => mp.FacultySpecialistUserId).ToList() ?? new List<Guid>(),
                CreatedAt = challenge.CreatedAt,
                UpdatedAt = challenge.UpdatedAt,
                RejectionReason = challenge.RejectionReason
            };
        }
    }
}
