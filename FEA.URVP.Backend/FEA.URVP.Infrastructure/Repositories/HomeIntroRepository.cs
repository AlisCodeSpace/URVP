using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.HomeIntro;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class HomeIntroRepository : IHomeIntroRepository
{
    private readonly AppDbContext _db;

    public HomeIntroRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<HomeIntro?> GetAsync(CancellationToken cancellationToken = default) =>
        _db.HomeIntro.FirstOrDefaultAsync(
            x => x.Id == HomeIntro.SingletonId,
            cancellationToken);

    public void Add(HomeIntro intro) => _db.HomeIntro.Add(intro);
}
