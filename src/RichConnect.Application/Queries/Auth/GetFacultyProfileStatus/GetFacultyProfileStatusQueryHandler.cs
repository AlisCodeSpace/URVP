using MediatR;
using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Queries.Auth.GetFacultyProfileStatus
{
    /// <summary>
    /// Handler for GetFacultyProfileStatusQuery. Retrieves the profile status for a faculty specialist user.
    /// </summary>
    public class GetFacultyProfileStatusQueryHandler : IRequestHandler<GetFacultyProfileStatusQuery, int?>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GetFacultyProfileStatusQueryHandler> _logger;
        
        public GetFacultyProfileStatusQueryHandler(
            AppDbContext context,
            ILogger<GetFacultyProfileStatusQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<int?> Handle(GetFacultyProfileStatusQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Querying faculty profile status for user: {UserId}", request.UserId);
            
            // Find the user and check if they are a faculty specialist
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
                
            if (user == null || user.Role != UserRole.FacultySpecialist)
            {
                _logger.LogInformation("User not found or not a faculty specialist: {UserId}", request.UserId);
                return null;
            }
            
            // Get the faculty specialist profile
            var facultySpecialist = await _context.FacultySpecialists
                .AsNoTracking()
                .FirstOrDefaultAsync(fs => fs.UserId == user.Id, cancellationToken);
                
            // Note: Status field has been removed from FacultySpecialist
            // Always return null for status as it no longer exists
            var status = facultySpecialist != null ? (int?)0 : null;
            
            _logger.LogInformation("Faculty profile status for user {UserId}: {Status}", 
                request.UserId, status.HasValue ? status.ToString() : "null");
                
            return status;
        }
    }
}
