using MediatR;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Commands.Themes.UpdateTheme
{
    public class UpdateThemeCommand : IRequest<ResearchTheme>
    {
        public Guid ThemeId { get; set; }
        public Guid UpdatedBy { get; set; }
        
        // Updatable properties
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ExpectedOutcomes { get; set; }
        public double? EstimatedFunding { get; set; }
        public Guid? ResearchFieldId { get; set; }
        
        // File uploads
        public IFormFile? Image { get; set; }
        public IFormFile? Document { get; set; }
        
        public UpdateThemeCommand()
        {
        }
        
        public UpdateThemeCommand(
            Guid themeId, 
            Guid updatedBy,
            string? title = null,
            string? description = null,
            string? expectedOutcomes = null,
            double? estimatedFunding = null,
            Guid? researchFieldId = null)
        {
            ThemeId = themeId;
            UpdatedBy = updatedBy;
            Title = title;
            Description = description;
            ExpectedOutcomes = expectedOutcomes;
            EstimatedFunding = estimatedFunding;
            ResearchFieldId = researchFieldId;
        }
    }
}
