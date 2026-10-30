using FEA.URVP.Application.Features.Matching.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Matching.Commands.Confirm;

/// <summary>Publishes a draft run: its placements become confirmed and occupy project seats.</summary>
public sealed record ConfirmMatchingRunCommand(Guid RunId, Guid CurrentUserId)
    : IRequest<MatchingRunDetailDto>;
