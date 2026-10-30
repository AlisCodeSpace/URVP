using FEA.URVP.Application.DTOs.Email;
using MediatR;

namespace FEA.URVP.Application.Queries.Email;

public sealed record GetEmailSettingsQuery : IRequest<EmailSettingsDto>;
