using FEA.URVP.Application.DTOs.Matching;
using MediatR;

namespace FEA.URVP.Application.Commands.Matching.Confirm;

/// <summary>Publishes a draft run: its placements become confirmed and occupy project seats.</summary>
public sealed record ConfirmMatchingRunCommand(Guid RunId, Guid CurrentUserId)
    : IRequest<MatchingRunDetailDto>;
