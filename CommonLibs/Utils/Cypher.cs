using System.Security.Cryptography;
using System.Text;

namespace CommonLibs.Utils
{
    public class Cypher
    {
        public record AesEncrypted(string CipherText, string Iv);
        public record RsaKeyPair(RSA PublicKey, RSA PrivateKey);

        public static class CypherHelper
        {
            // ===== UTF8 helpers =====

            public static byte[] Utf8ToByteArray(string str)
            {
                return Encoding.UTF8.GetBytes(str);
            }

            public static string ByteArrayToUtf8(byte[] bytes)
            {
                return Encoding.UTF8.GetString(bytes);
            }

            // ===== Base64 helpers =====

            public static string ByteArrayToBase64(byte[] bytes)
            {
                return Convert.ToBase64String(bytes);
            }

            public static byte[] Base64ToByteArray(string base64)
            {
                return Convert.FromBase64String(base64);
            }
        }

        public static class AesCypher
        {
            private const int IvSize = 12;      // 96 bits (WebCrypto standard)
            private const int TagSize = 16;     // 128 bits auth tag

            // ===== AES Key Generator =====
            public static byte[] GenerateAesKey(int keySize = 256)
            {
                if (keySize is not (128 or 192 or 256))
                    throw new ArgumentException("Key size must be 128, 192, or 256 bits");

                return RandomNumberGenerator.GetBytes(keySize / 8);
            }

            // ===== AES Encrypt =====
            public static AesEncrypted Encrypt(string plainText, byte[] key)
            {
                var iv = RandomNumberGenerator.GetBytes(IvSize);
                var plaintextBytes = CypherHelper.Utf8ToByteArray(plainText);

                var cipherText = new byte[plaintextBytes.Length];
                var tag = new byte[TagSize];

                using var aes = new AesGcm(key);

                aes.Encrypt(
                    nonce: iv,
                    plaintext: plaintextBytes,
                    ciphertext: cipherText,
                    tag: tag
                );

                // WebCrypto returns cipher + tag together → we must do the same
                var combinedCipher = new byte[cipherText.Length + tag.Length];
                Buffer.BlockCopy(cipherText, 0, combinedCipher, 0, cipherText.Length);
                Buffer.BlockCopy(tag, 0, combinedCipher, cipherText.Length, tag.Length);

                return new AesEncrypted(
                    CipherText: CypherHelper.ByteArrayToBase64(combinedCipher),
                    Iv: CypherHelper.ByteArrayToBase64(iv)
                );
            }

            // ===== AES Decrypt =====
            public static string Decrypt(string cipherTextBase64, string ivBase64, byte[] key)
            {
                var iv = CypherHelper.Base64ToByteArray(ivBase64);
                var combinedCipher = CypherHelper.Base64ToByteArray(cipherTextBase64);

                var cipherText = new byte[combinedCipher.Length - TagSize];
                var tag = new byte[TagSize];

                Buffer.BlockCopy(combinedCipher, 0, cipherText, 0, cipherText.Length);
                Buffer.BlockCopy(combinedCipher, cipherText.Length, tag, 0, TagSize);

                var plaintextBytes = new byte[cipherText.Length];

                using var aes = new AesGcm(key);

                aes.Decrypt(
                    nonce: iv,
                    ciphertext: cipherText,
                    tag: tag,
                    plaintext: plaintextBytes
                );

                return CypherHelper.ByteArrayToUtf8(plaintextBytes);
            }
        }

        public static class RsaCypher
        {
            // ===== Generate RSA Key Pair =====
            public static RsaKeyPair GenerateKeys(int modulusLength = 2048, HashAlgorithmName hashAlgorithm = default)
            {
                if (hashAlgorithm == default)
                    hashAlgorithm = HashAlgorithmName.SHA256;

                var rsa = RSA.Create(modulusLength);

                return new RsaKeyPair(
                    PublicKey: RSA.Create(rsa.ExportParameters(false)),
                    PrivateKey: rsa
                );
            }

            // ===== Export Keys =====

            public static string ExportPublicKey(RSA publicKey)
            {
                var bytes = publicKey.ExportSubjectPublicKeyInfo();
                return CypherHelper.ByteArrayToBase64(bytes);
            }

            public static string ExportPrivateKey(RSA privateKey)
            {
                var bytes = privateKey.ExportPkcs8PrivateKey();
                return CypherHelper.ByteArrayToBase64(bytes);
            }

            // ===== Import Keys =====

            public static RSA ImportPublicKey(
                string base64,
                HashAlgorithmName hashAlgorithm = default)
            {
                if (hashAlgorithm == default)
                    hashAlgorithm = HashAlgorithmName.SHA256;

                var bytes = CypherHelper.Base64ToByteArray(base64);

                var rsa = RSA.Create();
                rsa.ImportSubjectPublicKeyInfo(bytes, out _);

                return rsa;
            }

            public static RSA ImportPrivateKey(string base64, HashAlgorithmName hashAlgorithm = default)
            {
                if (hashAlgorithm == default)
                    hashAlgorithm = HashAlgorithmName.SHA256;

                var bytes = CypherHelper.Base64ToByteArray(base64);

                var rsa = RSA.Create();
                rsa.ImportPkcs8PrivateKey(bytes, out _);

                return rsa;
            }

            // ===== Encrypt / Decrypt =====

            public static string Encrypt(string message, RSA publicKey, HashAlgorithmName hashAlgorithm = default)
            {
                if (hashAlgorithm == default)
                    hashAlgorithm = HashAlgorithmName.SHA256;

                var data = CypherHelper.Utf8ToByteArray(message);

                var cipher = publicKey.Encrypt(
                    data,
                    GetOaepPadding(hashAlgorithm)
                );

                return CypherHelper.ByteArrayToBase64(cipher);
            }

            public static string Decrypt(string cipherTextBase64, RSA privateKey, HashAlgorithmName hashAlgorithm = default)
            {
                if (hashAlgorithm == default)
                    hashAlgorithm = HashAlgorithmName.SHA256;

                var cipher = CypherHelper.Base64ToByteArray(cipherTextBase64);

                var plain = privateKey.Decrypt(
                    cipher,
                    GetOaepPadding(hashAlgorithm)
                );

                return CypherHelper.ByteArrayToUtf8(plain);
            }

            // ===== Padding Resolver =====

            private static RSAEncryptionPadding GetOaepPadding(HashAlgorithmName hash)
            {
                if (hash == HashAlgorithmName.SHA1) return RSAEncryptionPadding.OaepSHA1;
                if (hash == HashAlgorithmName.SHA256) return RSAEncryptionPadding.OaepSHA256;
                if (hash == HashAlgorithmName.SHA384) return RSAEncryptionPadding.OaepSHA384;
                if (hash == HashAlgorithmName.SHA512) return RSAEncryptionPadding.OaepSHA512;

                throw new NotSupportedException($"Unsupported hash: {hash.Name}");
            }
        }
    }
}
