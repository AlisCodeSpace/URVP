using FluentValidation;
using MediatR;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Application.DTOs.Partners;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;

namespace RICHConnect.Backend.Application.Queries.Partners.GetUserPartner
{
    /// <summary>
    /// Handler for GetUserPartnerQuery
    /// </summary>
    public class GetUserPartnerQueryHandler : IRequestHandler<GetUserPartnerQuery, CommunityPartnerDto?>
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly IFileReadService _fileReadService;
        private readonly ILogger<GetUserPartnerQueryHandler> _logger;
        private readonly GetUserPartnerQueryValidator _validator;

        public GetUserPartnerQueryHandler(
            IPartnerRepository partnerRepository,
            IFileReadService fileReadService,
            ILogger<GetUserPartnerQueryHandler> logger,
            GetUserPartnerQueryValidator validator)
        {
            _partnerRepository = partnerRepository;
            _fileReadService = fileReadService;
            _logger = logger;
            _validator = validator;
        }

        /// <summary>
        /// Handles the query to get a community partner by user ID
        /// </summary>
        public async Task<CommunityPartnerDto?> Handle(GetUserPartnerQuery query, CancellationToken cancellationToken)
        {
            // Validate query
            var validationResult = await _validator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            try
            {
                // Get partner
                var partner = await _partnerRepository.GetByUserIdAsync(query.UserId);
                if (partner == null)
                {
                    return null;
                }

                // Map to DTO
                // Get file ID from FileStorage
                var fileId = await _fileReadService.GetFileIdByEntityAsync("Partner", partner.Id, "Logo");
                var logoUrl = fileId?.ToString();

                return new CommunityPartnerDto
                {
                    Id = partner.Id,
                    UserId = partner.UserId,
                    Email = partner.User?.Email ?? string.Empty,
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting partner for user {UserId}", query.UserId);
                throw;
            }
        }
    }
}