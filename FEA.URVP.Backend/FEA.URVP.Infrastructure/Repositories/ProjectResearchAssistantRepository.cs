using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Enums;
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

    public Task<bool> IsListedOnProjectAsync(
        Guid projectId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(email);
        if (normalized.Length == 0)
        {
            return Task.FromResult(false);
        }

        return _db.ProjectResearchAssistants
            .AsNoTracking()
            .AnyAsync(
                assistant => assistant.ProjectId == projectId && assistant.Email == normalized,
                cancellationToken);
    }

    public async Task<bool> CanViewStudentAsync(
        string email,
        Guid studentUserId,
        CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(email);
        if (normalized.Length == 0)
        {
            return false;
        }

        var projectIds = _db.ProjectResearchAssistants
            .AsNoTracking()
            .Where(assistant => assistant.Email == normalized)
            .Select(assistant => assistant.ProjectId);

        if (await _db.ProjectRankings.AsNoTracking().AnyAsync(
                ranking => ranking.StudentUserId == studentUserId
                           && projectIds.Contains(ranking.ProjectId),
                cancellationToken))
        {
            return true;
        }

        if (await _db.Placements.AsNoTracking().AnyAsync(
                placement => placement.StudentUserId == studentUserId
                             && placement.Status == PlacementStatus.Confirmed
                             && placement.MatchingRun.SemesterId == placement.Project.SemesterId
                             && projectIds.Contains(placement.ProjectId),
                cancellationToken))
        {
            return true;
        }

        return await _db.ProjectAlumni.AsNoTracking().AnyAsync(
            alumnus => alumnus.StudentUserId == studentUserId
                       && alumnus.WasConfirmed
                       && projectIds.Contains(alumnus.ProjectId),
            cancellationToken);
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

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
