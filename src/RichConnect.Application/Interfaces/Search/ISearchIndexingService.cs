namespace RICHConnect.Backend.Application.Interfaces.Search
{
    /// <summary>
    /// Service for indexing and managing search data
    /// </summary>
    public interface ISearchIndexingService
    {
        /// <summary>
        /// Index a theme for search
        /// </summary>
        /// <param name="themeId">The theme ID to index</param>
        /// <returns>True if successfully indexed</returns>
        Task<bool> IndexThemeAsync(Guid themeId);

        /// <summary>
        /// Remove a theme from the search index
        /// </summary>
        /// <param name="themeId">The theme ID to remove</param>
        /// <returns>True if successfully removed</returns>
        Task<bool> RemoveThemeFromIndexAsync(Guid themeId);

        /// <summary>
        /// Update a theme in the search index
        /// </summary>
        /// <param name="themeId">The theme ID to update</param>
        /// <returns>True if successfully updated</returns>
        Task<bool> UpdateThemeIndexAsync(Guid themeId);

        /// <summary>
        /// Index a research field for search
        /// </summary>
        /// <param name="researchFieldId">The research field ID to index</param>
        /// <returns>True if successfully indexed</returns>
        Task<bool> IndexResearchFieldAsync(Guid researchFieldId);

        /// <summary>
        /// Remove a research field from the search index
        /// </summary>
        /// <param name="researchFieldId">The research field ID to remove</param>
        /// <returns>True if successfully removed</returns>
        Task<bool> RemoveResearchFieldFromIndexAsync(Guid researchFieldId);

        /// <summary>
        /// Update a research field in the search index
        /// </summary>
        /// <param name="researchFieldId">The research field ID to update</param>
        /// <returns>True if successfully updated</returns>
        Task<bool> UpdateResearchFieldIndexAsync(Guid researchFieldId);

        /// <summary>
        /// Rebuild the entire search index
        /// </summary>
        /// <returns>True if successfully rebuilt</returns>
        Task<bool> RebuildIndexAsync();
    }
}
