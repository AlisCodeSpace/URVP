using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Divisions;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Domain.Entities.Divisions;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Divisions.Create;

public sealed class CreateDivisionCommandHandler
    : BaseCommandHandler<CreateDivisionCommand, DivisionDto>
{
    private readonly IDivisionRepository _divisions;

    public CreateDivisionCommandHandler(
        ILogger<CreateDivisionCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IDivisionRepository divisions)
        : base(logger, unitOfWork)
    {
        _divisions = divisions;
    }

    protected override async Task<DivisionDto> HandleInternal(
        CreateDivisionCommand request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var existing = await _divisions.FindByNameAsync(name, cancellationToken);

        if (existing is not null)
        {
            throw new InvalidOperationException($"A division named \"{name}\" already exists.");
        }

        var now = DateTime.UtcNow;
        var division = new Division
        {
            Name = name,
            Description = (request.Description ?? string.Empty).Trim(),
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        _divisions.Add(division);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Created division {DivisionId} ({Name})", division.Id, division.Name);

        return division.ToDto();
    }
}
