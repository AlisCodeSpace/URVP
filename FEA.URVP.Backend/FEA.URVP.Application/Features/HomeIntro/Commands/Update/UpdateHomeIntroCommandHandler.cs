using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.HomeIntro;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Domain.Catalog;
using Microsoft.Extensions.Logging;
using HomeIntroRow = FEA.URVP.Domain.Entities.HomeIntro.HomeIntro;

namespace FEA.URVP.Application.Commands.HomeIntro.Update;

public sealed class UpdateHomeIntroCommandHandler
    : BaseCommandHandler<UpdateHomeIntroCommand, HomeIntroDto>
{
    private readonly IHomeIntroRepository _intro;

    public UpdateHomeIntroCommandHandler(
        ILogger<UpdateHomeIntroCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IHomeIntroRepository intro)
        : base(logger, unitOfWork)
    {
        _intro = intro;
    }

    protected override async Task<HomeIntroDto> HandleInternal(
        UpdateHomeIntroCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var headline = request.Headline.Trim();
        var description = request.Description.Trim();
        var keyPoints = request.KeyPoints
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Take(HomeIntroRow.MaxKeyPoints)
            .ToList();

        var row = await _intro.GetAsync(cancellationToken);
        if (row is null)
        {
            row = new HomeIntroRow
            {
                Id = HomeIntroRow.SingletonId,
                Headline = HomeIntroCatalog.Headline,
                Description = HomeIntroCatalog.Description,
                KeyPoints = [.. HomeIntroCatalog.KeyPoints],
                CreatedAt = now,
            };
            _intro.Add(row);
        }

        row.Headline = headline;
        row.Description = description;
        row.KeyPoints = keyPoints;
        row.UpdatedAt = now;
        row.UpdatedByUserId = request.UpdatedByUserId == Guid.Empty
            ? null
            : request.UpdatedByUserId;

        await UnitOfWork.SaveChangesAsync(cancellationToken);
        return row.ToDto();
    }
}
