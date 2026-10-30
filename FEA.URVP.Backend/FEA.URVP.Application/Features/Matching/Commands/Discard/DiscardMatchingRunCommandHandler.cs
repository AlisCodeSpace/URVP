using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Common;
using FEA.URVP.Application.Features.Matching.DTOs;
using FEA.URVP.Application.Features.Matching.Mappings;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Features.Matching.Commands.Discard;

public sealed class DiscardMatchingRunCommandHandler
    : BaseCommandHandler<DiscardMatchingRunCommand, MatchingRunDto>
{
    private readonly IMatchingRunRepository _runs;

    public DiscardMatchingRunCommandHandler(
        ILogger<DiscardMatchingRunCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IMatchingRunRepository runs)
        : base(logger, unitOfWork)
    {
        _runs = runs;
    }

    protected override async Task<MatchingRunDto> HandleInternal(
        DiscardMatchingRunCommand request,
        CancellationToken cancellationToken)
    {
        var run = await _runs.FindByIdAsync(request.RunId, cancellationToken)
            ?? throw new KeyNotFoundException($"Matching run {request.RunId} was not found.");

        run.Discard(DateTime.UtcNow);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Matching run {RunId} discarded", run.Id);

        return run.ToDto();
    }
}
