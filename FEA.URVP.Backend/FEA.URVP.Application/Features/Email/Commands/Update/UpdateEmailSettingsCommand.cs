using System.Text.Json.Serialization;
using FEA.URVP.Application.Features.Email.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Email.Commands.Update;

public sealed class UpdateEmailSettingsCommand : IRequest<EmailSettingsDto>
{
    [JsonIgnore]
    public Guid UpdatedByUserId { get; set; }

    /// <summary>
    /// New SMTP password. Null or whitespace leaves the stored password unchanged.
    /// </summary>
    public string? Password { get; init; }

    /// <summary>When true, removes the stored password. Mutually exclusive with <see cref="Password"/>.</summary>
    public bool ClearPassword { get; init; }
}
