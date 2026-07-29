namespace RICHConnect.Backend.Api.Extensions
{
    /// <summary>
    /// Extension methods for IWebHostEnvironment
    /// </summary>
    public static class WebHostEnvironmentExtensions
    {
        /// <summary>
        /// Check if the current environment is staging
        /// </summary>
        public static bool IsStaging(this IWebHostEnvironment environment)
        {
            return environment.EnvironmentName.Equals("Staging", StringComparison.OrdinalIgnoreCase);
        }
    }
}
