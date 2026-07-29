using FluentValidation;
using MediatR;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Application.DTOs.Partners;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;
using RICHConnect.Backend.Domain.Entities.Partners;

namespace RICHConnect.Backend.Application.Queries.Partners.GetPartnersByStatus
{
    /// <summary>
    /// Handler for GetPartnersByStatusQuery
    /// </summary>
    public class GetPartnersByStatusQueryHandler : IRequestHandler<GetPartnersByStatusQuery, List<CommunityPartnerDto>>
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly IFileReadService _fileReadService;
        private readonly ILogger<GetPartnersByStatusQueryHandler> _logger;
        private readonly GetPartnersByStatusQueryValidator _validator;

        public GetPartnersByStatusQueryHandler(
            IPartnerRepository partnerRepository,
            IFileReadService fileReadService,
            ILogger<GetPartnersByStatusQueryHandler> logger,
            GetPartnersByStatusQueryValidator validator)
        {
            _partnerRepository = partnerRepository;
            _fileReadService = fileReadService;
            _logger = logger;
            _validator = validator;
        }

        /// <summary>
        /// Handles the query to get community partners by status
        /// </summary>
        public async Task<List<CommunityPartnerDto>> Handle(GetPartnersByStatusQuery query, CancellationToken cancellationToken)
        {
            // Validate query
            var validationResult = await _validator.ValidateAsync(query);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            try
            {
                // Get partners based on status
                List<CommunityPartner> partners;
                if (query.Status.HasValue)
                {
                    partners = await _partnerRepository.GetByStatusAsync(query.Status.Value);
                }
                else
                {
                    partners = await _partnerRepository.GetAllAsync();
                }

                // Apply sorting
                IEnumerable<CommunityPartner> sortedPartners = partners;
                if (!string.IsNullOrEmpty(query.SortBy))
                {
                    sortedPartners = query.SortBy switch
                    {
                        "InstitutionName" => query.SortDescending 
                            ? partners.OrderByDescending(p => p.InstitutionName)
                            : partners.OrderBy(p => p.InstitutionName),
                        "Status" => query.SortDescending 
                            ? partners.OrderByDescending(p => p.Status)
                            : partners.OrderBy(p => p.Status),
                        "CreatedAt" => query.SortDescending 
                            ? partners.OrderByDescending(p => p.CreatedAt)
                            : partners.OrderBy(p => p.CreatedAt),
                        "UpdatedAt" => query.SortDescending 
                            ? partners.OrderByDescending(p => p.UpdatedAt)
                            : partners.OrderBy(p => p.UpdatedAt),
                        _ => query.SortDescending 
                            ? partners.OrderByDescending(p => p.SubmittedAt)
                            : partners.OrderBy(p => p.SubmittedAt)
                    };
                }

                // Apply pagination
                var paginatedPartners = sortedPartners
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize);

                // Map to DTOs
                var result = new List<CommunityPartnerDto>();
                foreach (var p in paginatedPartners)
                {
                    // Get file ID from FileStorage
                    var fileId = await _fileReadService.GetFileIdByEntityAsync("Partner", p.Id, "Logo");
                    var logoUrl = fileId?.ToString();

                    result.Add(new CommunityPartnerDto
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        Email = p.User?.Email ?? string.Empty,
                        InstitutionName = p.InstitutionName,
                        LogoUrl = logoUrl,
                    InstitutionAddress = p.InstitutionAddress,
                    PhoneNumber = p.PhoneNumber,
                    RegistrationNumberArea = p.RegistrationNumberArea,
                    ChamberOfCommerceNumber = p.ChamberOfCommerceNumber,
                    Sector = p.Sector,
                    InstitutionSize = p.InstitutionSize,
                    Vision = p.Vision,
                    Mission = p.Mission,
                    CertificationNumber = p.CertificationNumber,
                    AccreditationType = p.AccreditationType,
                    Status = p.Status,
                    SubmittedAt = p.SubmittedAt,
                    ApprovedAt = p.ApprovedAt,
                    RejectedAt = p.RejectedAt,
                    RejectionReason = p.RejectionReason,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting partners by status {Status}", query.Status);
                throw;
            }
        }
    }
}