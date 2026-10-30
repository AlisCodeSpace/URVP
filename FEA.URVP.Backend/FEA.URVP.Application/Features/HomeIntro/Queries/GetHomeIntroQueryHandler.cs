using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.HomeIntro.DTOs;
using FEA.URVP.Application.Features.HomeIntro.Mappings;
using MediatR;

namespace FEA.URVP.Application.Features.HomeIntro.Queries;

public sealed class GetHomeIntroQueryHandler
    : IRequestHandler<GetHomeIntroQuery, HomeIntroDto>
{
    private readonly IHomeIntroRepository _intro;

    public GetHomeIntroQueryHandler(IHomeIntroRepository intro)
    {
        _intro = intro;
    }

    public async Task<HomeIntroDto> Handle(
        GetHomeIntroQuery request,
        CancellationToken cancellationToken)
    {
        var row = await _intro.GetAsync(cancellationToken);
        return row is null ? HomeIntroMappings.Defaults() : row.ToDto();
    }
}
