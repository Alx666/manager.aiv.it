using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace manager.aiv.it
{
    public static class StringExtensions
    {
        private static  string s_hChars = "abcdefghijklmnoqqrstuvwxyz0123456789";
        private static  Random s_hRand  = new System.Random();
        private const   string Password = "AivManager666!**";

        public static string Random(int iLength)
        {
            return new string(Enumerable.Repeat(s_hChars, iLength).Select(s => s[s_hRand.Next(s.Length)]).ToArray());
        }


        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }


        
        public static string Encrypt(this string plainText)
        {
            string cipherText   = "";

            byte[] salt = (from b in Enumerable.Range(0, 16) select (byte)s_hRand.Next()).ToArray();

            byte[] plainBytes = Encoding.Unicode.GetBytes(plainText);
            
            using (Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(Password, salt))
            {
                using (Aes aes = AesManaged.Create())
                {
                    aes.Key = pdb.GetBytes(aes.KeySize / 8);
                    aes.IV  = pdb.GetBytes(aes.BlockSize / 8);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(plainBytes, 0, plainBytes.Length);
                        }

                        byte[] cipherBytes = ms.ToArray();
                        byte[] saltedCipherBytes = new byte[salt.Length + cipherBytes.Length];

                        Array.Copy(salt, 0, saltedCipherBytes, 0, salt.Length);
                        Array.Copy(cipherBytes, 0, saltedCipherBytes, salt.Length, cipherBytes.Length);
                        cipherText = Convert.ToBase64String(saltedCipherBytes);
                    }

                    aes.Clear();
                }
            }

            return cipherText;
        }

        public static string Decrypt(this string cipherText)
        {

            string plainText = "";
            byte[] saltedCipherBytes    = Convert.FromBase64String(cipherText);
            byte[] salt                 = new byte[16];
            byte[] cipherBytes          = new byte[saltedCipherBytes.Length - salt.Length];

            Array.Copy(saltedCipherBytes, 0, salt, 0, salt.Length);
            Array.Copy(saltedCipherBytes, salt.Length, cipherBytes, 0, saltedCipherBytes.Length - salt.Length);

            using (Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(Password, salt))
            {
                using (Aes aes = AesManaged.Create())
                {
                    aes.Key = pdb.GetBytes(aes.KeySize / 8);
                    aes.IV  = pdb.GetBytes(aes.BlockSize / 8);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                        }

                        plainText = Encoding.Unicode.GetString(ms.ToArray());
                    }

                    aes.Clear();
                }
            }

            return plainText;
        }

    }
}