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
            .OrderBy(assistant => assistant.CreatedAt)
            .ThenBy(assistant => assistant.Email)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProjectResearchAssistant>> ListByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _db.ProjectResearchAssistants
            .AsNoTracking()
            .Include(assistant => assistant.Project)
                .ThenInclude(project => project.Semester)
            .Where(assistant => assistant.Email == normalized)
            .OrderByDescending(assistant => assistant.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, string>> NamesForEmailsAsync(
        IReadOnlyCollection<string> emails,
        CancellationToken cancellationToken = default)
    {
        if (emails.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        var normalized = emails.Select(email => email.Trim().ToLowerInvariant()).Distinct().ToList();
        var rows = await _db.Users
            .AsNoTracking()
            .Where(user => normalized.Contains(user.Email))
            .Select(user => new { user.Email, user.Name })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.Email.ToLowerInvariant())
            .ToDictionary(group => group.Key, group => group.First().Name);
    }

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
