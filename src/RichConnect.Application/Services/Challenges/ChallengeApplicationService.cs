using RICHConnect.Backend.Application.Commands.CreateChallenge;
using RICHConnect.Backend.Application.Commands.ApproveChallenge;
using RICHConnect.Backend.Application.Commands.RejectChallenge;
using RICHConnect.Backend.Application.Commands.UpdateChallenge;
using RICHConnect.Backend.Application.Commands.RequestChallengeEdit;
using RICHConnect.Backend.Application.Commands.ApproveEditRequest;
using RICHConnect.Backend.Application.Commands.RejectEditRequest;
using RICHConnect.Backend.Application.Commands.InviteFacultySpecialists;
using RICHConnect.Backend.Application.Commands.FinalizeMatching;
using RICHConnect.Backend.Application.Queries.GetChallengeById;
using RICHConnect.Backend.Application.Queries.GetChallengesByStatus;
using RICHConnect.Backend.Application.Queries.GetUserChallenges;
using RICHConnect.Backend.Application.Queries.GetEditRequestStatus;
using RICHConnect.Backend.Application.Queries.GetEditRequestDetails;
using RICHConnect.Backend.Application.Queries.GetEditRequestByChallengeId;
using RICHConnect.Backend.Application.Interfaces;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Application.DTOs.Challenge;
using RICHConnect.Backend.Application.DTOs.Matching;
using RICHConnect.Backend.Domain.Entities.Challenges;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Events;
using MediatR;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Services.Challenges
{
    public class ChallengeApplicationService : IChallengeApplicationService
    {
        private readonly IMediator _mediator;
        private readonly IChallengeRepository _repository;
        private readonly IFileReadService _fileReadService;
        private readonly IEventBus _eventBus;
        private readonly AppDbContext _context;

        public ChallengeApplicationService(
            IMediator mediator,
            IChallengeRepository repository,
            IFileReadService fileReadService,
            IEventBus eventBus,
            AppDbContext context)
        {
            _mediator = mediator;
            _repository = repository;
            _fileReadService = fileReadService;
            _eventBus = eventBus;
            _context = context;
        }

        public async Task<ChallengeDto> CreateChallengeAsync(CreateChallengeDto dto, Guid userId)
        {
            var command = new CreateChallengeCommand(
                dto.Title,
                dto.Description,
                dto.ResearchFieldId,
                dto.OtherResearchFieldName,
                dto.EstimatedCost,
                dto.SupportingDocumentUrl,
                userId
            );

            return await _mediator.Send(command);
        }

        public async Task<ChallengeDto> ApproveChallengeAsync(Guid id, Guid adminId)
        {
            var command = new ApproveChallengeCommand(id, adminId);
            return await _mediator.Send(command);
        }

        public async Task<ChallengeDto> RejectChallengeAsync(Guid id, RejectChallengeDto dto, Guid adminId)
        {
            var command = new RejectChallengeCommand(id, adminId, dto);
            return await _mediator.Send(command);
        }

        public async Task<ChallengeDto> UpdateChallengeAsync(Guid id, UpdateChallengeDto dto, Guid userId)
        {
            var command = new UpdateChallengeCommand(
                id,
                dto.Title,
                dto.Description,
                dto.ResearchFieldId,
                dto.EstimatedCost,
                dto.SupportingDocumentUrl,
                userId
            );

            return await _mediator.Send(command);
        }

        public async Task<ChallengeEditRequestDto> RequestChallengeEditAsync(Guid challengeId, RequestChallengeEditDto dto, Guid userId)
        {
            var command = new RequestChallengeEditCommand(
                challengeId,
                dto.EditReason,
                userId
            );

            return await _mediator.Send(command);
        }

        public async Task<ChallengeEditRequestDto?> GetEditRequestStatusAsync(Guid challengeId, Guid userId)
        {
            var query = new GetEditRequestStatusQuery(challengeId, userId);
            return await _mediator.Send<ChallengeEditRequestDto?>(query);
        }

        public async Task<ChallengeEditRequestDto> ApproveEditRequestAsync(Guid editRequestId, ApproveEditRequestDto dto, Guid adminId)
        {
            var command = new ApproveEditRequestCommand(
                editRequestId,
                dto.AdminResponse,
                adminId
            );

            return await _mediator.Send(command);
        }

        public async Task<ChallengeEditRequestDto> RejectEditRequestAsync(Guid editRequestId, RejectEditRequestDto dto, Guid adminId)
        {
            var command = new RejectEditRequestCommand(
                editRequestId,
                dto.AdminResponse,
                adminId
            );

            return await _mediator.Send(command);
        }

        public async Task<ChallengeEditRequestDto?> GetEditRequestDetailsAsync(Guid editRequestId)
        {
            var query = new GetEditRequestDetailsQuery(editRequestId);
            return await _mediator.Send<ChallengeEditRequestDto?>(query);
        }

        public async Task<ChallengeEditRequestDto?> GetEditRequestByChallengeIdAsync(Guid challengeId)
        {
            var query = new GetEditRequestByChallengeIdQuery(challengeId);
            return await _mediator.Send<ChallengeEditRequestDto?>(query);
        }

        public async Task<ChallengeDto?> GetChallengeByIdAsync(Guid id, Guid userId, string userRole)
        {
            var query = new GetChallengeByIdQuery(id, userId, userRole);
            return await _mediator.Send<ChallengeDto?>(query);
        }

        public async Task<List<ChallengeDto>> GetChallengesByStatusAsync(ChallengeStatus status)
        {
            var query = new GetChallengesByStatusQuery(status);
            return await _mediator.Send<List<ChallengeDto>>(query);
        }

        public async Task<List<ChallengeWithDetailsDto>> GetChallengesByStatusWithDetailsAsync(ChallengeStatus status)
        {
            var challenges = await _repository.GetByStatusWithIncludesAsync(status);
            
            // Batch load file IDs to avoid N+1
            var challengeIds = challenges.Select(c => c.Id).ToList();
            var fileIdMap = await _fileReadService.GetFileIdsByEntitiesAsync("Challenge", challengeIds, "SupportingDocument");
            
            var result = new List<ChallengeWithDetailsDto>();
            foreach (var challenge in challenges)
            {
                fileIdMap.TryGetValue(challenge.Id, out var fileId);
                result.Add(MapToDetailsDtoWithFileId(challenge, fileId));
            }
            return result;
        }
        
        public async Task<List<ChallengeWithDetailsDto>> GetApprovedChallengesForMatchingAsync()
        {
            var challenges = await _repository.GetApprovedChallengesForMatchingAsync();
            
            // Batch load file IDs to avoid N+1
            var challengeIds = challenges.Select(c => c.Id).ToList();
            var fileIdMap = await _fileReadService.GetFileIdsByEntitiesAsync("Challenge", challengeIds, "SupportingDocument");
            
            var result = new List<ChallengeWithDetailsDto>();
            foreach (var challenge in challenges)
            {
                fileIdMap.TryGetValue(challenge.Id, out var fileId);
                result.Add(MapToDetailsDtoWithFileId(challenge, fileId));
            }
            return result;
        }

        public async Task<List<ChallengeDto>> GetUserChallengesAsync(Guid userId)
        {
            var query = new GetUserChallengesQuery(userId);
            return await _mediator.Send<List<ChallengeDto>>(query);
        }

        public async Task<bool> ValidateResearchFieldExistsAsync(Guid researchFieldId)
        {
            return await _repository.ValidateResearchFieldExistsAsync(researchFieldId);
        }

        public async Task<bool> ValidateChallengeExistsAsync(Guid challengeId)
        {
            return await _repository.ExistsAsync(challengeId);
        }

        public async Task<bool> ValidateChallengeStatusAsync(Guid challengeId, ChallengeStatus expectedStatus)
        {
            var challenge = await _repository.GetByIdAsync(challengeId);
            return challenge?.Status == expectedStatus;
        }


        public async Task<List<MatchInviteDto>> InviteFacultySpecialistsAsync(Guid challengeId, List<Guid> FacultySpecialistIds, Guid adminId)
        {
            var command = new InviteFacultySpecialistsCommand(challengeId, FacultySpecialistIds, adminId);
            return await _mediator.Send(command);
        }

        public async Task<MatchFinalizeDto> FinalizeMatchingAsync(Guid challengeId, Guid adminId)
        {
            var command = new FinalizeMatchingCommand(challengeId, adminId);
            return await _mediator.Send(command);
        }


        private async Task<ChallengeWithDetailsDto> MapToDetailsDtoAsync(Challenge challenge)
        {
            // Get file ID from FileStorage (single entity fallback)
            var fileId = await _fileReadService.GetFileIdByEntityAsync("Challenge", challenge.Id, "SupportingDocument");
            return MapToDetailsDtoWithFileId(challenge, fileId);
        }

        private ChallengeWithDetailsDto MapToDetailsDtoWithFileId(Challenge challenge, Guid? fileId)
        {
            var supportingDocumentUrl = fileId?.ToString();

            return new ChallengeWithDetailsDto
            {
                Id = challenge.Id,
                Title = challenge.Title,
                Description = challenge.Description,
                ResearchFieldId = challenge.ResearchFieldId,
                ResearchFieldName = challenge.ResearchField?.Name ?? "Unknown Research Field",
                EstimatedCost = challenge.EstimatedCost,
                SupportingDocumentUrl = supportingDocumentUrl,
                SubmittedBy = challenge.SubmittedBy,
                SubmitterName = challenge.UserSubmitted?.Name ?? "Unknown User",
                Status = challenge.Status,
                MatchingStatus = challenge.MatchingStatus,
                ApprovedBy = challenge.ApprovedBy,
                ApprovedByName = challenge.UserApproved?.Name,
                MatchedFacultySpecialistIds = challenge.MatchedFacultySpecialists?.Select(mp => mp.FacultySpecialistUserId).ToList() ?? new List<Guid>(),
                CreatedAt = challenge.CreatedAt,
                UpdatedAt = challenge.UpdatedAt,
                RejectionReason = challenge.RejectionReason
            };
        }
    }
}
