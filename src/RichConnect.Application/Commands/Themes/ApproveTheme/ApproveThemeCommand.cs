using MediatR;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Commands.Themes.ApproveTheme
{
    public class ApproveThemeCommand : IRequest<ResearchTheme>
    {
        public Guid ThemeId { get; set; }
        public Guid ApprovedBy { get; set; }
        
        public ApproveThemeCommand()
        {
            // Default constructor for deserialization
        }
        
        public ApproveThemeCommand(Guid themeId, Guid approvedBy)
        {
            ThemeId = themeId;
            ApprovedBy = approvedBy;
        }
    }
}
