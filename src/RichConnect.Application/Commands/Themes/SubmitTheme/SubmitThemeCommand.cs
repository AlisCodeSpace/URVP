using MediatR;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Commands.Themes.SubmitTheme
{
    public class SubmitThemeCommand : IRequest<ResearchTheme>
    {
        // Required properties
        public string Title { get; set; } = string.Empty;
        public Guid SubmittedBy { get; set; }
        
        // Optional properties
        public string? Description { get; set; }
        public string? ExpectedOutcomes { get; set; }
        public double EstimatedFunding { get; set; }
        public Guid? ResearchFieldId { get; set; }
        
        // File uploads (supports multiple files)
        public List<IFormFile>? Images { get; set; }
        public List<IFormFile>? Documents { get; set; }
        
        // Single file uploads for backwards compatibility
        public IFormFile? Image { get; set; }
        public IFormFile? Document { get; set; }
        
        // Admin-specific properties
        public bool IsAdminCreated { get; set; }
        
        public SubmitThemeCommand()
        {
        }
        
        public SubmitThemeCommand(
            string title, 
            Guid submittedBy, 
            string? description = null,
            string? expectedOutcomes = null,
            double estimatedFunding = 0,
            Guid? researchFieldId = null,
            bool isAdminCreated = false)
        {
            Title = title;
            SubmittedBy = submittedBy;
            Description = description;
            ExpectedOutcomes = expectedOutcomes;
            EstimatedFunding = estimatedFunding;
            ResearchFieldId = researchFieldId;
            IsAdminCreated = isAdminCreated;
        }
    }
}
