using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.StudentProfiles;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class StudentProfileRepository : IStudentProfileRepository
{
    private readonly AppDbContext _db;

    public StudentProfileRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<StudentProfile?> FindByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _db.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public void Add(StudentProfile profile) => _db.StudentProfiles.Add(profile);
}
