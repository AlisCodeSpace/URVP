using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;
using RICHConnect.Backend.Infrastructure.Events;

namespace RICHConnect.Backend.Application.Commands.ResearchFields.DeleteField
{
    public class DeleteFieldCommandHandler : BaseCommandHandler<DeleteFieldCommand, bool>
    {
        private readonly IResearchFieldRepository _repository;
        private readonly IEventBus _eventBus;
        private readonly IFileUploadService _fileUploadService;
        private readonly IFileReadService _fileReadService;

        public DeleteFieldCommandHandler(
            ILogger<DeleteFieldCommandHandler> logger,
            AppDbContext context,
            IResearchFieldRepository repository,
            IEventBus eventBus,
            IFileUploadService fileUploadService,
            IFileReadService fileReadService)
            : base(logger, context)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _fileReadService = fileReadService ?? throw new ArgumentNullException(nameof(fileReadService));
        }

        protected override async Task<bool> HandleInternal(DeleteFieldCommand request, CancellationToken cancellationToken)
        {
            // Get the field to delete (validation already handled by base class)
            var field = await _repository.GetByIdAsync(request.FieldId);
            if (field == null)
            {
                throw new InvalidOperationException($"Research field with ID {request.FieldId} not found.");
            }
            
            // Get file IDs from FileStorage
            var imageFileId = await _fileReadService.GetFileIdByEntityAsync("ResearchField", field.Id, "Image");
            var documentFileId = await _fileReadService.GetFileIdByEntityAsync("ResearchField", field.Id, "Document");
            
            // Soft delete associated files from FileStorage if they exist
            if (imageFileId.HasValue)
            {
                await _fileUploadService.DeleteFileAsync(imageFileId.Value.ToString());
            }
            
            if (documentFileId.HasValue)
            {
                await _fileUploadService.DeleteFileAsync(documentFileId.Value.ToString());
            }
            
            // Delete the field from the database
            var deleted = await _repository.DeleteAsync(request.FieldId);
            if (!deleted)
            {
                return false;
            }
            
            // Publish domain event
            await _eventBus.PublishAsync(new ResearchFieldDeletedEvent(
                request.FieldId,
                request.DeletedBy
            ));
            
            return true;
        }
    }
}

