using MediatR;
using RICHConnect.Backend.Application.Commands.Partners.ApprovePartner;
using RICHConnect.Backend.Application.Commands.Partners.RegisterPartner;
using RICHConnect.Backend.Application.Commands.Partners.RejectPartner;
using RICHConnect.Backend.Application.Commands.Partners.UpdatePartner;
using RICHConnect.Backend.Application.Interfaces.Partners;
using RICHConnect.Backend.Application.Queries.Partners.GetPartnerById;
using RICHConnect.Backend.Application.Queries.Partners.GetPartnersByStatus;
using RICHConnect.Backend.Application.Queries.Partners.GetUserPartner;
using RICHConnect.Backend.Application.DTOs.Partners;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Services.Partners
{
    /// <summary>
    /// Application service implementation for community partner operations
    /// </summary>
    public class PartnerApplicationService : IPartnerApplicationService
    {
        private readonly IMediator _mediator;

        public PartnerApplicationService(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <inheritdoc />
        public async Task<CommunityPartnerDto> RegisterPartnerAsync(RegisterPartnerCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <inheritdoc />
        public async Task<CommunityPartnerDto> UpdatePartnerAsync(UpdatePartnerCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <inheritdoc />
        public async Task<bool> ApprovePartnerAsync(ApprovePartnerCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <inheritdoc />
        public async Task<bool> RejectPartnerAsync(RejectPartnerCommand command)
        {
            return await _mediator.Send(command);
        }

        /// <inheritdoc />
        public async Task<CommunityPartnerDto?> GetPartnerByIdAsync(Guid partnerId)
        {
            var query = new GetPartnerByIdQuery { PartnerId = partnerId };
            return await _mediator.Send(query);
        }

        /// <inheritdoc />
        public async Task<CommunityPartnerDto?> GetPartnerByUserIdAsync(Guid userId)
        {
            var query = new GetUserPartnerQuery { UserId = userId };
            return await _mediator.Send<CommunityPartnerDto?>(query);
        }

        /// <inheritdoc />
        public async Task<List<CommunityPartnerDto>> GetPartnersByStatusAsync(
            ApprovalStatus? status = null, 
            int pageNumber = 1, 
            int pageSize = 50, 
            string? sortBy = "SubmittedAt", 
            bool sortDescending = true)
        {
            var query = new GetPartnersByStatusQuery
            {
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending
            };
            return await _mediator.Send<List<CommunityPartnerDto>>(query);
        }
    }
}