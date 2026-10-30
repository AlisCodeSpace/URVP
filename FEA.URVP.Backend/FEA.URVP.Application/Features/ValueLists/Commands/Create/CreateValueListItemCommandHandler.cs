using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Common;
using FEA.URVP.Application.Features.ValueLists.DTOs;
using FEA.URVP.Application.Features.ValueLists.Mappings;
using FEA.URVP.Domain.Entities.ValueLists;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Features.ValueLists.Commands.Create;

public sealed class CreateValueListItemCommandHandler
    : BaseCommandHandler<CreateValueListItemCommand, ValueListItemDto>
{
    private readonly IValueListRepository _valueLists;

    public CreateValueListItemCommandHandler(
        ILogger<CreateValueListItemCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IValueListRepository valueLists)
        : base(logger, unitOfWork)
    {
        _valueLists = valueLists;
    }

    protected override async Task<ValueListItemDto> HandleInternal(
        CreateValueListItemCommand request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var existing = await _valueLists.FindByKindAndNameAsync(
            request.Kind,
            name,
            cancellationToken);

        if (existing is not null)
        {
            throw new InvalidOperationException($"A value named \"{name}\" already exists in this list.");
        }

        var now = DateTime.UtcNow;
        var item = new ValueListItem
        {
            Kind = request.Kind,
            Name = name,
            SortOrder = await _valueLists.GetNextSortOrderAsync(request.Kind, cancellationToken),
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        _valueLists.Add(item);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Created value list item {ItemId} ({Kind}: {Name})",
            item.Id,
            item.Kind,
            item.Name);

        return item.ToDto();
    }
}
