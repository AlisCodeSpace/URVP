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
using RICHConnect.Backend.Domain.Entities.ResearchFields;
using System.Text.RegularExpressions;

namespace RICHConnect.Backend.Application.Commands.CreateChallenge
{
    public class CreateChallengeCommandHandler : BaseCommandHandler<CreateChallengeCommand, ChallengeDto>
    {
        private readonly IChallengeRepository _repository;
        private readonly IFileReadService _fileReadService;
        private readonly IEventBus _eventBus;
        public CreateChallengeCommandHandler(
            IChallengeRepository repository,
            IFileReadService fileReadService,
            IEventBus eventBus,
            ILogger<CreateChallengeCommandHandler> logger,
            AppDbContext context) : base(logger, context)
        {
            _repository = repository;
            _fileReadService = fileReadService;
            _eventBus = eventBus;
        }
        
        // Enable transaction support for challenge creation
        protected override bool UseTransaction => true;
        private ChallengeSubmittedEvent? _pendingDomainEvent;

        protected override async Task<ChallengeDto> HandleInternal(CreateChallengeCommand command, CancellationToken cancellationToken)
        {
            _pendingDomainEvent = null;
            // Validation is handled by ValidationBehavior and CreateChallengeCommandValidator

            Guid researchFieldId = command.ResearchFieldId;

            // If OtherResearchFieldName is provided, create a new research field
            if (!string.IsNullOrWhiteSpace(command.OtherResearchFieldName))
            {
                var newResearchField = new ResearchField
                {
                    Id = Guid.NewGuid(),
                    Name = command.OtherResearchFieldName.Trim(),
                    Slug = GenerateSlug(command.OtherResearchFieldName.Trim()),
                    Category = null,
                    DisplayOrder = 999, // Place at end by default
                    IsActive = false, // Not active until admin approves
                    Status = ApprovalStatus.Pending,
                    SubmittedBy = command.SubmittedBy,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ResearchFields.Add(newResearchField);
                await _context.SaveChangesAsync(cancellationToken);

                researchFieldId = newResearchField.Id;

                _logger.LogInformation(
                    "Created new research field '{ResearchFieldName}' (ID: {ResearchFieldId}) for challenge submission by user {UserId}",
                    newResearchField.Name, newResearchField.Id, command.SubmittedBy);
            }

            var challenge = new Challenge
            {
                Title = command.Title.Trim(),
                Description = command.Description?.Trim(),
                ResearchFieldId = researchFieldId,
                EstimatedCost = command.EstimatedCost,
                // Note: SupportingDocumentUrl is obsolete - file is stored in FileStorage table
                // Don't set obsolete property - file is managed via FileStorage
                SubmittedBy = command.SubmittedBy,
                Status = ChallengeStatus.Pending,
                MatchingStatus = ChallengeMatchingStatus.NoInvite,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdChallenge = await _repository.CreateAsync(challenge);
            
            // Get user details for event
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == command.SubmittedBy, cancellationToken);
            var researchField = await _context.ResearchFields.FirstOrDefaultAsync(rf => rf.Id == createdChallenge.ResearchFieldId, cancellationToken);
            
            // Queue domain event to be published after transaction commit
            _pendingDomainEvent = new ChallengeSubmittedEvent(
                createdChallenge.Id,
                command.SubmittedBy,
                createdChallenge.Title,
                user?.Name ?? "Unknown User",
                createdChallenge.ResearchFieldId,
                createdChallenge.Description,
                researchField?.Name,
                createdChallenge.EstimatedCost,
                null // SupportingDocumentUrl is obsolete - file is in FileStorage
            );
            
            return await MapToDtoAsync(createdChallenge);
        }

        public override async Task<ChallengeDto> Handle(CreateChallengeCommand request, CancellationToken cancellationToken)
        {
            _pendingDomainEvent = null;
            try
            {
                var response = await base.Handle(request, cancellationToken);
                return response;
            }
            finally
            {
                if (_pendingDomainEvent != null)
                {
                    try
                    {
                        await _eventBus.PublishAsync(_pendingDomainEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish ChallengeSubmittedEvent for challenge {ChallengeId}", _pendingDomainEvent.ChallengeId);
                    }
                    finally
                    {
                        _pendingDomainEvent = null;
                    }
                }
            }
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

        private string GenerateSlug(string name)
        {
            // Convert to lowercase
            var slug = name.ToLowerInvariant();

            // Remove invalid characters
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Replace spaces with hyphens
            slug = Regex.Replace(slug, @"\s+", "-");

            // Remove duplicate hyphens
            slug = Regex.Replace(slug, @"-+", "-");

            // Trim hyphens from start and end
            slug = slug.Trim('-');

            return slug;
        }
    }
}
