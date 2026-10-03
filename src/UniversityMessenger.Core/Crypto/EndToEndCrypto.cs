using System.Security.Cryptography;
using System.Text;

namespace UniversityMessenger.Core.Crypto;

/// <summary>
/// Оконечное шифрование на стороне клиента.
/// Сервер не видит приватных ключей и открытых текстов никогда.
/// </summary>
public static class EndToEndCrypto
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    /// <summary>
    /// Пара ключей клиента. PrivateKeyBase64 не должен покидать устройство.
    /// </summary>
    public record KeyPair(string PrivateKeyBase64, string PublicKeyBase64);

    public static KeyPair CreateKeyPair()
    {
        using var ecdh = ECDiffieHellman.Create();
        return new KeyPair(
            Convert.ToBase64String(ecdh.ExportECPrivateKey()),
            Convert.ToBase64String(ecdh.ExportSubjectPublicKeyInfo()));
    }

    /// <summary>
    /// Вывод общего секрета: ECDH(мой приватный, публичный собеседника).
    /// </summary>
    public static byte[] DeriveSharedKey(string myPrivateKeyBase64, string otherPublicKeyBase64)
    {
        using var mine = ECDiffieHellman.Create();
        mine.ImportECPrivateKey(Convert.FromBase64String(myPrivateKeyBase64), out _);

        using var other = ECDiffieHellman.Create();
        other.ImportSubjectPublicKeyInfo(Convert.FromBase64String(otherPublicKeyBase64), out _);

        return mine.DeriveKeyFromHash(other.PublicKey, HashAlgorithmName.SHA256);
    }

    /// <summary>
    /// Шифрование текста общим ключом. Результат: base64(nonce + tag + шифротекст).
    /// </summary>
    public static string Encrypt(string plaintext, byte[] key)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var pt = Encoding.UTF8.GetBytes(plaintext);
        var ct = new byte[pt.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, pt, ct, tag);

        var packed = new byte[NonceSize + TagSize + ct.Length];
        nonce.CopyTo(packed, 0);
        tag.CopyTo(packed, NonceSize);
        ct.CopyTo(packed, NonceSize + TagSize);
        return Convert.ToBase64String(packed);
    }

    /// <summary>
    /// Расшифровка. Бросает CryptographicException при подмене шифротекста.
    /// </summary>
    public static string Decrypt(string packedBase64, byte[] key)
    {
        var packed = Convert.FromBase64String(packedBase64);

        var nonce = packed.AsSpan(0, NonceSize).ToArray();
        var tag = packed.AsSpan(NonceSize, TagSize).ToArray();
        var ct = packed.AsSpan(NonceSize + TagSize).ToArray();
        var pt = new byte[ct.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, ct, tag, pt);
        return Encoding.UTF8.GetString(pt);
    }

    /// <summary>
    /// Конверт для группового ключа: шифруем ключ группы парным секретом
    /// создателя и конкретного участника. Открыть может только адресат.
    /// </summary>
    public static string WrapKey(byte[] sharedKey, byte[] keyToWrap)
    {
        return Encrypt(Convert.ToBase64String(keyToWrap), sharedKey);
    }

    /// <summary>
    /// Вскрытие конверта: адресат получает ключ группы обратно.
    /// </summary>
    public static byte[] UnwrapKey(byte[] sharedKey, string wrappedBase64)
    {
        return Convert.FromBase64String(Decrypt(wrappedBase64, sharedKey));
    }

    /// <summary>
    /// Отпечаток публичного ключа для сверки глазами между пользователями.
    /// </summary>
    public static string GetFingerprint(string publicKeyBase64)
    {
        var hash = SHA256.HashData(Convert.FromBase64String(publicKeyBase64));
        return Convert.ToHexString(hash.AsSpan(0, 8));
    }
}
