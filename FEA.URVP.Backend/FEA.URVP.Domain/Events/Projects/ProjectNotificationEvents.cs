using FEA.URVP.Domain.Events;

namespace FEA.URVP.Domain.Events.Projects;

public sealed class ProjectOpenedEvent : DomainEvent
{
    public ProjectOpenedEvent(Guid projectId)
    {
        ProjectId = projectId;
    }

    public Guid ProjectId { get; }
}

public sealed class ProjectClosedEvent : DomainEvent
{
    public ProjectClosedEvent(Guid projectId, Guid ownerUserId, bool notifyOwner)
    {
        ProjectId = projectId;
        OwnerUserId = ownerUserId;
        NotifyOwner = notifyOwner;
    }

    public Guid ProjectId { get; }
    public Guid OwnerUserId { get; }
    public bool NotifyOwner { get; }
}

public sealed class ProjectDeletedEvent : DomainEvent
{
    public ProjectDeletedEvent(Guid projectId, Guid ownerUserId, string projectTitle)
    {
        ProjectId = projectId;
        OwnerUserId = ownerUserId;
        ProjectTitle = projectTitle;
    }

    public Guid ProjectId { get; }
    public Guid OwnerUserId { get; }
    public string ProjectTitle { get; }
}
