using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Utility
{
    public class DecEncCode
    {
        #region DES
        /*
        //容易被暴力破解，目前已非安全加密標準之一，故不採用
        /// <summary>
        /// 獲取密鑰
        /// </summary>
        private static string DES_Key
        {
            //get { return @"P@+#wG+Z"; }

            get { return @"SCONSCON"; }
        }
        /// <summary>
        /// 獲取向量
        /// </summary>
        private static string DES_IV
        {
            //get { return @"L%n67}G\Mk@k%:~Y"; }
            get { return @"SCONsconSCONscon"; }
        }
        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="plainStr">明文字符串</param>
        /// <returns>密文</returns>
        public static string DESEncrypt(string plainStr)
        {
            byte[] bKey = Encoding.UTF8.GetBytes(DES_Key);
            byte[] bIV = Encoding.UTF8.GetBytes(DES_IV);
            byte[] byteArray = Encoding.UTF8.GetBytes(plainStr);
            string encrypt = null;
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, des.CreateEncryptor(bKey, bIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        encrypt = Convert.ToBase64String(mStream.ToArray());
                    }
                }
            }
            catch { }
            des.Clear();
            return encrypt;
        }
        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="encryptStr">密文字符串</param>
        /// <returns>明文</returns>
        public static string DESDecrypt(string encryptStr)
        {
            byte[] bKey = Encoding.UTF8.GetBytes(DES_Key);
            byte[] bIV = Encoding.UTF8.GetBytes(DES_IV);
            byte[] byteArray = Convert.FromBase64String(encryptStr);
            string decrypt = null;
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            //RijndaelManaged provider_AES = new RijndaelManaged();
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, des.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        decrypt = Encoding.UTF8.GetString(mStream.ToArray());
                    }
                }
            }
            catch { }
            des.Clear();
            return decrypt;
        }
        */
        #endregion

        #region Common Config
        private static string iKey
        {
            //get { return @")O[NB]6,YF}+efcaj{+oESb9d8>Z'e9M"; }
            //get { return @"SCONsconSCONsconSCONsconSCONscon"; }
            get { return @"t71{54j\1kyaA7049bY)0"; }
        }
        private static string iIV
        {
            //get { return @"L+\~f4,Ir)b$=pkf"; }
            //get { return @"SCONsconSCONscon"; }
            get { return @"8804"; }
        }
        #endregion

        #region AES
        /// <summary>
        /// AES加密
        /// </summary>
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="plainStr">明文字符串</param>
        /// <returns>密文</returns>
        public static string AESEncrypt(string plainStr)
        {
            MD5CryptoServiceProvider provider_MD5 = new MD5CryptoServiceProvider();
            byte[] bKey_i = Encoding.UTF8.GetBytes(iKey);
            byte[] bKey = provider_MD5.ComputeHash(bKey_i);
            byte[] bIV_i = Encoding.UTF8.GetBytes(iIV);
            byte[] bIV = provider_MD5.ComputeHash(bIV_i);

            byte[] byteArray = Encoding.UTF8.GetBytes(plainStr);
            string encrypt = null;
            Rijndael aes = Rijndael.Create();
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateEncryptor(bKey, bIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        encrypt = Convert.ToBase64String(mStream.ToArray());
                    }
                }
            }
            catch { }
            aes.Clear();
            return encrypt;
        }
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="plainStr">明文字符串</param>
        /// <param name="returnNull">加密失敗時是否返回 null，false 返回 String.Empty</param>
        /// <returns>密文</returns>
        public static string AESEncrypt(string plainStr, bool returnNull)
        {
            string encrypt = AESEncrypt(plainStr);
            return returnNull ? encrypt : (encrypt == null ? String.Empty : encrypt);
        }
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="encryptStr">密文字符串</param>
        /// <returns>明文</returns>
        public static string AESDecrypt(string encryptStr)
        {
            MD5CryptoServiceProvider provider_MD5 = new MD5CryptoServiceProvider();
            byte[] bKey_i = Encoding.UTF8.GetBytes(iKey);
            byte[] bKey = provider_MD5.ComputeHash(bKey_i);
            byte[] bIV_i = Encoding.UTF8.GetBytes(iIV);
            byte[] bIV = provider_MD5.ComputeHash(bIV_i);

            byte[] byteArray = Convert.FromBase64String(encryptStr);
            string decrypt = null;
            Rijndael aes = Rijndael.Create();
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        decrypt = Encoding.UTF8.GetString(mStream.ToArray());
                    }
                }
            }
            catch { }
            aes.Clear();
            return decrypt;
        }
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="encryptStr">密文字符串</param>
        /// <param name="returnNull">解密失敗時是否返回 null，false 返回 String.Empty</param>
        /// <returns>明文</returns>
        public static string AESDecrypt(string encryptStr, bool returnNull)
        {
            string decrypt = AESDecrypt(encryptStr);
            return returnNull ? decrypt : (decrypt == null ? String.Empty : decrypt);
        }
        #endregion

        #region 3DES
        /// <summary>
        /// TripleDES加密
        /// </summary>
        /// <param name="plainStr">明文字符串</param>
        /// <returns>密文</returns>
        public static string TDESEncrypt(string plainStr)
        {
            MD5CryptoServiceProvider provider_MD5 = new MD5CryptoServiceProvider();
            byte[] bKey_i = Encoding.UTF8.GetBytes(iKey);
            byte[] bKey = provider_MD5.ComputeHash(bKey_i);
            byte[] bIV_i = Encoding.UTF8.GetBytes(iIV);
            byte[] bIV = provider_MD5.ComputeHash(bIV_i);

            byte[] byteArray = Encoding.UTF8.GetBytes(plainStr);
            string encrypt = null;
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, des.CreateEncryptor(bKey, bIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        encrypt = Convert.ToBase64String(mStream.ToArray());
                    }
                }
            }
            catch { }
            des.Clear();
            return encrypt;
        }
        /// <summary>
        /// TripleDES解密
        /// </summary>
        /// <param name="encryptStr">密文字符串</param>
        /// <returns>明文</returns>
        public static string TDESDecrypt(string encryptStr)
        {
            MD5CryptoServiceProvider provider_MD5 = new MD5CryptoServiceProvider();
            byte[] bKey_i = Encoding.UTF8.GetBytes(iKey);
            byte[] bKey = provider_MD5.ComputeHash(bKey_i);
            byte[] bIV_i = Encoding.UTF8.GetBytes(iIV);
            byte[] bIV = provider_MD5.ComputeHash(bIV_i);

            byte[] byteArray = Convert.FromBase64String(encryptStr);
            string decrypt = null;
            TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
            try
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream cStream = new CryptoStream(mStream, des.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write))
                    {
                        cStream.Write(byteArray, 0, byteArray.Length);
                        cStream.FlushFinalBlock();
                        decrypt = Encoding.UTF8.GetString(mStream.ToArray());
                    }
                }
            }
            catch { }
            des.Clear();
            return decrypt;
        }
        #endregion

        #region MD5

        public static string MD5Encrypt(string plainStr)
        {
            MD5 md5 = MD5.Create();//建立一個MD5
            byte[] source = Encoding.Default.GetBytes(plainStr);//將字串轉為Byte[]
            byte[] crypto = md5.ComputeHash(source);//進行MD5加密
            //string result = Convert.ToBase64String(crypto);//把加密後的字串從Byte[]轉為字串
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < crypto.Length; i++)
            {
                sb.Append(crypto[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }
        #endregion
    }
}
