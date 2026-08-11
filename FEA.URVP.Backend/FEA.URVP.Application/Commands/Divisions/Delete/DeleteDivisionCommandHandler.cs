using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Divisions.Delete;

public sealed class DeleteDivisionCommandHandler
    : BaseCommandHandler<DeleteDivisionCommand>
{
    private readonly IDivisionRepository _divisions;

    public DeleteDivisionCommandHandler(
        ILogger<DeleteDivisionCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IDivisionRepository divisions)
        : base(logger, unitOfWork)
    {
        _divisions = divisions;
    }

    protected override async Task HandleCommandAsync(
        DeleteDivisionCommand request,
        CancellationToken cancellationToken)
    {
        var division = await _divisions.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Division {request.Id} was not found.");

        _divisions.Remove(division);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Deleted division {DivisionId} ({Name})",
            division.Id,
            division.Name);
    }
}
