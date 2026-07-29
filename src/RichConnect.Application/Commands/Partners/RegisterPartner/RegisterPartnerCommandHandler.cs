using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.DTOs.Partners;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Partners;
using RICHConnect.Backend.Application.Services.Partners;

namespace RICHConnect.Backend.Application.Commands.Partners.RegisterPartner
{
    /// <summary>
    /// Handler for RegisterPartnerCommand
    /// Phase 6: Updated to use unified DatabaseFileUploadService instead of ILogoUploadService
    /// </summary>
    public class RegisterPartnerCommandHandler : BaseCommandHandler<RegisterPartnerCommand, CommunityPartnerDto>
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly IFileUploadService _fileUploadService;
        private readonly IFileReadService _fileReadService;
        private readonly IEventBus _eventBus;
        private readonly PartnerBusinessRulesService _businessRulesService;
        private PartnerRegisteredEvent? _pendingDomainEvent;

        public RegisterPartnerCommandHandler(
            IPartnerRepository partnerRepository,
            IFileUploadService fileUploadService,
            IFileReadService fileReadService,
            IEventBus eventBus,
            PartnerBusinessRulesService businessRulesService,
            AppDbContext context,
            ILogger<RegisterPartnerCommandHandler> logger)
            : base(logger, context)
        {
            _partnerRepository = partnerRepository;
            _fileUploadService = fileUploadService;
            _fileReadService = fileReadService;
            _eventBus = eventBus;
            _businessRulesService = businessRulesService;
        }

        protected override bool UseTransaction => true;

        /// <summary>
        /// Handles the command to register a new community partner
        /// </summary>
        protected override async Task<CommunityPartnerDto> HandleInternal(RegisterPartnerCommand command, CancellationToken cancellationToken)
        {
            _pendingDomainEvent = null;
            // Check for duplicate institution name
            var isDuplicate = await _businessRulesService.CheckDuplicateInstitutionAsync(command.InstitutionName, null);
            if (isDuplicate)
            {
                throw new InvalidOperationException($"An institution with the name '{command.InstitutionName}' already exists.");
            }

            // Create partner first to get the ID
            var partner = new CommunityPartner
            {
                UserId = command.UserId,
                InstitutionName = command.InstitutionName.Trim(),
                // Note: LogoUrl is obsolete - file is stored in FileStorage table
                InstitutionAddress = command.InstitutionAddress?.Trim(),
                PhoneNumber = command.PhoneNumber?.Trim(),
                RegistrationNumberArea = command.RegistrationNumberArea?.Trim(),
                ChamberOfCommerceNumber = command.ChamberOfCommerceNumber?.Trim(),
                Sector = command.Sector,
                InstitutionSize = command.InstitutionSize,
                Vision = command.Vision?.Trim(),
                Mission = command.Mission?.Trim(),
                CertificationNumber = command.CertificationNumber?.Trim(),
                AccreditationType = command.AccreditationType,
                Status = ApprovalStatus.Pending,
                SubmittedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save to database to get partner ID
            var savedPartner = await _partnerRepository.AddAsync(partner);

            // Handle logo upload if provided (Phase 6: Using unified file upload service)
            if (command.Logo != null && command.Logo.Length > 0)
            {
                await _fileUploadService.UploadFileAsync(
                    command.Logo, 
                    "Partner", 
                    savedPartner.Id, 
                    "Logo",
                    command.UserId);
                
                // Note: LogoUrl is obsolete - file is stored in FileStorage table
                // Don't set obsolete property - file is managed via FileStorage
            }

            // Get user email for the event
            string userEmail = savedPartner.User?.Email ?? "unknown@example.com";

            // Queue event to be published after transaction commit
            _pendingDomainEvent = new PartnerRegisteredEvent(
                savedPartner.Id,
                savedPartner.UserId,
                savedPartner.InstitutionName,
                userEmail);

            // Get file ID from FileStorage
            var fileId = await _fileReadService.GetFileIdByEntityAsync("Partner", savedPartner.Id, "Logo");
            var logoUrl = fileId?.ToString();

            // Map to DTO and return
            return new CommunityPartnerDto
            {
                Id = savedPartner.Id,
                UserId = savedPartner.UserId,
                Email = userEmail,
                InstitutionName = savedPartner.InstitutionName,
                LogoUrl = logoUrl,
                InstitutionAddress = savedPartner.InstitutionAddress,
                PhoneNumber = savedPartner.PhoneNumber,
                RegistrationNumberArea = savedPartner.RegistrationNumberArea,
                ChamberOfCommerceNumber = savedPartner.ChamberOfCommerceNumber,
                Sector = savedPartner.Sector,
                InstitutionSize = savedPartner.InstitutionSize,
                Vision = savedPartner.Vision,
                Mission = savedPartner.Mission,
                CertificationNumber = savedPartner.CertificationNumber,
                AccreditationType = savedPartner.AccreditationType,
                Status = savedPartner.Status,
                SubmittedAt = savedPartner.SubmittedAt,
                ApprovedAt = savedPartner.ApprovedAt,
                RejectedAt = savedPartner.RejectedAt,
                RejectionReason = savedPartner.RejectionReason,
                CreatedAt = savedPartner.CreatedAt,
                UpdatedAt = savedPartner.UpdatedAt
            };
        }

        public override async Task<CommunityPartnerDto> Handle(RegisterPartnerCommand request, CancellationToken cancellationToken)
        {
            _pendingDomainEvent = null;
            try
            {
                var response = await base.Handle(request, cancellationToken);
                return response;
            }
            finally
            {
                if (_pendingDomainEvent != null)
                {
                    try
                    {
                        await _eventBus.PublishAsync(_pendingDomainEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish PartnerRegisteredEvent for partner {PartnerId}", _pendingDomainEvent.PartnerId);
                    }
                    finally
                    {
                        _pendingDomainEvent = null;
                    }
                }
            }
        }
    }
}