using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.ValueLists.Delete;

public sealed class DeleteValueListItemCommandHandler
    : BaseCommandHandler<DeleteValueListItemCommand>
{
    private readonly IValueListRepository _valueLists;

    public DeleteValueListItemCommandHandler(
        ILogger<DeleteValueListItemCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IValueListRepository valueLists)
        : base(logger, unitOfWork)
    {
        _valueLists = valueLists;
    }

    protected override async Task HandleCommandAsync(
        DeleteValueListItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await _valueLists.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Value list item {request.Id} was not found.");

        if (item.Kind != request.Kind)
        {
            throw new KeyNotFoundException($"Value list item {request.Id} was not found in this list.");
        }

        _valueLists.Remove(item);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Deleted value list item {ItemId} ({Kind}: {Name})",
            item.Id,
            item.Kind,
            item.Name);
    }
}
