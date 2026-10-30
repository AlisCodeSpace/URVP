using System.Text.Json.Serialization;
using FEA.URVP.Application.Features.Matching.DTOs;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Features.Matching.Commands.UpdatePlacementStatus;

/// <summary>
/// Releases a confirmed seat when a student declines or an admin withdraws a placement.
/// Released students become eligible for a supplementary run.
/// </summary>
public sealed class UpdatePlacementStatusCommand : IRequest<PlacementDto>
{
    [JsonIgnore]
    public Guid PlacementId { get; set; }

    public PlacementStatus Status { get; init; }
}
