using FEA.URVP.Application.Features.Email.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Email.Queries;

public sealed record GetEmailSettingsQuery : IRequest<EmailSettingsDto>;
