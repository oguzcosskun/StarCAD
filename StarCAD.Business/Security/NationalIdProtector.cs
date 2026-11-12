using System.Security.Cryptography;
using System.Text;
using StarCAD.Core.Security;

namespace StarCAD.Business.Security;

/// <summary>
/// TC kimlik koruması: AES-GCM ile geri döndürülebilir şifreleme + HMAC-SHA256 ile deterministik
/// hash (benzersizlik). Anahtarlar config'ten base64 verilir (bkz. ServiceRegistration).
/// Saklanan şifreli paket düzeni: [12 byte nonce][ciphertext][16 byte tag].
/// </summary>
public sealed class NationalIdProtector : INationalIdProtector
{
    private const int NonceSize = 12;   // AES-GCM standart nonce
    private const int TagSize = 16;     // AES-GCM standart tag

    private readonly byte[] _encryptionKey;   // AES-256 → 32 byte
    private readonly byte[] _hmacKey;

    public NationalIdProtector(byte[] encryptionKey, byte[] hmacKey)
    {
        if (encryptionKey.Length != 32)
            throw new ArgumentException("Encryption key 32 byte (AES-256) olmalı.", nameof(encryptionKey));

        if (hmacKey.Length < 32)
            throw new ArgumentException("HMAC key en az 32 byte olmalı.", nameof(hmacKey));

        _encryptionKey = encryptionKey;
        _hmacKey = hmacKey;
    }

    public byte[] Encrypt(string nationalId)
    {
        var plain = Encoding.UTF8.GetBytes(nationalId);

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[plain.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_encryptionKey, TagSize);
        aes.Encrypt(nonce, plain, cipher, tag);

        // [nonce][ciphertext][tag]
        var result = new byte[NonceSize + cipher.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(cipher, 0, result, NonceSize, cipher.Length);
        Buffer.BlockCopy(tag, 0, result, NonceSize + cipher.Length, TagSize);
        return result;
    }

    public string Decrypt(byte[] data)
    {
        if (data.Length < NonceSize + TagSize)
            throw new ArgumentException("Şifreli veri geçersiz uzunlukta.", nameof(data));

        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        var cipher = new byte[data.Length - NonceSize - TagSize];

        Buffer.BlockCopy(data, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(data, NonceSize, cipher, 0, cipher.Length);
        Buffer.BlockCopy(data, NonceSize + cipher.Length, tag, 0, TagSize);

        var plain = new byte[cipher.Length];
        using var aes = new AesGcm(_encryptionKey, TagSize);
        aes.Decrypt(nonce, cipher, tag, plain);

        return Encoding.UTF8.GetString(plain);
    }

    public byte[] Hash(string nationalId)
    {
        // Deterministik: aynı TC → aynı hash → UNIQUE index çalışır.
        return HMACSHA256.HashData(_hmacKey, Encoding.UTF8.GetBytes(nationalId));
    }

    public string Mask(string nationalId)
    {
        if (string.IsNullOrEmpty(nationalId) || nationalId.Length <= 4)
            return nationalId;

        return new string('*', nationalId.Length - 4) + nationalId[^4..];
    }
}
