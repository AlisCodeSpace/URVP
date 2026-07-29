using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Application.DTOs.Matching;
using RICHConnect.Backend.Application.DTOs.Faculty;
using RICHConnect.Backend.Domain.Entities.Challenges;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.Interfaces.Challenges;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Application.Commands.FinalizeMatching;
using RICHConnect.Backend.Application.Interfaces.Files;
using Microsoft.EntityFrameworkCore;
using MediatR;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Services.Challenges
{
    /// <summary>
    /// Service for challenge matching operations
    /// </summary>
    public class ChallengeMatchingService : IChallengeMatchingService
    {
        private readonly IChallengeRepository _repository;
        private readonly IFileReadService _fileReadService;
        private readonly IEventBus _eventBus;
        private readonly AppDbContext _context;
        private readonly IMediator _mediator;

        public ChallengeMatchingService(
            IChallengeRepository repository,
            IFileReadService fileReadService,
            IEventBus eventBus, 
            AppDbContext context,
            IMediator mediator)
        {
            _repository = repository;
            _fileReadService = fileReadService;
            _eventBus = eventBus;
            _context = context;
            _mediator = mediator;
        }

        public async Task<List<MatchInviteDto>> InviteFacultySpecialistsAsync(Guid challengeId, List<Guid> FacultySpecialistIds)
        {
            // Validate challenge exists and is approved
            var challenge = await _repository.GetByIdAsync(challengeId);
            if (challenge == null)
                throw new ArgumentException($"Challenge with ID {challengeId} not found.");

            if (challenge.Status != ChallengeStatus.Approved)
                throw new InvalidOperationException("Invites can only be created for approved challenges.");

            // Validate faculty specialists exist and are available
            var validFacultySpecialistIds = new List<Guid>();
            var unavailableFacultySpecialists = new List<Guid>();
            
            foreach (var facultySpecialistId in FacultySpecialistIds)
            {
                if (await _repository.ValidateFacultySpecialistExistsAsync(facultySpecialistId))
                {
                    // All validated faculty specialists are considered available
                    validFacultySpecialistIds.Add(facultySpecialistId);
                }
            }

            if (validFacultySpecialistIds.Count == 0)
            {
                var message = unavailableFacultySpecialists.Count > 0 
                    ? "No available faculty specialists provided. Some faculty specialists may be unavailable." 
                    : "No valid faculty specialists provided.";
                throw new ArgumentException(message);
            }

            // Get existing invites to avoid duplicates
            var existingInvites = await _repository.GetInvitesByChallengeAsync(challengeId);
            var existingFacultySpecialistIds = existingInvites.Select(i => i.FacultySpecialistUserId).ToHashSet();
            var newFacultySpecialistIds = validFacultySpecialistIds.Where(id => !existingFacultySpecialistIds.Contains(id)).ToList();

            // Create new invites
            var newInvites = new List<ChallengeMatchInvite>();
            foreach (var facultySpecialistId in newFacultySpecialistIds)
            {
                var invite = new ChallengeMatchInvite
                {
                    ChallengeId = challengeId,
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
                // Get facultySpecialist and challenge details for event
                var facultySpecialist = await _context.Users
                    .Include(u => u.FacultySpecialist)
                    .FirstOrDefaultAsync(u => u.Id == invite.FacultySpecialistUserId);
                
                var challengeWithDetails = await _context.Challenges
                    .Include(c => c.ResearchField)
                    .Include(c => c.UserSubmitted)
                    .FirstOrDefaultAsync(c => c.Id == invite.ChallengeId);
                
                if (facultySpecialist != null && challengeWithDetails != null)
                {
                    await _eventBus.PublishAsync(new FacultySpecialistInvitedEvent(
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
            var allInvites = await _repository.GetInvitesByChallengeAsync(challengeId);
            return allInvites.Select(MapToInviteDto).ToList();
        }

        public async Task<MatchResponseDto> RespondToInviteAsync(Guid inviteId, Guid facultySpecialistId, InviteStatus decision)
        {
            var invite = await _repository.GetInviteByIdAsync(inviteId);
            if (invite == null)
                throw new ArgumentException($"Invite with ID {inviteId} not found.");

            if (invite.FacultySpecialistUserId != facultySpecialistId)
                throw new UnauthorizedAccessException("You can only respond to your own invites.");

            if (invite.Status != InviteStatus.Pending)
                throw new InvalidOperationException("This invite has already been responded to.");

            // Update invite status
            invite.Status = decision;
            invite.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateInviteAsync(invite);

            // Update challenge matching status
            var challenge = invite.Challenge;
            var allInvites = await _repository.GetInvitesByChallengeAsync(challenge.Id);
            var hasPendingInvites = allInvites.Any(i => i.Status == InviteStatus.Pending);

            challenge.MatchingStatus = hasPendingInvites ? ChallengeMatchingStatus.Pending : ChallengeMatchingStatus.AwaitingApproval;
            challenge.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(challenge);

            // Get facultySpecialist and challenge details for event
            var facultySpecialist = await _context.Users
                .Include(u => u.FacultySpecialist)
                .FirstOrDefaultAsync(u => u.Id == invite.FacultySpecialistUserId);
            
            var challengeDetails = await _context.Challenges
                .Include(c => c.ResearchField)
                .FirstOrDefaultAsync(c => c.Id == invite.ChallengeId);
            
            if (facultySpecialist != null && challengeDetails != null)
            {
                // Publish domain event
                await _eventBus.PublishAsync(new FacultySpecialistRespondedEvent(
                    invite.Id,
                    invite.ChallengeId,
                    challengeDetails.Title,
                    invite.FacultySpecialistUserId,
                    facultySpecialist.Name,
                    decision
                ));
            }

            return new MatchResponseDto
            {
                InviteId = invite.Id,
                Status = invite.Status,
                ChallengeMatchingStatus = challenge.MatchingStatus ?? ChallengeMatchingStatus.NoInvite,
                RespondedAt = invite.UpdatedAt
            };
        }

        public async Task<List<FacultySpecialistChallengeDto>> GetFacultySpecialistInvitesAsync(Guid facultySpecialistId)
        {
            var invites = await _repository.GetInvitesByFacultySpecialistAsync(facultySpecialistId);
            
            // Batch load file IDs to avoid N+1
            var challengeIds = invites.Select(i => i.Challenge.Id).ToList();
            var fileIdMap = await _fileReadService.GetFileIdsByEntitiesAsync("Challenge", challengeIds, "SupportingDocument");
            
            var result = new List<FacultySpecialistChallengeDto>();
            foreach (var invite in invites)
            {
                fileIdMap.TryGetValue(invite.Challenge.Id, out var fileId);
                result.Add(MapToFacultySpecialistChallengeDtoWithFileId(invite, fileId));
            }
            return result;
        }

        public async Task<List<FacultySpecialistChallengeDto>> GetFacultySpecialistParticipatingAsync(Guid facultySpecialistId)
        {
            var matchedFacultySpecialists = await _repository.GetMatchedFacultySpecialistsByFacultySpecialistAsync(facultySpecialistId);
            var challengeIds = matchedFacultySpecialists.Select(m => m.ChallengeId).ToList();
            
            // Batch load challenges
            var challengeTasks = challengeIds.Select(id => _repository.GetByIdWithIncludesAsync(id));
            var challengeResults = await Task.WhenAll(challengeTasks);
            var challenges = challengeResults.Where(c => c != null).ToList();
            
            // Batch load file IDs to avoid N+1
            var validChallengeIds = challenges.Select(c => c!.Id).ToList();
            var fileIdMap = await _fileReadService.GetFileIdsByEntitiesAsync("Challenge", validChallengeIds, "SupportingDocument");
            
            var result = new List<FacultySpecialistChallengeDto>();
            foreach (var match in matchedFacultySpecialists)
            {
                var challenge = challenges.FirstOrDefault(c => c!.Id == match.ChallengeId);
                if (challenge != null)
                {
                    fileIdMap.TryGetValue(challenge.Id, out var fileId);
                    var dto = MapToFacultySpecialistChallengeDtoWithFileId(challenge, fileId, true, match.MatchedAt);
                    result.Add(dto);
                }
            }

            return result;
        }

        public async Task<MatchFinalizeDto> FinalizeMatchingAsync(Guid challengeId, Guid adminId)
        {
            var command = new FinalizeMatchingCommand(challengeId, adminId);
            return await _mediator.Send(command);
        }

        public async Task<List<MatchInviteDto>> GetInvitesForChallengeAsync(Guid challengeId)
        {
            var invites = await _repository.GetInvitesByChallengeAsync(challengeId);
            return invites.Select(MapToInviteDto).ToList();
        }

        public async Task<bool> ValidateInviteExistsAsync(Guid inviteId)
        {
            var invite = await _repository.GetInviteByIdAsync(inviteId);
            return invite != null;
        }

        public async Task<bool> ValidateInviteBelongsToFacultySpecialistAsync(Guid inviteId, Guid facultySpecialistId)
        {
            var invite = await _repository.GetInviteByIdAsync(inviteId);
            return invite?.FacultySpecialistUserId == facultySpecialistId;
        }

        public async Task<bool> ValidateChallengeReadyForFinalizationAsync(Guid challengeId)
        {
            var challenge = await _repository.GetByIdAsync(challengeId);
            return challenge?.MatchingStatus == ChallengeMatchingStatus.AwaitingApproval;
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

        private async Task<FacultySpecialistChallengeDto> MapToFacultySpecialistChallengeDtoAsync(ChallengeMatchInvite invite, bool isParticipating = false, DateTime? matchedAt = null)
        {
            // Get file ID from FileStorage (single entity fallback)
            var fileId = await _fileReadService.GetFileIdByEntityAsync("Challenge", invite.Challenge.Id, "SupportingDocument");
            return MapToFacultySpecialistChallengeDtoWithFileId(invite, fileId, isParticipating, matchedAt);
        }

        private FacultySpecialistChallengeDto MapToFacultySpecialistChallengeDtoWithFileId(ChallengeMatchInvite invite, Guid? fileId, bool isParticipating = false, DateTime? matchedAt = null)
        {
            var supportingDocumentUrl = fileId?.ToString();

            return new FacultySpecialistChallengeDto
            {
                // Invite data
                InviteId = invite.Id,
                InviteStatus = invite.Status,
                InviteCreatedAt = invite.CreatedAt,
                InviteUpdatedAt = invite.UpdatedAt,
                
                // Challenge data
                ChallengeId = invite.Challenge.Id,
                Title = invite.Challenge.Title,
                Description = invite.Challenge.Description,
                ResearchFieldId = invite.Challenge.ResearchFieldId,
                ResearchFieldName = invite.Challenge.ResearchField?.Name ?? "",
                EstimatedCost = invite.Challenge.EstimatedCost,
                SupportingDocumentUrl = supportingDocumentUrl,
                SubmittedBy = invite.Challenge.SubmittedBy,
                SubmitterName = invite.Challenge.UserSubmitted?.Name ?? "",
                Status = invite.Challenge.Status,
                MatchingStatus = invite.Challenge.MatchingStatus ?? ChallengeMatchingStatus.NoInvite,
                ChallengeCreatedAt = invite.Challenge.CreatedAt,
                ChallengeUpdatedAt = invite.Challenge.UpdatedAt,
                
                // Participation data
                IsParticipating = isParticipating,
                MatchedAt = matchedAt
            };
        }

        private async Task<FacultySpecialistChallengeDto> MapToFacultySpecialistChallengeDtoAsync(Challenge challenge, bool isParticipating = false, DateTime? matchedAt = null)
        {
            // Get file ID from FileStorage (single entity fallback)
            var fileId = await _fileReadService.GetFileIdByEntityAsync("Challenge", challenge.Id, "SupportingDocument");
            return MapToFacultySpecialistChallengeDtoWithFileId(challenge, fileId, isParticipating, matchedAt);
        }

        private FacultySpecialistChallengeDto MapToFacultySpecialistChallengeDtoWithFileId(Challenge challenge, Guid? fileId, bool isParticipating = false, DateTime? matchedAt = null)
        {
            var supportingDocumentUrl = fileId?.ToString();

            return new FacultySpecialistChallengeDto
            {
                ChallengeId = challenge.Id,
                Title = challenge.Title,
                Description = challenge.Description,
                ResearchFieldId = challenge.ResearchFieldId,
                ResearchFieldName = challenge.ResearchField?.Name ?? "",
                EstimatedCost = challenge.EstimatedCost,
                SupportingDocumentUrl = supportingDocumentUrl,
                SubmittedBy = challenge.SubmittedBy,
                SubmitterName = challenge.UserSubmitted?.Name ?? "",
                Status = challenge.Status,
                MatchingStatus = challenge.MatchingStatus ?? ChallengeMatchingStatus.NoInvite,
                ChallengeCreatedAt = challenge.CreatedAt,
                ChallengeUpdatedAt = challenge.UpdatedAt,
                IsParticipating = isParticipating,
                MatchedAt = matchedAt
            };
        }
    }
}
