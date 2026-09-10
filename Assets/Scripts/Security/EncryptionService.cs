using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Platformer.Security
{
    /// <summary>
    /// Servicio criptográfico de bajo nivel.
    /// Provee cifrado simétrico AES-256 (CBC con PKCS7) y cálculo de resúmenes criptográficos SHA-256.
    /// </summary>
    public static class EncryptionService
    {
        // Clave y vector de inicialización fijos derivados para la aplicación
        // En producción se derivan con PBKDF2 + identificador de hardware del usuario
        private static readonly byte[] EncryptionKey = new byte[32]
        {
            0x2D, 0x4A, 0x61, 0x75, 0x38, 0x1F, 0x9B, 0x0C,
            0xAA, 0x55, 0x7E, 0x43, 0x82, 0xD1, 0xFE, 0x66,
            0x90, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE,
            0xF0, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77
        };

        private static readonly byte[] InitializationVector = new byte[16]
        {
            0x1B, 0x3C, 0x5E, 0x7F, 0x90, 0x2A, 0x4D, 0x68,
            0x8A, 0xA1, 0xB2, 0xC3, 0xD4, 0xE5, 0xF6, 0x07
        };

        /// <summary>
        /// Cifra un texto UTF-8 utilizando AES-256.
        /// </summary>
        public static byte[] EncryptStringToBytes(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return Array.Empty<byte>();

            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.IV  = InitializationVector;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Descifra un búfer de bytes cifrado con AES-256 a texto legible.
        /// </summary>
        public static string DecryptStringFromBytes(byte[] cipherBytes)
        {
            if (cipherBytes == null || cipherBytes.Length == 0)
                return string.Empty;

            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.IV  = InitializationVector;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var ms = new MemoryStream(cipherBytes))
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var reader = new StreamReader(cs, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Calcula el hash SHA-256 de un arreglo de bytes.
        /// </summary>
        public static byte[] ComputeSha256(byte[] data)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(data);
            }
        }

        /// <summary>
        /// Comprueba si dos secuencias de bytes (hashes) son idénticas en tiempo constante
        /// para prevenir ataques de temporización (timing attacks).
        /// </summary>
        public static bool ConstantTimeAreEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
