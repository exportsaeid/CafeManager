using System;
using System.Security.Cryptography;
using System.Text;

namespace CafeManager
{
    public static class SecurityHelper
    {
        // یک کلید رمزنگاری ساده (می‌توانید هر چیزی بنویسید)
        private static readonly string SecretKey = "CafeManagerLogKey!";

        // تبدیل متن خوانا به متن رمزنگاری شده خرچنگ‌قورباغه!
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(plainText);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ SecretKey[i % SecretKey.Length]); // عملیات XOR
            }
            return Convert.ToBase64String(bytes);
        }

        // برگرداندن متن رمز شده به متن خوانا (مخصوص نمایش به مدیر)
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            try
            {
                var bytes = Convert.FromBase64String(cipherText);
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(bytes[i] ^ SecretKey[i % SecretKey.Length]);
                }
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return "🚨 خطا در رمزگشایی! فایل دستکاری شده است.";
            }
        }

        // ==================== هش کردن رمز عبور با SHA256 ====================
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // ==================== بررسی رمز عبور ====================
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            string hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }

        // ==================== تولید رمز عبور تصادفی ====================
        public static string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var random = new Random();
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

        // ==================== تولید توکن یکبار مصرف ====================
        public static string GenerateToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                         .Replace("+", "")
                         .Replace("/", "")
                         .Replace("=", "")
                         .Substring(0, 32);
        }

        // ==================== هش کردن با Salt ====================
        public static string HashPasswordWithSalt(string password, string salt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(salt))
                return string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] combinedBytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashedBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // ==================== تولید Salt ====================
        public static string GenerateSalt(int length = 16)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

        // ==================== رمزنگاری پیشرفته با AES ====================
        public static string EncryptAES(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(key))
                return string.Empty;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                    aes.IV = new byte[16]; // IV صفر برای سادگی

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    return Convert.ToBase64String(cipherBytes);
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        // ==================== رمزگشایی پیشرفته با AES ====================
        public static string DecryptAES(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(key))
                return string.Empty;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
                    aes.IV = new byte[16];

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    byte[] cipherBytes = Convert.FromBase64String(cipherText);
                    byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
            catch
            {
                return "🚨 خطا در رمزگشایی!";
            }
        }

        // ==================== Base64 Encode ====================
        public static string Base64Encode(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainBytes);
        }

        // ==================== Base64 Decode ====================
        public static string Base64Decode(string base64Text)
        {
            if (string.IsNullOrEmpty(base64Text))
                return string.Empty;

            try
            {
                byte[] base64Bytes = Convert.FromBase64String(base64Text);
                return Encoding.UTF8.GetString(base64Bytes);
            }
            catch
            {
                return "🚨 خطا در دیکد کردن Base64!";
            }
        }
    }
}