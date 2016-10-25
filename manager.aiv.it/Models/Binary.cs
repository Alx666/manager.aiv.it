using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace manager.aiv.it
{
    public partial class Binary
    {
        public Binary(Binary hToCopy)
        {
            this.Data = hToCopy.Data;
            this.Filename = hToCopy.Filename;
            this.Id = hToCopy.Id;
        }

        public static Binary CreateFrom(HttpPostedFileBase hFile)
        {
            byte[] hBytes = new byte[hFile.InputStream.Length];
            hFile.InputStream.Read(hBytes, 0, hBytes.Length);

            FileInfo hFInfo = new FileInfo(hFile.FileName);

            Binary hBin     = new Binary();
            hBin.Data       = hBytes;
            hBin.Filename   = $"{StringExtensions.Random(15)}{hFInfo.Extension}";

            return hBin;          
        }
    }
}