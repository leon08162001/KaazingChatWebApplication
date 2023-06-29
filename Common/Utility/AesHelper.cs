using System;
using System.Security.Cryptography;
using System.Text;

namespace Common.Utility
{
    public static class AesHelper
    {
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="PlainText">待加密的明文</param>
        /// <returns></returns>
        public static string AesEncrypt(string PlainText, string KeyStr, string IvStr)
        {
            var rijndaelCipher = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                BlockSize = 128
            };

            var toEncryptBytes = Encoding.UTF8.GetBytes(PlainText);
            var keyBytes = Encoding.UTF8.GetBytes(KeyStr);
            var ivBytes = Encoding.UTF8.GetBytes(IvStr);

            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = ivBytes;

            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
            byte[] cipherBytes = transform.TransformFinalBlock(toEncryptBytes, 0, toEncryptBytes.Length);
            return Convert.ToBase64String(cipherBytes);
        }
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="CipherText"></param>
        /// <returns></returns>
        public static string AesDecrpt(string CipherText, string KeyStr, string IvStr)
        {
            var key = Encoding.UTF8.GetBytes(KeyStr);
            var encryptedData = Convert.FromBase64String(CipherText);
            var iv = Encoding.UTF8.GetBytes(IvStr);

            var rDel = new RijndaelManaged
            {
                Key = key,
                IV = iv,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            var cTransform = rDel.CreateDecryptor();
            var resultArray = cTransform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

            return Encoding.UTF8.GetString(resultArray);
        }
    }
}
