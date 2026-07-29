using MediatR;
using RICHConnect.Backend.Application.DTOs.Themes;

namespace RICHConnect.Backend.Application.Queries.ResearchFields.GetFieldBySlug
{
    public class GetFieldBySlugQuery : IRequest<ResearchFieldDto>
    {
        public string Slug { get; }

        public GetFieldBySlugQuery(string slug)
        {
            Slug = slug;
        }
    }
}

