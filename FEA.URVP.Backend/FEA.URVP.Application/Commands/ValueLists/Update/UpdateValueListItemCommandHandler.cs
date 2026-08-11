using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.ValueLists;
using FEA.URVP.Application.Mappings;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.ValueLists.Update;

public sealed class UpdateValueListItemCommandHandler
    : BaseCommandHandler<UpdateValueListItemCommand, ValueListItemDto>
{
    private readonly IValueListRepository _valueLists;

    public UpdateValueListItemCommandHandler(
        ILogger<UpdateValueListItemCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IValueListRepository valueLists)
        : base(logger, unitOfWork)
    {
        _valueLists = valueLists;
    }

    protected override async Task<ValueListItemDto> HandleInternal(
        UpdateValueListItemCommand request,
        CancellationToken cancellationToken)
    {
        var item = await _valueLists.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Value list item {request.Id} was not found.");

        if (item.Kind != request.Kind)
        {
            throw new KeyNotFoundException($"Value list item {request.Id} was not found in this list.");
        }

        var name = request.Name.Trim();
        var duplicate = await _valueLists.FindByKindAndNameAsync(request.Kind, name, cancellationToken);
        if (duplicate is not null && duplicate.Id != item.Id)
        {
            throw new InvalidOperationException($"A value named \"{name}\" already exists in this list.");
        }

        item.Name = name;
        if (request.IsActive.HasValue)
        {
            item.IsActive = request.IsActive.Value;
        }

        item.UpdatedAt = DateTime.UtcNow;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Updated value list item {ItemId}", item.Id);

        return item.ToDto();
    }
}
