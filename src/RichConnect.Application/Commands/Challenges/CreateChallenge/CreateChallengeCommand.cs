using MediatR;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Commands.CreateChallenge
{
    public class CreateChallengeCommand : IRequest<ChallengeDto>
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Guid ResearchFieldId { get; set; }
        public string? OtherResearchFieldName { get; set; }
        public decimal EstimatedCost { get; set; }
        public string? SupportingDocumentUrl { get; set; }
        public Guid SubmittedBy { get; set; }

        public CreateChallengeCommand(
            string title,
            string? description,
            Guid researchFieldId,
            string? otherResearchFieldName,
            decimal estimatedCost,
            string? supportingDocumentUrl,
            Guid submittedBy)
        {
            Title = title;
            Description = description;
            ResearchFieldId = researchFieldId;
            OtherResearchFieldName = otherResearchFieldName;
            EstimatedCost = estimatedCost;
            SupportingDocumentUrl = supportingDocumentUrl;
            SubmittedBy = submittedBy;
        }
    }
}
