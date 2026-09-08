namespace FEA.URVP.Application.Abstractions.Security;

/// <summary>
/// Encrypts and decrypts the SMTP password for storage. Implementations must
/// use authenticated encryption bound to this application.
/// </summary>
public interface ISmtpCredentialProtector
{
    string Protect(string plaintext);

    bool TryUnprotect(string protectedPayload, out string? plaintext);
}
