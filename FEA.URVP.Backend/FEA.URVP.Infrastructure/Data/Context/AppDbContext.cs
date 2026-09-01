using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Divisions;
using FEA.URVP.Domain.Entities.Files;
using FEA.URVP.Domain.Entities.News;
using FEA.URVP.Domain.Entities.ProjectRankings;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.StudentProfiles;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Entities.ValueLists;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Entities.Workshops;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FEA.URVP.Infrastructure.Data.Context;

/// <summary>
/// Application database context. Entity sets are added as features are implemented.
/// </summary>
public class AppDbContext : DbContext, IUnitOfWork, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();

    public DbSet<ProjectRanking> ProjectRankings => Set<ProjectRanking>();

    public DbSet<FileStorage> FileStorage => Set<FileStorage>();

    public DbSet<ValueListItem> ValueListItems => Set<ValueListItem>();

    public DbSet<Division> Divisions => Set<Division>();

    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();

    public DbSet<Workshop> Workshops => Set<Workshop>();

    public DbSet<Semester> Semesters => Set<Semester>();

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        await using IDbContextTransaction transaction =
            await Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await action(cancellationToken);
            await SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        await using IDbContextTransaction transaction =
            await Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await action(cancellationToken);
            await SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
