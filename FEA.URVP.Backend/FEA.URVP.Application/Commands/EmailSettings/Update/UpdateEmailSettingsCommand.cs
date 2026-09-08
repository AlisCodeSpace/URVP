using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.Email;
using MediatR;

namespace FEA.URVP.Application.Commands.Email.Update;

public sealed class UpdateEmailSettingsCommand : IRequest<EmailSettingsDto>
{
    [JsonIgnore]
    public Guid UpdatedByUserId { get; set; }

    public string? UserName { get; init; }

    /// <summary>
    /// New SMTP password. Null or whitespace leaves the stored password unchanged.
    /// </summary>
    public string? Password { get; init; }

    /// <summary>When true, removes the stored password. Mutually exclusive with <see cref="Password"/>.</summary>
    public bool ClearPassword { get; init; }
}
