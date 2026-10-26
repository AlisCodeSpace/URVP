using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class ProjectResearchAssistantRepository : IProjectResearchAssistantRepository
{
    private readonly AppDbContext _db;

    public ProjectResearchAssistantRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProjectResearchAssistant>> ListByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        await _db.ProjectResearchAssistants
            .AsNoTracking()
            .Where(assistant => assistant.ProjectId == projectId)
            .OrderBy(assistant => assistant.Name)
            .ThenBy(assistant => assistant.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<ProjectResearchAssistant?> FindTrackedAsync(
        Guid projectId,
        Guid assistantId,
        CancellationToken cancellationToken = default) =>
        _db.ProjectResearchAssistants.FirstOrDefaultAsync(
            assistant => assistant.ProjectId == projectId && assistant.Id == assistantId,
            cancellationToken);

    public void Add(ProjectResearchAssistant assistant) =>
        _db.ProjectResearchAssistants.Add(assistant);

    public void Remove(ProjectResearchAssistant assistant) =>
        _db.ProjectResearchAssistants.Remove(assistant);
}
