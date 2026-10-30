using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Workshops.Delete;

public sealed class DeleteWorkshopCommandHandler
    : BaseCommandHandler<DeleteWorkshopCommand>
{
    private readonly IWorkshopRepository _workshops;

    public DeleteWorkshopCommandHandler(
        ILogger<DeleteWorkshopCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IWorkshopRepository workshops)
        : base(logger, unitOfWork)
    {
        _workshops = workshops;
    }

    protected override async Task HandleCommandAsync(
        DeleteWorkshopCommand request,
        CancellationToken cancellationToken)
    {
        var workshop = await _workshops.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Workshop {request.Id} was not found.");

        _workshops.Remove(workshop);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Deleted workshop {WorkshopId} ({Title})", workshop.Id, workshop.Title);
    }
}
