using System;
using System.Text;
using System.Security.Cryptography;

public class SelfCrypto
{
    private static readonly string ENCRYPTION_KEY = Environment.GetEnvironmentVariable("ENCRYPTION_KEY") 
        ?? throw new InvalidOperationException("ENCRYPTION_KEY environment variable is not set.");

    /// <summary>
    /// Mã hóa chuỗi text sử dụng AES-256-GCM
    /// Output format: IV:Tag:Ciphertext (Hex encoded)
    /// </summary>
    public static string Encrypt(string plainText)
    {
        byte[] keyBytes = Convert.FromHexString(ENCRYPTION_KEY);
        ValidateKey(keyBytes);

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        
        byte[] iv = new byte[12]; 
        RandomNumberGenerator.Fill(iv);

        byte[] tag = new byte[16]; 
        byte[] cipherBytes = new byte[plainBytes.Length];

        using (var aes = new AesGcm(keyBytes, tag.Length))
        {
            aes.Encrypt(iv, plainBytes, cipherBytes, tag);
        }

        return $"{Convert.ToHexString(iv).ToLower()}:{Convert.ToHexString(tag).ToLower()}:{Convert.ToHexString(cipherBytes).ToLower()}";
    }

    /// <summary>
    /// Giải mã chuỗi hash
    /// Input format: IV:Tag:Ciphertext
    /// </summary>
    public static string? Decrypt(string hash)
    {
        try
        {
            if (string.IsNullOrEmpty(hash)) return null;

            var parts = hash.Split(':');
            if (parts.Length != 3) return null;

            byte[] iv = Convert.FromHexString(parts[0]);
            byte[] tag = Convert.FromHexString(parts[1]);
            byte[] cipherBytes = Convert.FromHexString(parts[2]);
            
            byte[] keyBytes = Convert.FromHexString(ENCRYPTION_KEY);
            ValidateKey(keyBytes);

            byte[] decryptedBytes = new byte[cipherBytes.Length];

            using (var aes = new AesGcm(keyBytes, tag.Length))
            {
                aes.Decrypt(iv, cipherBytes, tag, decryptedBytes);
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static void ValidateKey(byte[] keyBytes)
    {
        if (keyBytes.Length != 32)
        {
            throw new ArgumentException($"Key must be exactly 32 bytes (64 hex characters). Current byte length: {keyBytes.Length}");
        }
    }
}