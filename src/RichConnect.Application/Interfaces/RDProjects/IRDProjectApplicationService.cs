using RICHConnect.Backend.Application.DTOs.RDProject;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Interfaces.RDProjects
{
    public interface IRDProjectApplicationService
    {
        Task<RDProjectDto> CreateRDProjectAsync(CreateRDProjectDto dto, Guid userId);
        Task<RDProjectDto?> GetRDProjectByIdAsync(Guid id);
        Task<List<RDProjectDto>> GetUserRDProjectsAsync(Guid userId);
        
        // Admin operations
        Task<List<RDProjectDto>> GetRDProjectsByStatusAsync(RDProjectStatus status);
        Task<RDProjectDto> ApproveRDProjectAsync(Guid id, Guid adminId);
        Task<RDProjectDto> RejectRDProjectAsync(Guid id, Guid adminId, string rejectionReason);
    }
}
