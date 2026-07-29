using MediatR;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Queries.GetEditRequestByChallengeId
{
    /// <summary>
    /// Handler for GetEditRequestByChallengeIdQuery
    /// </summary>
    public class GetEditRequestByChallengeIdQueryHandler : IRequestHandler<GetEditRequestByChallengeIdQuery, ChallengeEditRequestDto?>
    {
        private readonly IChallengeEditRequestRepository _editRequestRepository;
        private readonly ILogger<GetEditRequestByChallengeIdQueryHandler> _logger;

        public GetEditRequestByChallengeIdQueryHandler(
            IChallengeEditRequestRepository editRequestRepository,
            ILogger<GetEditRequestByChallengeIdQueryHandler> logger)
        {
            _editRequestRepository = editRequestRepository;
            _logger = logger;
        }

        public async Task<ChallengeEditRequestDto?> Handle(GetEditRequestByChallengeIdQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetEditRequestByChallengeIdQuery for ChallengeId: {ChallengeId}", query.ChallengeId);

            var editRequests = await _editRequestRepository.GetByChallengeIdAsync(query.ChallengeId);
            var latestEditRequest = editRequests.FirstOrDefault();
            
            if (latestEditRequest == null)
            {
                _logger.LogInformation("No edit request found for challenge: {ChallengeId}", query.ChallengeId);
                return null;
            }

            var dto = new ChallengeEditRequestDto
            {
                Id = latestEditRequest.Id,
                ChallengeId = latestEditRequest.ChallengeId,
                EditReason = latestEditRequest.EditReason,
                RequestedBy = latestEditRequest.RequestedBy,
                RequestedByName = latestEditRequest.RequestedByUser?.Name ?? "Unknown",
                RequestedAt = latestEditRequest.RequestedAt,
                Status = (int)latestEditRequest.Status,
                AdminResponse = latestEditRequest.AdminResponse,
                RespondedAt = latestEditRequest.RespondedAt,
                RespondedBy = latestEditRequest.RespondedBy
            };

            _logger.LogInformation("Returning edit request DTO: {Dto}", System.Text.Json.JsonSerializer.Serialize(dto));
            return dto;
        }
    }
}
