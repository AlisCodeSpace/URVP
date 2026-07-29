using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.DTOs.Partners;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Application.Services.Partners;

namespace RICHConnect.Backend.Application.Commands.Partners.UpdatePartner
{
    /// <summary>
    /// Handler for UpdatePartnerCommand
    /// Phase 6: Updated to use unified DatabaseFileUploadService instead of ILogoUploadService
    /// </summary>
    public class UpdatePartnerCommandHandler : BaseCommandHandler<UpdatePartnerCommand, CommunityPartnerDto>
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly IFileUploadService _fileUploadService;
        private readonly IFileReadService _fileReadService;
        private readonly IEventBus _eventBus;
        private readonly PartnerBusinessRulesService _businessRulesService;
        
        public UpdatePartnerCommandHandler(
            IPartnerRepository partnerRepository,
            IFileUploadService fileUploadService,
            IFileReadService fileReadService,
            IEventBus eventBus,
            PartnerBusinessRulesService businessRulesService,
            AppDbContext context,
            ILogger<UpdatePartnerCommandHandler> logger)
            : base(logger, context)
        {
            _partnerRepository = partnerRepository;
            _fileUploadService = fileUploadService;
            _fileReadService = fileReadService;
            _eventBus = eventBus;
            _businessRulesService = businessRulesService;
        }

        /// <summary>
        /// Handles the command to update a community partner profile
        /// </summary>
        protected override async Task<CommunityPartnerDto> HandleInternal(UpdatePartnerCommand command, CancellationToken cancellationToken)
        {
            // Validation is handled by ValidationBehavior and UpdatePartnerCommandValidator
            var partner = await _partnerRepository.GetByUserIdAsync(command.UserId);
            if (partner == null)
            {
                throw new InvalidOperationException($"Partner with User ID {command.UserId} not found.");
            }

            // Check for duplicate institution name if changing
            if (!string.IsNullOrWhiteSpace(command.InstitutionName) && command.InstitutionName != partner.InstitutionName)
            {
                var isDuplicate = await _businessRulesService.CheckDuplicateInstitutionAsync(command.InstitutionName, partner.Id);
                if (isDuplicate)
                {
                    throw new InvalidOperationException($"An institution with the name '{command.InstitutionName}' already exists.");
                }
            }

            // Track changed fields for event
            var changedFields = new Dictionary<string, object>();

            // Handle logo upload if provided (Phase 6: Using unified file upload service)
            if (command.Logo != null && command.Logo.Length > 0)
            {
                // Delete old logo if exists (soft delete from FileStorage)
                var oldFileId = await _fileReadService.GetFileIdByEntityAsync("Partner", partner.Id, "Logo");
                if (oldFileId.HasValue)
                {
                    await _fileUploadService.DeleteFileAsync(oldFileId.Value.ToString());
                }

                // Upload new logo to FileStorage
                var fileId = await _fileUploadService.UploadFileAsync(
                    command.Logo, 
                    "Partner", 
                    partner.Id, 
                    "Logo",
                    command.UserId);
                
                // Note: LogoUrl is obsolete - file is stored in FileStorage table
                // Don't set obsolete property - file is managed via FileStorage
                changedFields.Add("Logo", fileId);
            }

            // Update other fields
            if (!string.IsNullOrWhiteSpace(command.InstitutionName) && command.InstitutionName != partner.InstitutionName)
            {
                partner.InstitutionName = command.InstitutionName.Trim();
                changedFields.Add("InstitutionName", partner.InstitutionName);
            }

            if (command.InstitutionAddress != null && command.InstitutionAddress != partner.InstitutionAddress)
            {
                partner.InstitutionAddress = command.InstitutionAddress.Trim();
                changedFields.Add("InstitutionAddress", partner.InstitutionAddress ?? string.Empty);
            }

            if (command.PhoneNumber != null && command.PhoneNumber != partner.PhoneNumber)
            {
                partner.PhoneNumber = command.PhoneNumber.Trim();
                changedFields.Add("PhoneNumber", partner.PhoneNumber ?? string.Empty);
            }

            if (command.RegistrationNumberArea != null && command.RegistrationNumberArea != partner.RegistrationNumberArea)
            {
                partner.RegistrationNumberArea = command.RegistrationNumberArea.Trim();
                changedFields.Add("RegistrationNumberArea", partner.RegistrationNumberArea ?? string.Empty);
            }

            if (command.ChamberOfCommerceNumber != null && command.ChamberOfCommerceNumber != partner.ChamberOfCommerceNumber)
            {
                partner.ChamberOfCommerceNumber = command.ChamberOfCommerceNumber.Trim();
                changedFields.Add("ChamberOfCommerceNumber", partner.ChamberOfCommerceNumber ?? string.Empty);
            }

            if (command.Sector.HasValue && command.Sector != partner.Sector)
            {
                partner.Sector = command.Sector;
                changedFields.Add("Sector", partner.Sector?.ToString() ?? string.Empty);
            }

            if (command.InstitutionSize.HasValue && command.InstitutionSize != partner.InstitutionSize)
            {
                partner.InstitutionSize = command.InstitutionSize;
                changedFields.Add("InstitutionSize", partner.InstitutionSize?.ToString() ?? string.Empty);
            }

            if (command.Vision != null && command.Vision != partner.Vision)
            {
                partner.Vision = command.Vision.Trim();
                changedFields.Add("Vision", partner.Vision ?? string.Empty);
            }

            if (command.Mission != null && command.Mission != partner.Mission)
            {
                partner.Mission = command.Mission.Trim();
                changedFields.Add("Mission", partner.Mission ?? string.Empty);
            }

            if (command.CertificationNumber != null && command.CertificationNumber != partner.CertificationNumber)
            {
                partner.CertificationNumber = command.CertificationNumber.Trim();
                changedFields.Add("CertificationNumber", partner.CertificationNumber ?? string.Empty);
            }

            if (command.AccreditationType.HasValue && command.AccreditationType != partner.AccreditationType)
            {
                partner.AccreditationType = command.AccreditationType;
                changedFields.Add("AccreditationType", partner.AccreditationType?.ToString() ?? string.Empty);
            }

            // Only update if there are changes
            if (changedFields.Count > 0)
            {
                partner.UpdatedAt = DateTime.UtcNow;
                changedFields.Add("UpdatedAt", partner.UpdatedAt);

                // Save changes
                await _partnerRepository.UpdateAsync(partner);

                // Publish event if there are changes
                var partnerUpdatedEvent = new PartnerUpdatedEvent(
                    partner.Id,
                    command.UserId,
                    changedFields);

                await _eventBus.PublishAsync(partnerUpdatedEvent);
            }

            // Get user email for the response
            string userEmail = partner.User?.Email ?? "unknown@example.com";

            // Get file ID from FileStorage
            var logoFileId = await _fileReadService.GetFileIdByEntityAsync("Partner", partner.Id, "Logo");
            var logoUrl = logoFileId?.ToString();

            // Map to DTO and return
            return new CommunityPartnerDto
            {
                Id = partner.Id,
                UserId = partner.UserId,
                Email = userEmail,
                InstitutionName = partner.InstitutionName,
                LogoUrl = logoUrl,
                InstitutionAddress = partner.InstitutionAddress,
                PhoneNumber = partner.PhoneNumber,
                RegistrationNumberArea = partner.RegistrationNumberArea,
                ChamberOfCommerceNumber = partner.ChamberOfCommerceNumber,
                Sector = partner.Sector,
                InstitutionSize = partner.InstitutionSize,
                Vision = partner.Vision,
                Mission = partner.Mission,
                CertificationNumber = partner.CertificationNumber,
                AccreditationType = partner.AccreditationType,
                Status = partner.Status,
                SubmittedAt = partner.SubmittedAt,
                ApprovedAt = partner.ApprovedAt,
                RejectedAt = partner.RejectedAt,
                RejectionReason = partner.RejectionReason,
                CreatedAt = partner.CreatedAt,
                UpdatedAt = partner.UpdatedAt
            };
        }
    }
}