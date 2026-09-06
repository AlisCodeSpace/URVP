using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Workshops;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Entities.Workshops;
using FEA.URVP.Domain.Events.Workshops;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Workshops.Create;

public sealed class CreateWorkshopCommandHandler
    : BaseCommandHandler<CreateWorkshopCommand, WorkshopDto>
{
    private readonly IWorkshopRepository _workshops;
    private readonly IEventBus _eventBus;

    public CreateWorkshopCommandHandler(
        ILogger<CreateWorkshopCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IWorkshopRepository workshops,
        IEventBus eventBus)
        : base(logger, unitOfWork)
    {
        _workshops = workshops;
        _eventBus = eventBus;
    }

    protected override async Task<WorkshopDto> HandleInternal(
        CreateWorkshopCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var workshop = new Workshop
        {
            Title = request.Title.Trim(),
            Date = request.Date.Trim(),
            Time = string.IsNullOrWhiteSpace(request.Time) ? null : request.Time.Trim(),
            Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
            Description = request.Description.Trim(),
            RegistrationUrl = request.RegistrationUrl.Trim(),
            PosterAlt = string.IsNullOrWhiteSpace(request.PosterAlt) ? null : request.PosterAlt.Trim(),
            SortOrder = await _workshops.GetNextSortOrderAsync(cancellationToken),
            CreatedAt = now,
            UpdatedAt = now,
        };

        _workshops.Add(workshop);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation("Created workshop {WorkshopId} ({Title})", workshop.Id, workshop.Title);

        await NotificationEventPublish.TryPublishAsync(
            _eventBus,
            new WorkshopAnnouncedEvent(workshop.Id),
            Logger,
            cancellationToken);

        return workshop.ToDto();
    }
}
