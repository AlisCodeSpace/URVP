using System.Security.Cryptography;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.Extensions.DependencyInjection;

namespace FEA.URVP.Infrastructure.DataProtection;

/// <summary>
/// AES-GCM wrapper for Data Protection key XML so Linux hosts can encrypt keys at rest
/// without a LocalMachine certificate store.
/// </summary>
internal sealed class AesXmlEncryptor : IXmlEncryptor
{
    private readonly DataProtectionEncryptionKey _key;

    public AesXmlEncryptor(DataProtectionEncryptionKey key) => _key = key;

    public EncryptedXmlInfo Encrypt(XElement plaintextElement)
    {
        var plaintext = System.Text.Encoding.UTF8.GetBytes(
            plaintextElement.ToString(SaveOptions.DisableFormatting));
        var nonce = RandomNumberGenerator.GetBytes(12);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[16];

        using (var aes = new AesGcm(_key.Bytes, 16))
        {
            aes.Encrypt(nonce, plaintext, ciphertext, tag);
        }

        var encrypted = new XElement(
            "enc",
            new XElement("n", Convert.ToBase64String(nonce)),
            new XElement("c", Convert.ToBase64String(ciphertext)),
            new XElement("t", Convert.ToBase64String(tag)));

        return new EncryptedXmlInfo(encrypted, typeof(AesXmlDecryptor));
    }
}

public sealed class AesXmlDecryptor : IXmlDecryptor
{
    private readonly DataProtectionEncryptionKey _key;

    public AesXmlDecryptor(IServiceProvider services)
        => _key = services.GetRequiredService<DataProtectionEncryptionKey>();

    public XElement Decrypt(XElement encryptedElement)
    {
        var nonce = Convert.FromBase64String(encryptedElement.Element("n")?.Value ?? string.Empty);
        var ciphertext = Convert.FromBase64String(encryptedElement.Element("c")?.Value ?? string.Empty);
        var tag = Convert.FromBase64String(encryptedElement.Element("t")?.Value ?? string.Empty);
        var plaintext = new byte[ciphertext.Length];

        using (var aes = new AesGcm(_key.Bytes, 16))
        {
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
        }

        return XElement.Parse(System.Text.Encoding.UTF8.GetString(plaintext));
    }
}

internal sealed class DataProtectionEncryptionKey
{
    public DataProtectionEncryptionKey(byte[] bytes) => Bytes = bytes;

    public byte[] Bytes { get; }

    public static DataProtectionEncryptionKey FromSecret(string secret)
        => new(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(secret)));
}
