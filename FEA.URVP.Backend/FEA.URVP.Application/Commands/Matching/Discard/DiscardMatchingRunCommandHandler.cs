using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.Mappings;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Matching.Discard;

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
