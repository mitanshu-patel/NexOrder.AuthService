namespace NexOrder.AuthService.Shared.EncryptionDecryption;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

public class EncryptionDecryptionService : IEncryptionDecryptionService
{
    private readonly string encryptKey;

    private readonly ILogger<EncryptionDecryptionService> logger;

    public EncryptionDecryptionService(IOptions<EncryptionDecryptionServiceOptions> options, ILogger<EncryptionDecryptionService> logger)
    {
        this.encryptKey = options.Value.EncryptionKey;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public string DecryptSecret(string value)
    {
        var fullCipher = Convert.FromBase64String(value);

        var iv = new byte[16];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, fullCipher.Length - iv.Length);

        var key = Encoding.UTF8.GetBytes(this.encryptKey);

        using (var aesAlg = Aes.Create())
        {
            using (var decryptor = aesAlg.CreateDecryptor(key, iv))
            {
                string result;
                using (var msDecrypt = new MemoryStream(cipher))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            result = srDecrypt.ReadToEnd();
                        }
                    }
                }

                return result;
            }
        }
    }

    public bool TryDecryptValue(string value, out string? decryptedValue)
    {
        try
        {
            decryptedValue = this.DecryptSecret(value);
            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError("Failed to decrypt value.Exception: {exceptionMessage}", ex.Message);
            decryptedValue = null;
            return false;
        }
    }

    public string EncryptSecret(string value)
    {
        var key = Encoding.UTF8.GetBytes(this.encryptKey);

        using (var aesAlg = Aes.Create())
        {
            using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
            {
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(value);
                    }

                    var iv = aesAlg.IV;

                    var decryptedContent = msEncrypt.ToArray();

                    var result = new byte[iv.Length + decryptedContent.Length];

                    Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                    Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                    return Convert.ToBase64String(result);
                }
            }
        }
    }
}
