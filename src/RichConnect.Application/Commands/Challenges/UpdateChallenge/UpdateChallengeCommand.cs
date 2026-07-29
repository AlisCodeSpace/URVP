using MediatR;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Commands.UpdateChallenge
{
    public class UpdateChallengeCommand : IRequest<ChallengeDto>
    {
        public Guid ChallengeId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Guid ResearchFieldId { get; set; }
        public decimal EstimatedCost { get; set; }
        public string? SupportingDocumentUrl { get; set; }
        public Guid UpdatedBy { get; set; }

        public UpdateChallengeCommand(
            Guid challengeId,
            string title,
            string? description,
            Guid researchFieldId,
            decimal estimatedCost,
            string? supportingDocumentUrl,
            Guid updatedBy)
        {
            ChallengeId = challengeId;
            Title = title;
            Description = description;
            ResearchFieldId = researchFieldId;
            EstimatedCost = estimatedCost;
            SupportingDocumentUrl = supportingDocumentUrl;
            UpdatedBy = updatedBy;
        }
    }
}
