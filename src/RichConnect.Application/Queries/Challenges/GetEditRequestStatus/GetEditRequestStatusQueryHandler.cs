using MediatR;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Application.DTOs.Challenge;
using RICHConnect.Backend.Domain.Entities.Challenges;

namespace RICHConnect.Backend.Application.Queries.GetEditRequestStatus
{
    public class GetEditRequestStatusQueryHandler : IRequestHandler<GetEditRequestStatusQuery, ChallengeEditRequestDto?>
    {
        private readonly IChallengeEditRequestRepository _editRequestRepository;
        private readonly IChallengeRepository _challengeRepository;
        private readonly ILogger<GetEditRequestStatusQueryHandler> _logger;

        public GetEditRequestStatusQueryHandler(
            IChallengeEditRequestRepository editRequestRepository,
            IChallengeRepository challengeRepository,
            ILogger<GetEditRequestStatusQueryHandler> logger)
        {
            _editRequestRepository = editRequestRepository ?? throw new ArgumentNullException(nameof(editRequestRepository));
            _challengeRepository = challengeRepository ?? throw new ArgumentNullException(nameof(challengeRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ChallengeEditRequestDto?> Handle(GetEditRequestStatusQuery query, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling GetEditRequestStatusQuery for challenge: {ChallengeId}, user: {UserId}", 
                    query.ChallengeId, query.UserId);

                // First verify the challenge exists and user has permission
                var challenge = await _challengeRepository.GetByIdAsync(query.ChallengeId);
                if (challenge == null)
                {
                    _logger.LogWarning("Challenge not found: {ChallengeId}", query.ChallengeId);
                    return null;
                }

                // Check if user is the owner of the challenge
                if (challenge.SubmittedBy != query.UserId)
                {
                    _logger.LogWarning("Access denied for user {UserId} to challenge {ChallengeId}", 
                        query.UserId, query.ChallengeId);
                    throw new UnauthorizedAccessException("You don't have permission to view edit requests for this challenge.");
                }

                // Get the most recent edit request for this challenge
                var editRequests = await _editRequestRepository.GetByChallengeIdAsync(query.ChallengeId);
                var latestEditRequest = editRequests.FirstOrDefault();

                if (latestEditRequest == null)
                {
                    _logger.LogInformation("No edit request found for challenge: {ChallengeId}", query.ChallengeId);
                    return null;
                }

                var result = MapToDto(latestEditRequest);
                _logger.LogInformation("Successfully retrieved edit request status for challenge: {ChallengeId}", query.ChallengeId);
                return result;
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Re-throw authorization exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetEditRequestStatusQuery for challenge: {ChallengeId}", query.ChallengeId);
                throw;
            }
        }

        private ChallengeEditRequestDto MapToDto(ChallengeEditRequest editRequest)
        {
            return new ChallengeEditRequestDto
            {
                Id = editRequest.Id,
                ChallengeId = editRequest.ChallengeId,
                EditReason = editRequest.EditReason,
                RequestedBy = editRequest.RequestedBy,
                RequestedByName = editRequest.RequestedByUser?.Name ?? "Unknown",
                RequestedAt = editRequest.RequestedAt,
                Status = (int)editRequest.Status,
                AdminResponse = editRequest.AdminResponse,
                RespondedAt = editRequest.RespondedAt,
                RespondedBy = editRequest.RespondedBy
            };
        }
    }
}
