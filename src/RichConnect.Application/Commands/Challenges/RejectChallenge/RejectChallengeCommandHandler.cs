using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.DTOs.Challenge;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Challenges;

namespace RICHConnect.Backend.Application.Commands.RejectChallenge
{
    public class RejectChallengeCommandHandler : BaseCommandHandler<RejectChallengeCommand, ChallengeDto>
    {
        private readonly IChallengeRepository _repository;
        private readonly IFileReadService _fileReadService;
        private readonly IEventBus _eventBus;
        public RejectChallengeCommandHandler(
            IChallengeRepository repository,
            IFileReadService fileReadService,
            IEventBus eventBus,
            ILogger<RejectChallengeCommandHandler> logger,
            AppDbContext context) : base(logger, context)
        {
            _repository = repository;
            _fileReadService = fileReadService;
            _eventBus = eventBus;
        }

        protected override async Task<ChallengeDto> HandleInternal(RejectChallengeCommand command, CancellationToken cancellationToken)
        {
            // Validation is handled by ValidationBehavior and RejectChallengeCommandValidator
            var challenge = await _repository.GetByIdAsync(command.ChallengeId);
            if (challenge == null)
            {
                throw new InvalidOperationException($"Challenge with ID {command.ChallengeId} not found.");
            }

            // Capture previous status for accurate event logging
            var previousStatus = challenge.Status;

            challenge.Status = ChallengeStatus.Rejected;
            challenge.RejectionReason = command.RejectDto.RejectionReason.Trim();
            challenge.ApprovedBy = command.AdminId;
            challenge.UpdatedAt = DateTime.UtcNow;

            var updatedChallenge = await _repository.UpdateAsync(challenge);
            
            // Get user details for events
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == command.AdminId, cancellationToken);
            var submitterUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == updatedChallenge.SubmittedBy, cancellationToken);
            
            // Publish domain events
            await _eventBus.PublishAsync(new ChallengeStatusChangedEvent(
                updatedChallenge.Id,
                updatedChallenge.Title,
                previousStatus,
                ChallengeStatus.Rejected,
                command.AdminId,
                adminUser?.Name ?? "Admin",
                command.RejectDto.RejectionReason.Trim()
            ));
            
            await _eventBus.PublishAsync(new ChallengeRejectedEvent(
                updatedChallenge.Id,
                command.AdminId,
                updatedChallenge.Title,
                adminUser?.Name ?? "Admin",
                command.RejectDto.RejectionReason.Trim(),
                updatedChallenge.SubmittedBy,
                submitterUser?.Name ?? "Unknown User"
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
