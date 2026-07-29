using MediatR;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Application.DTOs.Challenge;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Challenges;

namespace RICHConnect.Backend.Application.Queries.GetChallengeById
{
    public class GetChallengeByIdQueryHandler : IRequestHandler<GetChallengeByIdQuery, ChallengeDto?>
    {
        private readonly IChallengeRepository _repository;
        private readonly IFileReadService _fileReadService;
        private readonly ILogger<GetChallengeByIdQueryHandler> _logger;

        public GetChallengeByIdQueryHandler(
            IChallengeRepository repository,
            IFileReadService fileReadService,
            ILogger<GetChallengeByIdQueryHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _fileReadService = fileReadService ?? throw new ArgumentNullException(nameof(fileReadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ChallengeDto?> Handle(GetChallengeByIdQuery query, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling GetChallengeByIdQuery for challenge: {ChallengeId}, user: {UserId}", 
                    query.ChallengeId, query.UserId);

                var challenge = await _repository.GetByIdWithIncludesAsync(query.ChallengeId);
                if (challenge == null)
                {
                    _logger.LogWarning("Challenge not found: {ChallengeId}", query.ChallengeId);
                    return null;
                }

                // Check permissions
                var isAdmin = query.UserRole == "Admin";
                var isOwner = challenge.SubmittedBy == query.UserId;
                
                if (!isAdmin && !isOwner && challenge.Status != ChallengeStatus.Approved)
                {
                    _logger.LogWarning("Access denied for user {UserId} to challenge {ChallengeId}", 
                        query.UserId, query.ChallengeId);
                    throw new UnauthorizedAccessException("You don't have permission to view this challenge.");
                }

                var result = await MapToDtoAsync(challenge);
                _logger.LogInformation("Successfully retrieved challenge: {ChallengeId}", query.ChallengeId);
                return result;
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Re-throw authorization exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetChallengeByIdQuery for challenge: {ChallengeId}", query.ChallengeId);
                throw;
            }
        }

        private async Task<ChallengeDto> MapToDtoAsync(Challenge challenge)
        {
            // Get file ID from FileStorage
            var fileId = await _fileReadService.GetFileIdByEntityAsync("Challenge", challenge.Id, "SupportingDocument");
            var supportingDocumentUrl = fileId?.ToString();

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
