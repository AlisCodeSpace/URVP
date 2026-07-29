using MediatR;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Queries.GetEditRequestDetails
{
    /// <summary>
    /// Handler for GetEditRequestDetailsQuery
    /// </summary>
    public class GetEditRequestDetailsQueryHandler : IRequestHandler<GetEditRequestDetailsQuery, ChallengeEditRequestDto?>
    {
        private readonly IChallengeEditRequestRepository _editRequestRepository;
        private readonly ILogger<GetEditRequestDetailsQueryHandler> _logger;

        public GetEditRequestDetailsQueryHandler(
            IChallengeEditRequestRepository editRequestRepository,
            ILogger<GetEditRequestDetailsQueryHandler> logger)
        {
            _editRequestRepository = editRequestRepository;
            _logger = logger;
        }

        public async Task<ChallengeEditRequestDto?> Handle(GetEditRequestDetailsQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetEditRequestDetailsQuery for EditRequestId: {EditRequestId}", query.EditRequestId);

            var editRequest = await _editRequestRepository.GetByIdAsync(query.EditRequestId);
            if (editRequest == null)
            {
                _logger.LogWarning("Edit request not found: {EditRequestId}", query.EditRequestId);
                return null;
            }

            return new ChallengeEditRequestDto
            {
                Id = editRequest.Id,
                ChallengeId = editRequest.ChallengeId,
                EditReason = editRequest.EditReason,
                RequestedBy = editRequest.RequestedBy,
                RequestedByName = editRequest.RequestedByUser?.Name,
                RequestedAt = editRequest.RequestedAt,
                Status = (int)editRequest.Status,
                AdminResponse = editRequest.AdminResponse,
                RespondedAt = editRequest.RespondedAt,
                RespondedBy = editRequest.RespondedBy
            };
        }
    }
}
