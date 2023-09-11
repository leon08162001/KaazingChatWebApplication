using System;
using System.Security.Cryptography;

namespace Common.Ciphers
{
    public class Sha1Hasher
    {
        public static string sha1Hash(string plaintext)
        {
            SHA1 sh1 = SHA1.Create();
            String hash = "mySaltHere" + plaintext;
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = encoding.GetBytes(hash);
            String hashed = byteArrayToHexString(sh1.ComputeHash(bytes));
            return hashed;
        }

        public static string byteArrayToHexString(byte[] b)
        {
            string result = "";
            for (int i = 0; i < b.Length; i++)
            {
                result +=
                    ((b[i] & 0xff) + 0x100).ToString("x").Substring(1);
            }
            return result;
        }
    }

}
