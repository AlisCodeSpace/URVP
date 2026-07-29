using MediatR;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Queries.Themes.GetThemeBySlug
{
    public class GetThemeBySlugQuery : IRequest<ResearchTheme?>
    {
        public string Slug { get; set; } = string.Empty;
        
        public GetThemeBySlugQuery()
        {
            // Default constructor for deserialization
        }
        
        public GetThemeBySlugQuery(string slug)
        {
            Slug = slug;
        }
    }
}
