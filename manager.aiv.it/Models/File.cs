using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;


namespace manager.aiv.it.Models
{
    public partial class File
    {

        public static aiv.it.File CreateOnDisk(HttpServerUtilityBase hServer, HttpPostedFileBase hFile)
        {
            string sPath = hServer.MapPath("~\\FileStorage");

            if (!Directory.Exists(sPath))
                Directory.CreateDirectory(sPath);

            aiv.it.File hToStore = new aiv.it.File();

            using (MemoryStream hMs = new MemoryStream())
            {
                hFile.InputStream.CopyTo(hMs);
                byte[] hData = hMs.ToArray();

                hToStore.ArchivationName = AivExtensions.RandomString(50) + ".dat";
                hToStore.OriginalName    = Path.GetFileName(hFile.FileName);

                using (FileStream hFs = System.IO.File.OpenWrite(sPath + "\\" + hToStore.ArchivationName))
                {
                    hFs.Write(hData, 0, hData.Length);
                    hFs.Flush();
                }
            }

            return hToStore;
        }


        public static aiv.it.File DeleteOnDisk(AivEntities hDb, aiv.it.File hFileRef, HttpServerUtilityBase hServer)
        {
            //string sPath = hServer.MapPath("~\\FileStorage\\") + hFileRef.ArchivationName;

            //if(System.IO.File.Exists(sPath))
            //{
            //    try
            //    {

            //    }
            //}

            throw new NotImplementedException();
            
        }
    }
}