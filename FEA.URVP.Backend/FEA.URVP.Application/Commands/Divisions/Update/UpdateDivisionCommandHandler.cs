using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Divisions;
using FEA.URVP.Application.Mappings;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Divisions.Update;

public sealed class UpdateDivisionCommandHandler
    : BaseCommandHandler<UpdateDivisionCommand, DivisionDto>
{
    private readonly IDivisionRepository _divisions;

    public UpdateDivisionCommandHandler(
        ILogger<UpdateDivisionCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IDivisionRepository divisions)
        : base(logger, unitOfWork)
    {
        _divisions = divisions;
    }

    protected override async Task<DivisionDto> HandleInternal(
        UpdateDivisionCommand request,
        CancellationToken cancellationToken)
    {
        var division = await _divisions.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Division {request.Id} was not found.");

        var name = request.Name.Trim();
        var duplicate = await _divisions.FindByNameAsync(name, cancellationToken);
        if (duplicate is not null && duplicate.Id != division.Id)
        {
            throw new InvalidOperationException($"A division named \"{name}\" already exists.");
        }

        division.Name = name;
        if (request.Description is not null)
        {
            division.Description = request.Description.Trim();
        }

        if (request.IsActive.HasValue)
        {
            division.IsActive = request.IsActive.Value;
        }

        division.UpdatedAt = DateTime.UtcNow;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Updated division {DivisionId}", division.Id);

        return division.ToDto();
    }
}
