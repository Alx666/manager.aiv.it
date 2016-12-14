using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace manager.aiv.it
{
    public static class AivExtensions
    {
        private static  string s_hChars = "abcdefghijklmnoqqrstuvwxyz0123456789";
        private static  Random s_hRand  = new System.Random();


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


        #region Cryptography

        private const string Password = "AivManager666!**";

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

        #endregion


        public static MvcHtmlString SearchBar(this HtmlHelper hHelper, string actionName, string controllerName, FormMethod method, IEnumerable hEnum, string sIdField, string sNameField)
        {
            StringBuilder hSb = new StringBuilder();

            hSb.AppendLine($"<form action=\"/{controllerName}\" class=\"filters\" method=\"{method.ToString().ToLower()}\">");
                hSb.AppendLine("<div class=\"row\">");
                    hSb.AppendLine("<div class=\"col-xs-12 col-sm-12 col-md-6\">");
                        hSb.AppendLine("<div class=\"input-group\">");
                            hSb.AppendLine("<input id=\"search\" type=\"text\" name=\"search\" placeholder=\"\" class=\"form-control\" value='@Request.QueryString[\"search\"]' />");
                            hSb.AppendLine("<span class=\"input-group-btn\">");
                                hSb.AppendLine("<button class=\"btn btn-info\" type=\"submit\">");
                                    hSb.AppendLine("<span class=\"glyphicon glyphicon-search\"></span>");
                                hSb.AppendLine("</button>");
                            hSb.AppendLine("</span>");
                        hSb.AppendLine("</div>");
                    hSb.AppendLine("</div>");
                    hSb.AppendLine("<div class=\"col-xs-12 col-sm-12 col-md-6\" layout-align=\"end\">");
                        hSb.AppendLine(hHelper.DropDownList("searchId", new SelectList(hEnum, sIdField, sNameField), "-", htmlAttributes: new { @class = "form-control" }).ToString());
                    hSb.AppendLine("</div>");
                hSb.AppendLine("</div>");
            hSb.AppendLine("</form>");

            return MvcHtmlString.Create(hSb.ToString());
        }

        /*
         * 1. Holy Wars... The Punishment Due
2. Symphony of Destruction
3. Hangar 18
4. Peace Sells
5. Rust In Peace... Polaris
6. Take No Prisoners
7. Washington Is Next <===
8. Good Mourning / Black Friday
9. Dawn Patrol
10. United Abomination
         */
    }
}