namespace RICHConnect.Backend.Application.Interfaces.Archiving
{
    /// <summary>
    /// Service for archiving deleted or expired data for compliance and audit purposes
    /// </summary>
    public interface IArchivingService
    {
        /// <summary>
        /// Archive a deleted theme
        /// </summary>
        /// <param name="themeId">The theme ID to archive</param>
        /// <param name="deletedBy">The user who deleted it</param>
        /// <param name="deletionReason">The reason for deletion</param>
        /// <returns>True if successfully archived</returns>
        Task<bool> ArchiveDeletedThemeAsync(Guid themeId, Guid deletedBy, string? deletionReason = null);

        /// <summary>
        /// Archive a rejected theme
        /// </summary>
        /// <param name="themeId">The theme ID to archive</param>
        /// <param name="rejectedBy">The user who rejected it</param>
        /// <param name="rejectionReason">The reason for rejection</param>
        /// <returns>True if successfully archived</returns>
        Task<bool> ArchiveRejectedThemeAsync(Guid themeId, Guid rejectedBy, string rejectionReason);

        /// <summary>
        /// Archive a deleted research field
        /// </summary>
        /// <param name="researchFieldId">The research field ID to archive</param>
        /// <param name="deletedBy">The user who deleted it</param>
        /// <param name="deletionReason">The reason for deletion</param>
        /// <returns>True if successfully archived</returns>
        Task<bool> ArchiveDeletedResearchFieldAsync(Guid researchFieldId, Guid deletedBy, string? deletionReason = null);

        /// <summary>
        /// Archive a rejected research field
        /// </summary>
        /// <param name="researchFieldId">The research field ID to archive</param>
        /// <param name="rejectedBy">The user who rejected it</param>
        /// <param name="rejectionReason">The reason for rejection</param>
        /// <returns>True if successfully archived</returns>
        Task<bool> ArchiveRejectedResearchFieldAsync(Guid researchFieldId, Guid rejectedBy, string rejectionReason);

        /// <summary>
        /// Retrieve archived data for audit purposes
        /// </summary>
        /// <param name="entityType">The type of entity (Theme, ResearchField, etc.)</param>
        /// <param name="entityId">The entity ID</param>
        /// <returns>Archived data as JSON string</returns>
        Task<string?> GetArchivedDataAsync(string entityType, Guid entityId);

        /// <summary>
        /// Clean up old archived data based on retention policy
        /// </summary>
        /// <param name="retentionDays">Number of days to retain archived data</param>
        /// <returns>Number of records cleaned up</returns>
        Task<int> CleanupOldArchivedDataAsync(int retentionDays = 2555); // 7 years default
    }
}
