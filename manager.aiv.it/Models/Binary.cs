using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace manager.aiv.it
{
    public partial class Binary
    {
        public Binary(Binary hToCopy)
        {
            if(hToCopy != null)
            {
                this.Data = hToCopy.Data;
                this.Filename = hToCopy.Filename;
                this.Id = hToCopy.Id;
            }
        }



        public static Binary CreateFrom(HttpPostedFileBase hFile, bool bKeepFileName = false)
        {            
            FileInfo hFInfo = new FileInfo(hFile.FileName);
            string sExtension = hFInfo.Extension.ToLower();
            byte[] hBytes;



            if (sExtension == ".png" || sExtension == ".jpg")
            {
                int iNewWidth  = 256;
                int iNewHeight = 256;
                sExtension     = ".jpg";

                using (Image hImage = Image.FromStream(hFile.InputStream))
                {
                    Bitmap hDownsampled = new Bitmap(iNewWidth, iNewHeight);

                    using (Graphics hGfx = Graphics.FromImage(hDownsampled))
                    {
                        hGfx.SmoothingMode      = SmoothingMode.HighQuality;
                        hGfx.PixelOffsetMode    = PixelOffsetMode.HighQuality;
                        hGfx.CompositingQuality = CompositingQuality.HighQuality;
                        hGfx.InterpolationMode  = InterpolationMode.HighQualityBicubic;
                        hGfx.DrawImage(hImage, new Rectangle(0, 0, iNewWidth, iNewHeight), new Rectangle(0, 0, hImage.Width, hImage.Height), GraphicsUnit.Pixel);

                        using (MemoryStream hStream = new MemoryStream())
                        {
                            using (EncoderParameters hParams = new EncoderParameters(1))
                            {
                                ImageCodecInfo hJpgInfo = ImageCodecInfo.GetImageEncoders().Where(c => c.MimeType == "image/jpeg").First();

                                hParams.Param[0] = new EncoderParameter(Encoder.Quality, 70L);
                                hDownsampled.Save(hStream, hJpgInfo, hParams);
                            }

                            byte[] hDisposableBuffer = hStream.GetBuffer();
                            hBytes = new byte[hDisposableBuffer.Length];
                            Buffer.BlockCopy(hDisposableBuffer, 0, hBytes, 0, hDisposableBuffer.Length);
                        }
                    }
                }
            }
            else
            {
                hBytes = new byte[hFile.InputStream.Length];
                hFile.InputStream.Read(hBytes, 0, hBytes.Length);
            }

            Binary hBin     = new Binary();
            hBin.Data       = hBytes;

            if(!bKeepFileName)
                hBin.Filename = $"{AivExtensions.Random(15)}{sExtension}";
            else
                hBin.Filename = hFInfo.Name;

            return hBin;          
        }
    }
}