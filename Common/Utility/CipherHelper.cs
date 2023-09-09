using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Common.Ciphers
{
    public class CipherHelper
    {
        #region RSA 加密解密

        #region RSA 的密鑰產生
        /// <summary>
        /// RSA產生密鑰
        /// </summary>
        /// <param name="xmlKeys">私鑰</param>
        /// <param name="xmlPublicKey">公鑰</param>
        static public void RSAKey(out string xmlKeys, out string xmlPublicKey)
        {
            try
            {
                System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                xmlKeys = rsa.ToXmlString(true);
                xmlPublicKey = rsa.ToXmlString(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region RSA加密函數
        //############################################################################## 
        //RSA 方式加密 
        //KEY必須是XML的形式,返回的是字符串 
        //該加密方式有長度限制的！
        //##############################################################################

        /// <summary>
        /// RSA的加密函數
        /// </summary>
        /// <param name="xmlPublicKey">公鑰</param>
        /// <param name="encryptString">待加密的字符串</param>
        /// <returns></returns>
        static public string RSAEncrypt(string xmlPublicKey, string encryptString)
        {
            try
            {
                byte[] PlainTextBArray;
                byte[] CypherTextBArray;
                string Result;
                System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(xmlPublicKey);
                PlainTextBArray = (new UTF8Encoding()).GetBytes(encryptString);
                CypherTextBArray = rsa.Encrypt(PlainTextBArray, false);
                Result = Convert.ToBase64String(CypherTextBArray);
                return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// RSA的加密函數 
        /// </summary>
        /// <param name="xmlPublicKey">公鑰</param>
        /// <param name="EncryptString">待加密的字節數組</param>
        /// <returns></returns>
        static public string RSAEncrypt(string xmlPublicKey, byte[] EncryptString)
        {
            try
            {
                byte[] CypherTextBArray;
                string Result;
                System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(xmlPublicKey);
                CypherTextBArray = rsa.Encrypt(EncryptString, false);
                Result = Convert.ToBase64String(CypherTextBArray);
                return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region RSA解密函數 
        /// <summary>
        /// RSA的解密函數
        /// </summary>
        /// <param name="xmlPrivateKey">私鑰</param>
        /// <param name="decryptString">待解密的字符串</param>
        /// <returns></returns>
        static public string RSADecrypt(string xmlPrivateKey, string decryptString)
        {
            try
            {
                byte[] PlainTextBArray;
                byte[] DypherTextBArray;
                string Result;
                System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(xmlPrivateKey);
                PlainTextBArray = Convert.FromBase64String(decryptString);
                DypherTextBArray = rsa.Decrypt(PlainTextBArray, false);
                Result = (new UTF8Encoding()).GetString(DypherTextBArray);
                return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// RSA的解密函數 
        /// </summary>
        /// <param name="xmlPrivateKey">私鑰</param>
        /// <param name="DecryptString">待解密的字節數組</param>
        /// <returns></returns>
        static public string RSADecrypt(string xmlPrivateKey, byte[] DecryptString)
        {
            try
            {
                byte[] DypherTextBArray;
                string Result;
                System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(xmlPrivateKey);
                DypherTextBArray = rsa.Decrypt(DecryptString, false);
                Result = (new UnicodeEncoding()).GetString(DypherTextBArray);
                return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        static public string EncryptRSA(string publickey, string plaintext)
        {
            //RSA的加解密過程： 
            // 有 rsa1 和 rsa2 兩個RSA對象。 
            // 現在要 rsa2 發送一段信息給 rsa1， 則先由 rsa1 發送“公鑰”給 rsa2 
            // rsa2 獲取得公鑰之後，用來加密要發送的數據內容。 
            // rsa1 獲取加密後的內容後，用自己的私鑰解密，得出原始的數據內容。

            RSACryptoServiceProvider rsa1 = new RSACryptoServiceProvider();
            RSACryptoServiceProvider rsa2 = new RSACryptoServiceProvider();

            publickey = rsa1.ToXmlString(false); //導出 rsa1 的公鑰

            rsa2.FromXmlString(publickey); //rsa2 導入 rsa1 的公鑰，用於加密信息

            //rsa2開始加密 
            byte[] cipherbytes;
            cipherbytes = rsa2.Encrypt(Encoding.UTF8.GetBytes(plaintext), false);

            string resut = string.Empty;
            for (int i = 0; i < cipherbytes.Length; i++)
            {
                resut += string.Format("{0:X2} ", cipherbytes[i]);
            }
            return resut;
        }

        public static void DecryptRSA(string publickey, string plaintext)
        {
            ////RSA的加解密過程： 
            //// 有 rsa1 和 rsa2 兩個RSA對象。 
            //// 現在要 rsa2 發送一段信息給 rsa1， 則先由 rsa1 發送“公鑰”給 rsa2 
            //// rsa2 獲取得公鑰之後，用來加密要發送的數據內容。 
            //// rsa1 獲取加密後的內容後，用自己的私鑰解密，得出原始的數據內容。

            //RSACryptoServiceProvider rsa1 = new RSACryptoServiceProvider();
            //RSACryptoServiceProvider rsa2 = new RSACryptoServiceProvider();

            //publickey = rsa1.ToXmlString(false); //導出 rsa1 的公鑰

            //rsa2.FromXmlString(publickey); //rsa2 導入 rsa1 的公鑰，用於加密信息

            ////rsa2開始加密 
            //byte[] cipherbytes;
            //cipherbytes = rsa2.Encrypt(Encoding.UTF8.GetBytes(plaintext), false);

            //string resut = string.Empty;
            //for (int i = 0; i < cipherbytes.Length; i++)
            //{
            // resut += string.Format("{0:X2} ", cipherbytes[i]);
            //}
            //return resut;
            ////rsa1開始解密 
            //byte[] plaintbytes;
            //plaintbytes = rsa1.Decrypt(cipherbytes, false);

            //Console.WriteLine("解密後的數據是：");
            //Console.WriteLine(Encoding.UTF8.GetString(plaintbytes));

            //Console.ReadLine();
        }

        #endregion

        #region RSA 數字簽名

        #region 獲取Hash描述表 
        /// <summary>
        /// 獲取Hash描述表
        /// </summary>
        /// <param name="strSource">待簽名的字符串</param>
        /// <param name="HashData">Hash描述</param>
        /// <returns></returns>
        static public bool GetHash(string strSource, ref byte[] HashData)
        {
            try
            {
                byte[] Buffer;
                System.Security.Cryptography.HashAlgorithm SHA256 = System.Security.Cryptography.HashAlgorithm.Create("SHA256");
                Buffer = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(strSource);
                HashData = SHA256.ComputeHash(Buffer);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 獲取Hash描述表
        /// </summary>
        /// <param name="strSource">待簽名的字符串</param>
        /// <param name="strHashData">Hash描述</param>
        /// <returns></returns>
        static public bool GetHash(string strSource, ref string strHashData)
        {
            try
            {
                //從字符串中取得Hash描述 
                byte[] Buffer;
                byte[] HashData;
                System.Security.Cryptography.HashAlgorithm SHA256 = System.Security.Cryptography.HashAlgorithm.Create("SHA256");
                Buffer = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(strSource);
                HashData = SHA256.ComputeHash(Buffer);
                strHashData = Convert.ToBase64String(HashData);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 獲取Hash描述表
        /// </summary>
        /// <param name="objFile">待簽名的文檔</param>
        /// <param name="HashData">Hash描述</param>
        /// <returns></returns>
        static public bool GetHash(System.IO.FileStream objFile, ref byte[] HashData)
        {
            try
            {
                //從文檔中取得Hash描述 
                System.Security.Cryptography.HashAlgorithm SHA256 = System.Security.Cryptography.HashAlgorithm.Create("SHA256");
                HashData = SHA256.ComputeHash(objFile);
                objFile.Close();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 獲取Hash描述表
        /// </summary>
        /// <param name="objFile">待簽名的文檔</param>
        /// <param name="strHashData">Hash描述</param>
        /// <returns></returns>
        static public bool GetHash(System.IO.FileStream objFile, ref string strHashData)
        {
            try
            {
                //從文檔中取得Hash描述 
                byte[] HashData;
                System.Security.Cryptography.HashAlgorithm SHA256 = System.Security.Cryptography.HashAlgorithm.Create("SHA256");
                HashData = SHA256.ComputeHash(objFile);
                objFile.Close();
                strHashData = Convert.ToBase64String(HashData);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region RSA簽名
        /// <summary>
        /// RSA簽名
        /// </summary>
        /// <param name="strKeyPrivate">私鑰</param>
        /// <param name="HashbyteSignature">待簽名Hash描述</param>
        /// <param name="EncryptedSignatureData">簽名後的結果</param>
        /// <returns></returns>
        static public bool SignatureFormatter(string strKeyPrivate, byte[] HashbyteSignature, ref byte[] EncryptedSignatureData)
        {
            try
            {
                System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();

                RSA.FromXmlString(strKeyPrivate);
                System.Security.Cryptography.RSAPKCS1SignatureFormatter RSAFormatter = new System.Security.Cryptography.RSAPKCS1SignatureFormatter(RSA);
                //設置簽名的算法為SHA256 
                RSAFormatter.SetHashAlgorithm("SHA256");
                //執行簽名 
                EncryptedSignatureData = RSAFormatter.CreateSignature(HashbyteSignature);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// RSA簽名
        /// </summary>
        /// <param name="strKeyPrivate">私鑰</param>
        /// <param name="HashbyteSignature">待簽名Hash描述</param>
        /// <param name="m_strEncryptedSignatureData">簽名後的結果</param>
        /// <returns></returns>
        static public bool SignatureFormatter(string strKeyPrivate, byte[] HashbyteSignature, ref string strEncryptedSignatureData)
        {
            try
            {
                byte[] EncryptedSignatureData;
                System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
                RSA.FromXmlString(strKeyPrivate);
                System.Security.Cryptography.RSAPKCS1SignatureFormatter RSAFormatter = new System.Security.Cryptography.RSAPKCS1SignatureFormatter(RSA);
                //設置簽名的算法為SHA256 
                RSAFormatter.SetHashAlgorithm("SHA256");
                //執行簽名 
                EncryptedSignatureData = RSAFormatter.CreateSignature(HashbyteSignature);
                strEncryptedSignatureData = Convert.ToBase64String(EncryptedSignatureData);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// RSA簽名
        /// </summary>
        /// <param name="strKeyPrivate">私鑰</param>
        /// <param name="strHashbyteSignature">待簽名Hash描述</param>
        /// <param name="EncryptedSignatureData">簽名後的結果</param>
        /// <returns></returns>
        static public bool SignatureFormatter(string strKeyPrivate, string strHashbyteSignature, ref byte[] EncryptedSignatureData)
        {
            try
            {
                byte[] HashbyteSignature;

                HashbyteSignature = Convert.FromBase64String(strHashbyteSignature);
                System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();

                RSA.FromXmlString(strKeyPrivate);
                System.Security.Cryptography.RSAPKCS1SignatureFormatter RSAFormatter = new System.Security.Cryptography.RSAPKCS1SignatureFormatter(RSA);
                //設置簽名的算法為SHA256 
                RSAFormatter.SetHashAlgorithm("SHA256");
                //執行簽名 
                EncryptedSignatureData = RSAFormatter.CreateSignature(HashbyteSignature);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// RSA簽名
        /// </summary>
        /// <param name="strKeyPrivate">私鑰</param>
        /// <param name="strHashbyteSignature">待簽名Hash描述</param>
        /// <param name="strEncryptedSignatureData">簽名後的結果</param>
        /// <returns></returns>
        static public bool SignatureFormatter(string strKeyPrivate, string strHashbyteSignature, ref string strEncryptedSignatureData)
        {
            try
            {
                byte[] HashbyteSignature;
                byte[] EncryptedSignatureData;
                HashbyteSignature = Convert.FromBase64String(strHashbyteSignature);
                System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
                RSA.FromXmlString(strKeyPrivate);
                System.Security.Cryptography.RSAPKCS1SignatureFormatter RSAFormatter = new System.Security.Cryptography.RSAPKCS1SignatureFormatter(RSA);
                //設置簽名的算法為SHA256 
                RSAFormatter.SetHashAlgorithm("SHA256");
                //執行簽名 
                EncryptedSignatureData = RSAFormatter.CreateSignature(HashbyteSignature);
                strEncryptedSignatureData = Convert.ToBase64String(EncryptedSignatureData);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// RSA簽名
        /// </summary>
        /// <param name="text">待加密文本</param>
        /// <param name="privateKey">私鑰</param>
        /// <returns></returns>
        public static string RsaSign(string text, string privateKey)
        {
            var bytesToEncrypt = Encoding.UTF8.GetBytes(text);
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKey);
            var sha = SHA256.Create();
            var encrypt = rsa.SignData(bytesToEncrypt, sha);
            return Convert.ToBase64String(encrypt);
        }
        #endregion

        #region RSA 簽名驗證
        /// <summary>
        /// RSA簽名驗證
        /// </summary>
        /// <param name="strKeyPublic">公鑰</param>
        /// <param name="HashbyteDeformatter">Hash描述</param>
        /// <param name="DeformatterData">簽名後的結果</param>
        /// <returns></returns>
        static public bool SignatureDeformatter(string strKeyPublic, byte[] HashbyteDeformatter, byte[] DeformatterData)
        {
            try
            {
                System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
                RSA.FromXmlString(strKeyPublic);
                System.Security.Cryptography.RSAPKCS1SignatureDeformatter RSADeformatter = new System.Security.Cryptography.RSAPKCS1SignatureDeformatter(RSA);
                //指定解密的時候HASH算法為SHA256 
                RSADeformatter.SetHashAlgorithm("SHA256");
                if (RSADeformatter.VerifySignature(HashbyteDeformatter, DeformatterData))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// RSA簽名驗證
        /// </summary>
        /// <param name="strKeyPublic">公鑰</param>
        /// <param name="strHashbyteDeformatter">Hash描述</param>
        /// <param name="DeformatterData">簽名後的結果</param>
        /// <returns></returns>
        static public bool SignatureDeformatter(string strKeyPublic, string strHashbyteDeformatter, byte[] DeformatterData)
        {
            try
            {
                byte[] HashbyteDeformatter;
                HashbyteDeformatter = Convert.FromBase64String(strHashbyteDeformatter);
                System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
                RSA.FromXmlString(strKeyPublic);
                System.Security.Cryptography.RSAPKCS1SignatureDeformatter RSADeformatter = new System.Security.Cryptography.RSAPKCS1SignatureDeformatter(RSA);
                //指定解密的時候HASH算法為SHA256 
                RSADeformatter.SetHashAlgorithm("SHA256");
                if (RSADeformatter.VerifySignature(HashbyteDeformatter, DeformatterData))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// RSA簽名驗證
        /// </summary>
        /// <param name="strKeyPublic">公鑰</param>
        /// <param name="HashbyteDeformatter">Hash描述</param>
        /// <param name="strDeformatterData">簽名後的結果</param>
        /// <returns></returns>
        static public bool SignatureDeformatter(string strKeyPublic, byte[] HashbyteDeformatter, string strDeformatterData)
        {
            try
            {
                byte[] DeformatterData;
                System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
                RSA.FromXmlString(strKeyPublic);
                System.Security.Cryptography.RSAPKCS1SignatureDeformatter RSADeformatter = new System.Security.Cryptography.RSAPKCS1SignatureDeformatter(RSA);
                //指定解密的時候HASH算法為SHA256 
                RSADeformatter.SetHashAlgorithm("SHA256");
                DeformatterData = Convert.FromBase64String(strDeformatterData);
                if (RSADeformatter.VerifySignature(HashbyteDeformatter, DeformatterData))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// RSA簽名驗證
        /// </summary>
        /// <param name="strKeyPublic">公鑰</param>
        /// <param name="strHashbyteDeformatter">Hash描述</param>
        /// <param name="strDeformatterData">簽名後的結果</param>
        /// <returns></returns>
        static public bool SignatureDeformatter(string strKeyPublic, string strHashbyteDeformatter, string strDeformatterData)
        {
            try
            {
                byte[] DeformatterData;
                byte[] HashbyteDeformatter;
                HashbyteDeformatter = Convert.FromBase64String(strHashbyteDeformatter);
                System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider();
                RSA.FromXmlString(strKeyPublic);
                System.Security.Cryptography.RSAPKCS1SignatureDeformatter RSADeformatter = new System.Security.Cryptography.RSAPKCS1SignatureDeformatter(RSA);
                //指定解密的時候HASH算法為SHA256 
                RSADeformatter.SetHashAlgorithm("SHA256");
                DeformatterData = Convert.FromBase64String(strDeformatterData);
                if (RSADeformatter.VerifySignature(HashbyteDeformatter, DeformatterData))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// rsa驗證簽名
        /// </summary>
        /// <param name="txt">明文</param>
        /// <param name="sign">簽名串</param>
        /// <param name="pubKey">公鑰</param>
        /// <returns></returns>
        public static bool RsaVerifyData(string txt, string sign, string pubKey)
        {
            var soures = Encoding.UTF8.GetBytes(txt);
            var bytes = Convert.FromBase64String(sign);

            var rsa = new RSACryptoServiceProvider(2048);
            rsa.FromXmlString(pubKey);
            SHA256 sha = SHA256.Create();

            return rsa.VerifyData(soures, sha, bytes);
        }
        #endregion

        #endregion

        #region 公鑰私鑰DotNet和Java互轉

        /// <summary> 
        /// RSA私鑰格式轉換，java->.net 
        /// </summary> 
        /// <param name="privateKey">java生成的RSA私鑰</param> 
        /// <returns></returns> 
        public static string RSAPrivateKeyJava2DotNet(string privateKey)
        {
            RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));

            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
            Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned()),
            Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned()),
            Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned()),
            Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned()),
            Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned()),
            Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned()),
            Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned()),
            Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned()));
        }

        /// <summary> 
        /// RSA私鑰格式轉換，.net->java 
        /// </summary> 
        /// <param name="privateKey">.net生成的私鑰</param> 
        /// <returns></returns> 
        public static string RSAPrivateKeyDotNet2Java(string privateKey)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(privateKey);
            BigInteger m = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Modulus")[0].InnerText));
            BigInteger exp = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Exponent")[0].InnerText));
            BigInteger d = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("D")[0].InnerText));
            BigInteger p = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("P")[0].InnerText));
            BigInteger q = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Q")[0].InnerText));
            BigInteger dp = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("DP")[0].InnerText));
            BigInteger dq = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("DQ")[0].InnerText));
            BigInteger qinv = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("InverseQ")[0].InnerText));

            RsaPrivateCrtKeyParameters privateKeyParam = new RsaPrivateCrtKeyParameters(m, exp, d, p, q, dp, dq, qinv);

            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKeyParam);
            byte[] serializedPrivateBytes = privateKeyInfo.ToAsn1Object().GetEncoded();
            return Convert.ToBase64String(serializedPrivateBytes);
        }

        /// <summary> 
        /// RSA公鑰格式轉換，java->.net 
        /// </summary> 
        /// <param name="publicKey">java生成的公鑰</param> 
        /// <returns></returns> 
        public static string RSAPublicKeyJava2DotNet(string publicKey)
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
            Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
            Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
        }

        /// <summary> 
        /// RSA公鑰格式轉換，.net->java 
        /// </summary> 
        /// <param name="publicKey">.net生成的公鑰</param> 
        /// <returns></returns> 
        public static string RSAPublicKeyDotNet2Java(string publicKey)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(publicKey);
            BigInteger m = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Modulus")[0].InnerText));
            BigInteger p = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Exponent")[0].InnerText));
            RsaKeyParameters pub = new RsaKeyParameters(false, m, p);

            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pub);
            byte[] serializedPublicBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
            return Convert.ToBase64String(serializedPublicBytes);
        }

        /// <summary>
        /// Xml轉換成Der,傳入私鑰文檔格式
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static byte[] RSAPrivateKeytoJava(string privateKey)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(privateKey);
            BigInteger m = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Modulus")[0].InnerText));
            BigInteger exp = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Exponent")[0].InnerText));
            BigInteger d = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("D")[0].InnerText));
            BigInteger p = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("P")[0].InnerText));
            BigInteger q = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Q")[0].InnerText));
            BigInteger dp = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("DP")[0].InnerText));
            BigInteger dq = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("DQ")[0].InnerText));
            BigInteger qinv = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("InverseQ")[0].InnerText));

            RsaPrivateCrtKeyParameters privateParam = new RsaPrivateCrtKeyParameters(m, exp, d, p, q, dp, dq, qinv);

            var publicKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateParam);
            byte[] serializedPublicBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
            return serializedPublicBytes;
        }

        /// <summary>
        /// Xml轉換成Der,傳入公鑰文檔格式
        /// </summary>
        /// <param name="publickey"></param>
        /// <returns></returns>
        public static byte[] RSAPublicKeytoJava(string publickey)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(publickey);
            BigInteger m = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Modulus")[0].InnerText));
            BigInteger p = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Exponent")[0].InnerText));
            RsaKeyParameters pub = new RsaKeyParameters(false, m, p);

            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pub);
            byte[] serializedPublicBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
            return serializedPublicBytes;
        }
        #endregion
    }
}