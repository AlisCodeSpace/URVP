using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Application.Utilities.Files;
using RICHConnect.Backend.Infrastructure.Data;

namespace RICHConnect.Backend.Application.Services.Files
{
    /// <summary>
    /// Factory for creating the database-backed file storage service
    /// Phase 6: Simplified to database-only implementation (legacy file system storage removed)
    /// </summary>
    public class FileStorageFactory
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly ILogger<DatabaseFileUploadService> _logger;
        private readonly IMimeTypeValidator _mimeTypeValidator;
        private readonly IContentHashHelper _contentHashHelper;

        public FileStorageFactory(
            IConfiguration configuration,
            AppDbContext context,
            ILogger<DatabaseFileUploadService> logger,
            IMimeTypeValidator mimeTypeValidator,
            IContentHashHelper contentHashHelper)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _mimeTypeValidator = mimeTypeValidator;
            _contentHashHelper = contentHashHelper;
        }

        /// <summary>
        /// Creates the database-backed file storage service
        /// Phase 6: Always returns DatabaseFileUploadService (migration complete)
        /// </summary>
        /// <returns>DatabaseFileUploadService implementation</returns>
        public IFileUploadService CreateFileStorageService()
        {
            _logger.LogInformation("FileStorageFactory: Using database-backed file storage (Phase 6: DB-only mode)");
            
            return new DatabaseFileUploadService(
                _context, 
                _configuration, 
                _logger, 
                _mimeTypeValidator, 
                _contentHashHelper);
        }
    }
}
