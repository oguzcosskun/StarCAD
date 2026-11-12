namespace StarCAD.Core.Security;

/// <summary>
/// TC kimlik numarasının KVKK uyumlu korunması. Şifreleme geri döndürülebilir (maskeli gösterim
/// için çözme gerekir); ayrıca deterministik hash benzersizlik (UNIQUE) kontrolü içindir.
/// Uygulama impl'i (AES-GCM + HMAC) DataAccess/Business katmanında; Core sadece sözleşmeyi tutar.
/// </summary>
public interface INationalIdProtector
{
    /// <summary>Düz TC'yi şifreler (nonce ‖ ciphertext ‖ tag paketi döner).</summary>
    byte[] Encrypt(string nationalId);

    /// <summary>Şifreli paketi çözüp düz TC'yi döner.</summary>
    string Decrypt(byte[] data);

    /// <summary>Deterministik HMAC — aynı TC hep aynı hash (UNIQUE index için).</summary>
    byte[] Hash(string nationalId);

    /// <summary>Düz TC'yi maskeler: son 4 hane görünür (ör. "*******1234").</summary>
    string Mask(string nationalId);
}
