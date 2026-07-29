namespace RICHConnect.Backend.Application.Interfaces.ResearchFields
{
    /// <summary>
    /// Service for managing the research field catalog and related operations
    /// </summary>
    public interface IResearchFieldCatalogService
    {
        /// <summary>
        /// Add a research field to the catalog
        /// </summary>
        /// <param name="researchFieldId">The research field ID</param>
        /// <returns>True if successfully added</returns>
        Task<bool> AddToCatalogAsync(Guid researchFieldId);

        /// <summary>
        /// Remove a research field from the catalog
        /// </summary>
        /// <param name="researchFieldId">The research field ID</param>
        /// <returns>True if successfully removed</returns>
        Task<bool> RemoveFromCatalogAsync(Guid researchFieldId);

        /// <summary>
        /// Update a research field in the catalog
        /// </summary>
        /// <param name="researchFieldId">The research field ID</param>
        /// <returns>True if successfully updated</returns>
        Task<bool> UpdateCatalogEntryAsync(Guid researchFieldId);

        /// <summary>
        /// Get catalog statistics
        /// </summary>
        /// <returns>Dictionary of statistics</returns>
        Task<Dictionary<string, int>> GetCatalogStatisticsAsync();

        /// <summary>
        /// Rebuild the entire catalog
        /// </summary>
        /// <returns>True if successfully rebuilt</returns>
        Task<bool> RebuildCatalogAsync();
    }
}
