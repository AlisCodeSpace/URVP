using MediatR;

namespace RICHConnect.Backend.Application.Queries.Auth.GetFacultyProfileStatus
{
    /// <summary>
    /// Query to get the profile status for a faculty specialist user
    /// </summary>
    public class GetFacultyProfileStatusQuery : IRequest<int?>
    {
        /// <summary>
        /// The ID of the user to check profile status for
        /// </summary>
        public Guid UserId { get; }
        
        public GetFacultyProfileStatusQuery(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }
            
            UserId = userId;
        }
    }
}
