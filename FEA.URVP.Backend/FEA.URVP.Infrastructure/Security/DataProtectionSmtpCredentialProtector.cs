using System.Security.Cryptography;
using FEA.URVP.Application.Abstractions.Security;
using Microsoft.AspNetCore.DataProtection;

namespace FEA.URVP.Infrastructure.Security;

/// <summary>
/// Protects the SMTP password with ASP.NET Data Protection (AES + HMAC,
/// keys persisted in DataProtectionKeys). Purpose-bound so the payload
/// cannot be swapped into another protector.
/// </summary>
public sealed class DataProtectionSmtpCredentialProtector : ISmtpCredentialProtector
{
    internal const string Purpose = "FEA.URVP.Email.SmtpPassword.v1";

    private readonly IDataProtector _protector;

    public DataProtectionSmtpCredentialProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector(Purpose);
    }

    public string Protect(string plaintext)
    {
        ArgumentException.ThrowIfNullOrEmpty(plaintext);
        return _protector.Protect(plaintext);
    }

    public bool TryUnprotect(string protectedPayload, out string? plaintext)
    {
        plaintext = null;
        if (string.IsNullOrEmpty(protectedPayload))
        {
            return false;
        }

        try
        {
            plaintext = _protector.Unprotect(protectedPayload);
            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}
