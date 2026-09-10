using FEA.URVP.Domain.Entities.HomeIntro;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IHomeIntroRepository
{
    Task<HomeIntro?> GetAsync(CancellationToken cancellationToken = default);

    void Add(HomeIntro intro);
}
