using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Challenges;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces
{
    /// <summary>
    /// Repository interface for ChallengeEditRequest operations
    /// </summary>
    public interface IChallengeEditRequestRepository
    {
        /// <summary>
        /// Creates a new challenge edit request
        /// </summary>
        /// <param name="request">The edit request to create</param>
        /// <returns>The created edit request</returns>
        Task<ChallengeEditRequest> CreateAsync(ChallengeEditRequest request);

        /// <summary>
        /// Gets an edit request by its ID
        /// </summary>
        /// <param name="id">The edit request ID</param>
        /// <returns>The edit request if found, null otherwise</returns>
        Task<ChallengeEditRequest?> GetByIdAsync(Guid id);

        /// <summary>
        /// Gets all edit requests for a specific challenge
        /// </summary>
        /// <param name="challengeId">The challenge ID</param>
        /// <returns>List of edit requests for the challenge</returns>
        Task<List<ChallengeEditRequest>> GetByChallengeIdAsync(Guid challengeId);

        /// <summary>
        /// Gets all pending edit requests
        /// </summary>
        /// <returns>List of pending edit requests</returns>
        Task<List<ChallengeEditRequest>> GetPendingRequestsAsync();

        /// <summary>
        /// Gets edit requests by status
        /// </summary>
        /// <param name="status">The status to filter by</param>
        /// <returns>List of edit requests with the specified status</returns>
        Task<List<ChallengeEditRequest>> GetByStatusAsync(EditRequestStatus status);

        /// <summary>
        /// Gets edit requests by user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of edit requests by the user</returns>
        Task<List<ChallengeEditRequest>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Updates an existing edit request
        /// </summary>
        /// <param name="request">The edit request to update</param>
        /// <returns>The updated edit request</returns>
        Task<ChallengeEditRequest> UpdateAsync(ChallengeEditRequest request);

        /// <summary>
        /// Deletes an edit request
        /// </summary>
        /// <param name="id">The edit request ID to delete</param>
        /// <returns>True if deleted successfully, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Checks if a challenge has any pending edit requests
        /// </summary>
        /// <param name="challengeId">The challenge ID</param>
        /// <returns>True if there are pending requests, false otherwise</returns>
        Task<bool> HasPendingRequestsAsync(Guid challengeId);
    }
}
