using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Infrastructure.Data.Repositories.RDProjects.Interfaces;
using RICHConnect.Backend.Domain.Entities.RDProjects;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.RDProjects
{
    public class RDProjectRepository : IRDProjectRepository
    {
        private readonly AppDbContext _context;

        public RDProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RDProject?> GetByIdAsync(Guid id)
        {
            return await _context.RDProjects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<RDProject?> GetByIdWithIncludesAsync(Guid id)
        {
            return await _context.RDProjects
                .AsNoTracking()
                .Include(p => p.ResearchField)
                .Include(p => p.SupportTypes)
                .Include(p => p.MatchedFacultySpecialists!)
                    .ThenInclude(mp => mp.FacultySpecialist)
                .Include(p => p.UserSubmitted)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<RDProject>> GetByStatusAsync(RDProjectStatus status)
        {
            return await _context.RDProjects
                .AsNoTracking()
                .Where(p => p.Status == status)
                .ToListAsync();
        }

        public async Task<List<RDProject>> GetByStatusWithIncludesAsync(RDProjectStatus status)
        {
            return await _context.RDProjects
                .AsNoTracking()
                .Include(p => p.ResearchField)
                .Include(p => p.SupportTypes)
                .Include(p => p.UserSubmitted)
                .Include(p => p.UserApproved)
                .Include(p => p.MatchedFacultySpecialists!)
                    .ThenInclude(mp => mp.FacultySpecialist)
                .Where(p => p.Status == status)
                .ToListAsync();
        }

        public async Task<List<RDProject>> GetByUserAsync(Guid userId)
        {
            return await _context.RDProjects
                .AsNoTracking()
                .Include(p => p.SupportTypes)
                .Where(p => p.SubmittedBy == userId)
                .ToListAsync();
        }

        public async Task<RDProject> CreateAsync(RDProject rdProject)
        {
            _context.RDProjects.Add(rdProject);
            await _context.SaveChangesAsync();
            return rdProject;
        }

        public async Task<RDProject> UpdateAsync(RDProject rdProject)
        {
            // Check if an entity with the same key is already being tracked
            var existingEntry = _context.ChangeTracker.Entries<RDProject>()
                .FirstOrDefault(e => e.Entity.Id == rdProject.Id);
            
            if (existingEntry != null)
            {
                // Detach the existing tracked entity to avoid conflicts
                existingEntry.State = EntityState.Detached;
            }
            
            _context.RDProjects.Update(rdProject);
            await _context.SaveChangesAsync();
            return rdProject;
        }

        public async Task DeleteAsync(Guid id)
        {
            var project = await _context.RDProjects.FindAsync(id);
            if (project != null)
            {
                _context.RDProjects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.RDProjects.AsNoTracking().AnyAsync(p => p.Id == id);
        }
    }
}
