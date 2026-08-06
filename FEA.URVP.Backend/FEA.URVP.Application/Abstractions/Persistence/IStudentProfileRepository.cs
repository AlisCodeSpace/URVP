using FEA.URVP.Domain.Entities.StudentProfiles;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IStudentProfileRepository
{
    Task<StudentProfile?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(StudentProfile profile);
}
