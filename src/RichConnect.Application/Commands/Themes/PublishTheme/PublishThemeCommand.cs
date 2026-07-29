using MediatR;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Commands.Themes.PublishTheme
{
    public class PublishThemeCommand : IRequest<ResearchTheme>
    {
        public Guid ThemeId { get; set; }
        public Guid PublishedBy { get; set; }
        
        public PublishThemeCommand()
        {
            // Default constructor for deserialization
        }
        
        public PublishThemeCommand(Guid themeId, Guid publishedBy)
        {
            ThemeId = themeId;
            PublishedBy = publishedBy;
        }
    }
}
