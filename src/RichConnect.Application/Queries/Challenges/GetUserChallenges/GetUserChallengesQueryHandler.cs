using MediatR;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Application.DTOs.Challenge;
using RICHConnect.Backend.Domain.Entities.Challenges;

namespace RICHConnect.Backend.Application.Queries.GetUserChallenges
{
    public class GetUserChallengesQueryHandler : IRequestHandler<GetUserChallengesQuery, List<ChallengeDto>>
    {
        private readonly IChallengeRepository _repository;
        private readonly IFileReadService _fileReadService;
        private readonly ILogger<GetUserChallengesQueryHandler> _logger;

        public GetUserChallengesQueryHandler(
            IChallengeRepository repository,
            IFileReadService fileReadService,
            ILogger<GetUserChallengesQueryHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _fileReadService = fileReadService ?? throw new ArgumentNullException(nameof(fileReadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ChallengeDto>> Handle(GetUserChallengesQuery query, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling GetUserChallengesQuery for user: {UserId}", query.UserId);

                var challenges = await _repository.GetByUserAsync(query.UserId);
                var result = new List<ChallengeDto>();
                foreach (var challenge in challenges)
                {
                    result.Add(await MapToDtoAsync(challenge));
                }

                _logger.LogInformation("Successfully retrieved {Count} challenges for user: {UserId}", 
                    result.Count, query.UserId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetUserChallengesQuery for user: {UserId}", query.UserId);
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
