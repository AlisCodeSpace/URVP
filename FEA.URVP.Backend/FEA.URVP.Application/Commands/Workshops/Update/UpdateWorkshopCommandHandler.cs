using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Workshops;
using FEA.URVP.Application.Mappings;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Workshops.Update;

public sealed class UpdateWorkshopCommandHandler
    : BaseCommandHandler<UpdateWorkshopCommand, WorkshopDto>
{
    private readonly IWorkshopRepository _workshops;

    public UpdateWorkshopCommandHandler(
        ILogger<UpdateWorkshopCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IWorkshopRepository workshops)
        : base(logger, unitOfWork)
    {
        _workshops = workshops;
    }

    protected override async Task<WorkshopDto> HandleInternal(
        UpdateWorkshopCommand request,
        CancellationToken cancellationToken)
    {
        var workshop = await _workshops.FindByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Workshop {request.Id} was not found.");

        workshop.Title = request.Title.Trim();
        workshop.Date = request.Date.Trim();
        workshop.Time = string.IsNullOrWhiteSpace(request.Time) ? null : request.Time.Trim();
        workshop.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
        workshop.Description = request.Description.Trim();
        workshop.RegistrationUrl = request.RegistrationUrl.Trim();
        workshop.PosterFileId = request.PosterFileId;
        workshop.PosterAlt = string.IsNullOrWhiteSpace(request.PosterAlt) ? null : request.PosterAlt.Trim();
        workshop.UpdatedAt = DateTime.UtcNow;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Updated workshop {WorkshopId}", workshop.Id);

        return workshop.ToDto();
    }
}
