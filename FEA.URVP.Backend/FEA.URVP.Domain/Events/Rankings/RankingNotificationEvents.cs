using FEA.URVP.Domain.Events;

namespace FEA.URVP.Domain.Events.Rankings;

public sealed class ProjectRankingSubmittedEvent : DomainEvent
{
    public ProjectRankingSubmittedEvent(
        Guid rankingId,
        Guid projectId,
        Guid ownerUserId,
        string projectTitle,
        string studentName)
    {
        RankingId = rankingId;
        ProjectId = projectId;
        OwnerUserId = ownerUserId;
        ProjectTitle = projectTitle;
        StudentName = studentName;
    }

    public Guid RankingId { get; }
    public Guid ProjectId { get; }
    public Guid OwnerUserId { get; }
    public string ProjectTitle { get; }
    public string StudentName { get; }
}

public sealed class ProjectRankingRemovedEvent : DomainEvent
{
    public ProjectRankingRemovedEvent(
        Guid projectId,
        Guid ownerUserId,
        string projectTitle,
        string studentName)
    {
        ProjectId = projectId;
        OwnerUserId = ownerUserId;
        ProjectTitle = projectTitle;
        StudentName = studentName;
    }

    public Guid ProjectId { get; }
    public Guid OwnerUserId { get; }
    public string ProjectTitle { get; }
    public string StudentName { get; }
}
