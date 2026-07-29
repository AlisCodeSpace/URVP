using MediatR;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Queries.Themes.GetThemeById
{
    public class GetThemeByIdQuery : IRequest<ResearchTheme?>
    {
        public Guid ThemeId { get; set; }
        
        public GetThemeByIdQuery()
        {
            // Default constructor for deserialization
        }
        
        public GetThemeByIdQuery(Guid themeId)
        {
            ThemeId = themeId;
        }
    }
}
