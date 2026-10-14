using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace SkillTree.SaveSystem
{
    /// <summary>
    /// Handles encryption and decryption of save data using AES-256.
    /// Provides secure storage for game saves to prevent tampering and cheating.
    /// </summary>
    public static class SaveEncryption
    {
        // Default encryption key - IMPORTANT: Change this for your game!
        // For production, consider using a more secure key management system
        private const string DefaultEncryptionKey = "SkillTree_Default_Key_Change_This_In_Production_2024";

        // Salt for key derivation (adds additional security)
        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("SkillTreeSaveSystem_Salt_V1");

        /// <summary>
        /// Encrypts a string using AES-256 encryption.
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <param name="encryptionKey">Optional custom encryption key. If null, uses default key.</param>
        /// <returns>Base64 encoded encrypted string</returns>
        public static string Encrypt(string plainText, string encryptionKey = null)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            try
            {
                var key = DeriveKey(encryptionKey ?? DefaultEncryptionKey);

                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.GenerateIV();

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var msEncrypt = new MemoryStream())
                    {
                        // Write IV at the beginning of the encrypted data
                        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveEncryption] Encryption failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Decrypts a string that was encrypted with the Encrypt method.
        /// </summary>
        /// <param name="cipherText">Base64 encoded encrypted string</param>
        /// <param name="encryptionKey">Optional custom encryption key. Must match the key used for encryption.</param>
        /// <returns>Decrypted plain text</returns>
        public static string Decrypt(string cipherText, string encryptionKey = null)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            try
            {
                var key = DeriveKey(encryptionKey ?? DefaultEncryptionKey);
                var cipherBytes = Convert.FromBase64String(cipherText);

                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    // Extract IV from the beginning of the encrypted data
                    var iv = new byte[aes.IV.Length];
                    Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var msDecrypt = new MemoryStream(cipherBytes, iv.Length, cipherBytes.Length - iv.Length))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (CryptographicException ex)
            {
                Debug.LogError($"[SaveEncryption] Decryption failed - possibly wrong key or corrupted data: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveEncryption] Decryption failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Derives a 256-bit encryption key from a password using PBKDF2.
        /// </summary>
        private static byte[] DeriveKey(string password)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, Salt, 10000, HashAlgorithmName.SHA256))
            {
                return deriveBytes.GetBytes(32); // 256 bits for AES-256
            }
        }

        /// <summary>
        /// Validates if a string is properly encrypted (can be decrypted).
        /// </summary>
        /// <param name="cipherText">The encrypted string to validate</param>
        /// <param name="encryptionKey">Optional custom encryption key</param>
        /// <returns>True if the string can be decrypted successfully</returns>
        public static bool IsValidEncryptedData(string cipherText, string encryptionKey = null)
        {
            try
            {
                Decrypt(cipherText, encryptionKey);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a random encryption key suitable for use with this encryption system.
        /// Useful for creating unique keys per user or installation.
        /// </summary>
        /// <returns>A cryptographically secure random key string</returns>
        public static string GenerateRandomKey()
        {
            const int keyLength = 32;
            var randomBytes = new byte[keyLength];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Creates a device-unique encryption key based on device identifiers.
        /// This ties the save data to a specific device, preventing easy copying.
        /// </summary>
        /// <returns>Device-specific encryption key</returns>
        public static string GetDeviceUniqueKey()
        {
            // Use SystemInfo.deviceUniqueIdentifier as base
            var deviceId = SystemInfo.deviceUniqueIdentifier;

            // Combine with additional device info for extra entropy
            var combinedData = $"{deviceId}_{Application.identifier}_{SystemInfo.deviceModel}";

            // Hash the combined data to create a consistent key
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedData));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
