using System.Security.Cryptography;
using System.Text;

namespace TSL.Base.Platform.Utilities
{
    /// <summary>
    /// A helper class to encrypt and decrypt string text values
    /// </summary>
    public static class EncryptionHelper
    {
        private const string _secretKey = "V8RmflUdZKqYmxQp3tjVU2m+dbR5e7FDapmWzGoN8rSHjrtv0oNcQtEWNM8NomC7";
        private const string _iv = "0mwULawCjjM2wl7ZVHkY5g==";

        /// <summary>
        /// Encrypts a string
        /// </summary>
        /// <param name="stringToEncrypt">String to to encyprt</param>
        /// <returns></returns>
        public static string Encrypt(string stringToEncrypt)
        {
            byte[] stringToEncryptBytes = Encoding.UTF8.GetBytes(stringToEncrypt);

            using Aes aes = GetAes();
            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] resultBytes = encryptor.TransformFinalBlock(stringToEncryptBytes, 0, stringToEncryptBytes.Length);

            return Convert.ToBase64String(resultBytes);
        }

        /// <summary>
        /// Decrypts a string that was encrypted using this class
        /// </summary>
        /// <param name="stringToDecrypt">Encrypted string to decrypt</param>
        /// <returns></returns>
        public static string Decrypt(string stringToDecrypt)
        {
            byte[] stringToDecryptBytes = Convert.FromBase64String(stringToDecrypt);

            using Aes aes = GetAes();
            using ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] resultBytes = decryptor.TransformFinalBlock(stringToDecryptBytes, 0, stringToDecryptBytes.Length);

            return Encoding.UTF8.GetString(resultBytes);
        }

        /// <summary>
        /// Extension method for decrypting a string that have been encrypted using EncryptionHelper
        /// </summary>
        /// <param name="stringToDecrypt">Encrypted string to decrypt</param>
        /// <returns></returns>
        public static string DecryptEncryptedString(this string stringToDecrypt)
        {
            return Decrypt(stringToDecrypt);
        }

        private static Aes GetAes()
        {
            byte[] key = Encoding.UTF8.GetBytes(_secretKey);
            byte[] iv = Convert.FromBase64String(_iv);

            Aes aes = Aes.Create();

            aes.Key = key;
            aes.IV = iv;

            return aes;
        }
    }
}

