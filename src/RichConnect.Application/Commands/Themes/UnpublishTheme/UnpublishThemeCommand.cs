using MediatR;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Commands.Themes.UnpublishTheme
{
    public class UnpublishThemeCommand : IRequest<ResearchTheme>
    {
        public Guid ThemeId { get; set; }
        public Guid UnpublishedBy { get; set; }
        
        public UnpublishThemeCommand()
        {
            // Default constructor for deserialization
        }
        
        public UnpublishThemeCommand(Guid themeId, Guid unpublishedBy)
        {
            ThemeId = themeId;
            UnpublishedBy = unpublishedBy;
        }
    }
}
