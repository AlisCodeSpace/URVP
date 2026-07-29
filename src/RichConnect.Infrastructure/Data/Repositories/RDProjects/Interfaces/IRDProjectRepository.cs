using RICHConnect.Backend.Domain.Entities.RDProjects;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.RDProjects.Interfaces
{
    public interface IRDProjectRepository
    {
        Task<RDProject?> GetByIdAsync(Guid id);
        Task<RDProject?> GetByIdWithIncludesAsync(Guid id);
        Task<List<RDProject>> GetByStatusAsync(RDProjectStatus status);
        Task<List<RDProject>> GetByStatusWithIncludesAsync(RDProjectStatus status);
        Task<List<RDProject>> GetByUserAsync(Guid userId);
        Task<RDProject> CreateAsync(RDProject rdProject);
        Task<RDProject> UpdateAsync(RDProject rdProject);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
