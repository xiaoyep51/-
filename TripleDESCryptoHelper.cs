using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace CommonHelper
{
    public class TripleDESCryptoHelper
    {
        private SymmetricAlgorithm _algorithm;

        public TripleDESCryptoHelper(string Key, string IV)
        {
            _algorithm = new TripleDESCryptoServiceProvider();

            _algorithm.Key = Encoding.UTF8.GetBytes(Key);
            _algorithm.IV = Encoding.UTF8.GetBytes(IV);
        }

        public string Encrypt(string strToEncrypt)
        {
            if (string.IsNullOrWhiteSpace(strToEncrypt))
            {
                return "";
            }

            try
            {
                Byte[] byteInput = Encoding.UTF8.GetBytes(strToEncrypt);

                using (MemoryStream outputStream = new MemoryStream())
                using (ICryptoTransform cryptoTransform = _algorithm.CreateEncryptor())
                using (CryptoStream cryptoStream = new CryptoStream(outputStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(byteInput, 0, byteInput.Length);
                    cryptoStream.FlushFinalBlock();

                    return Convert.ToBase64String(outputStream.ToArray());
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(ex.ToString());
                return "加密错误";
            }
        }

        public string Decrypt(string strToDecrypt)
        {
            if (string.IsNullOrWhiteSpace(strToDecrypt))
            {
                return "";
            }

            try
            {
                Byte[] byteInput = Convert.FromBase64String(strToDecrypt);

                using (MemoryStream outputStream = new MemoryStream())
                using (ICryptoTransform cryptoTransform = _algorithm.CreateDecryptor())
                using (CryptoStream cryptoStream = new CryptoStream(outputStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(byteInput, 0, byteInput.Length);
                    cryptoStream.FlushFinalBlock();

                    return Encoding.UTF8.GetString(outputStream.ToArray());
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(ex.ToString());
                return "解密错误";
            }
        }
    }

    public struct MobileEncrypKey
    {
        public const string KEY = "L~a!x@e#h$d%H^q&j*p(j)p_";
        public const string IV = "pvD^e$x%";
    }
}
