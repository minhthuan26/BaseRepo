using CommonLibs.Utils;
using System;
using System.Security.Cryptography;

namespace SystemManagement.Views.ConfigEnv
{
    public static class EnvValueGenerator
    {
        public static string GenerateAesKey(int bits = 512)
        {
            if (bits is not (128 or 256 or 512))
                throw new ArgumentException("Độ dài khóa AES phải là 128, 256 hoặc 512 bit.", nameof(bits));

            byte[] keyBytes = RandomNumberGenerator.GetBytes(bits / 8);
            return Convert.ToBase64String(keyBytes);
        }

        public static string GenerateUrn(string prefix = "urn:api:")
        {
            prefix ??= "urn:api:";
            return $"{prefix}{Guid.NewGuid():N}";
        }

        public static string GenerateRandomString(int length = 32)
        {
            if (length is < 8 or > 256)
                throw new ArgumentOutOfRangeException(nameof(length), "Độ dài chuỗi ngẫu nhiên phải từ 8 đến 256 ký tự.");

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return RandomNumberGenerator.GetString(chars, length);
        }
    }
}
