using FEA.URVP.Application.Features.Email.DTOs;
using FEA.URVP.Domain.Entities.Notifications;

namespace FEA.URVP.Application.Features.Email.Mappings;

public static class EmailLogMappings
{
    public static EmailLogDto ToDto(this EmailLog log) => new()
    {
        Id = log.Id,
        From = log.From,
        To = log.To,
        Cc = log.Cc,
        Bcc = log.Bcc,
        Exception = log.Exception,
        Success = log.Success,
        CreatedOn = log.CreatedOn,
    };
}
