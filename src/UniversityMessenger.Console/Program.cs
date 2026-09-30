using UniversityMessenger.Core.Crypto;

Console.WriteLine("=== Демонстрация оконечного шифрования: слепой сервер ===");
Console.WriteLine();

// 1. Клиенты создают пары ключей. Приватные не покидают устройства.
var alice = EndToEndCrypto.CreateKeyPair();
var bob = EndToEndCrypto.CreateKeyPair();

Console.WriteLine("Алиса и Боб создали пары ключей на устройствах.");
Console.WriteLine($"Публичный ключ Алисы (виден серверу): {alice.PublicKeyBase64[..24]}...");
Console.WriteLine($"Публичный ключ Боба (виден серверу): {bob.PublicKeyBase64[..24]}...");
Console.WriteLine();

// 2. Обмен публичными ключами через сервер и вывод общего секрета.
var aliceKey = EndToEndCrypto.DeriveSharedKey(alice.PrivateKeyBase64, bob.PublicKeyBase64);
var bobKey = EndToEndCrypto.DeriveSharedKey(bob.PrivateKeyBase64, alice.PublicKeyBase64);

Console.WriteLine($"Секреты сторон совпали без передачи по сети: {Convert.ToHexString(aliceKey) == Convert.ToHexString(bobKey)}");
Console.WriteLine();

// 3. Алиса шифрует сообщение. На сервер уезжает только шифротекст.
var plaintext = "Отчёт по проекту готов, посмотри вечером.";
var packed = EndToEndCrypto.Encrypt(plaintext, aliceKey);

Console.WriteLine($"Открытый текст Алисы: {plaintext}");
Console.WriteLine($"На сервер ушло: {packed}");
Console.WriteLine();

// 4. Дамп сервера: что увидит злоумышленник при изъятии.
Console.WriteLine("ДАМП СЕРВЕРА (взгляд злоумышленника):");
Console.WriteLine($"  запись 1: {packed}");
Console.WriteLine("  Ключей нет. Открытых текстов нет. Расшифровка невозможна.");
Console.WriteLine();

// 5. Боб получает шифротекст и расшифровывает на своём устройстве.
var decrypted = EndToEndCrypto.Decrypt(packed, bobKey);
Console.WriteLine($"Боб расшифровал у себя: {decrypted}");
Console.WriteLine();

// 6. Целостность: подмена шифротекста обнаруживается при расшифровке.
var chars = packed.ToCharArray();
var middle = chars.Length / 2;
chars[middle] = chars[middle] == 'A' ? 'B' : 'A';
var tampered = new string(chars);

try
{
    EndToEndCrypto.Decrypt(tampered, bobKey);
    Console.WriteLine("ОШИБКА: подмена не замечена!");
}
catch (System.Security.Cryptography.CryptographicException)
{
    Console.WriteLine("Подмена обнаружена: изменённый шифротекст отклонён при расшифровке.");
}

// 7. Отпечатки ключей для сверки глазами между пользователями.
Console.WriteLine();
Console.WriteLine($"Отпечаток ключа Алисы: {EndToEndCrypto.GetFingerprint(alice.PublicKeyBase64)}");
Console.WriteLine($"Отпечаток ключа Боба: {EndToEndCrypto.GetFingerprint(bob.PublicKeyBase64)}");
Console.WriteLine("Сверив эти строки по другому каналу, пользователи убеждаются, что ключи не подменены.");
