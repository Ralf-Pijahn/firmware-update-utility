using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UsbEncryptionUtils
{
    public static class KeyDecrypter
    {
        /// <summary>
        /// Decrypt the content of an encrypted keyfile and extract the information
        /// </summary>
        /// <param name="encryptedData">The data to decrypt and decompose</param>
        /// <param name="deviceId">the id of the device the key was read from</param>
        /// <param name="keyValue">the decrypted key</param>
        /// <param name="userName">the decrypted username</param>
        /// <param name="dateTime">the decrypted datetime</param>
        /// <exception cref="CryptographicException">Thown when encryptedData cannot be decrypted (usually due to an invalid / incorrect deviceId)</exception>
        public static void DecryptKeyData(byte[] encryptedData, string deviceId, out byte[] keyValue, out string userName, out DateTime dateTime)
        {
            var bytes = new byte[deviceId.Length * sizeof(char)];
            System.Buffer.BlockCopy(deviceId.ToCharArray(), 0, bytes, 0, bytes.Length);
            var hasher = SHA256Managed.Create();
            byte[] keyBytes = hasher.ComputeHash(bytes);

            byte[] decrypted;
            using (Aes myAes = Aes.Create())
            {
                // Encrypt the string to an array of bytes.
                decrypted = DecryptByteArray(keyBytes, encryptedData);
            }

            // Now split it up
            int pos = 0;

            //var keyLength = decrypted[pos++];

            var keyLengthBytes = decrypted.RangeSubset(pos, 4);
            pos += 4;
            var keyLength = BitConverter.ToInt32(keyLengthBytes, 0);

            var key = decrypted.RangeSubset(pos, keyLength);
            pos += keyLength;

            var deviceIdReadLength = decrypted[pos++];
            var deviceIdRead = decrypted.RangeSubset(pos, deviceIdReadLength);
            pos += deviceIdReadLength;

            var userNameReadLength = decrypted[pos++];
            var userNameRead = decrypted.RangeSubset(pos, userNameReadLength);
            pos += userNameReadLength;

            var dateTimeReadLength = decrypted[pos++];
            var dateTimeRead = decrypted.RangeSubset(pos, dateTimeReadLength);

            keyValue = key;
            userName = Encoding.UTF8.GetString(userNameRead);
            dateTime = DateTime.FromBinary(BitConverter.ToInt64(dateTimeRead, 0));
        }

        /// <summary>
        /// Decrypt a byte array
        /// </summary>
        /// <param name="encryptionKey">The key used to decrypt</param>
        /// <param name="dataToDecrypt">The data to decrypt</param>
        /// <exception cref="CryptographicException">Thown when dateToDecrypt cannot be decrypted (usually due to an invalid / incorrect encryptionKey)</exception>
        /// <returns>Decrypted byte array</returns>
        /// Note On CA2202 Supression:  See https://blogs.msdn.microsoft.com/tilovell/2014/02/12/the-worst-code-analysis-rule-thats-recommended-ca2202/
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static byte[] DecryptByteArray(byte[] encryptionKey, byte[] dataToDecrypt)
        {
            byte[] iv = new byte[16]; //initial vector is 16 bytes
            byte[] encryptedContent = new byte[dataToDecrypt.Length - 16]; //the rest should be encryptedcontent

            //Copy data to byte array
            System.Buffer.BlockCopy(dataToDecrypt, 0, iv, 0, iv.Length);
            System.Buffer.BlockCopy(dataToDecrypt, iv.Length, encryptedContent, 0, encryptedContent.Length);

            using (MemoryStream ms = new MemoryStream())
            {
                using (AesManaged cryptor = new AesManaged())
                {
                    cryptor.Mode = CipherMode.CBC;
                    cryptor.Padding = PaddingMode.PKCS7;
                    cryptor.KeySize = 128;
                    cryptor.BlockSize = 128;

                    using (CryptoStream cs = new CryptoStream(ms, cryptor.CreateDecryptor(encryptionKey, iv), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedContent, 0, encryptedContent.Length);

                    }
                    return ms.ToArray();
                }
            }
        }
    }
}
