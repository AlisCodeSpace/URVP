using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.Semesters;
using MediatR;

namespace FEA.URVP.Application.Commands.Semesters.SetRegistrationWindow;

/// <summary>
/// Opens or closes the registration window for a semester.
/// Pass null for both dates to clear the window. An omitted end date
/// keeps the window open until it is closed instantly or an end is set.
/// </summary>
public sealed class SetRegistrationWindowCommand : IRequest<SemesterDto>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    /// <summary>UTC start. Pass null to clear the window.</summary>
    public DateTime? RegistrationWindowStart { get; init; }

    /// <summary>UTC end. Pass null to leave the window open until it is closed.</summary>
    public DateTime? RegistrationWindowEnd { get; init; }
}
