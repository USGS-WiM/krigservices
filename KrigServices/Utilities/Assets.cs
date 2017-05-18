//------------------------------------------------------------------------------
//----- Asset ------------------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   
//           
//discussion: 
//

#region "Comments"
//12.20.2013 jkn - Created
#endregion

#region "Imports"
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Web;
#endregion
namespace KrigServices.Utilities
{
    public class Assets
    {
        #region "Properties"
        public string AssetDirectory { get; set; }
        #endregion
        #region "Constructor and IDisposable Support"
        public Assets(string anAssetName)
        {
            AssetDirectory = anAssetName + "/Assets/Data";
        }//end Storage
        #region "IDisposable Support"
        #endregion
        #endregion
        #region "Methods"
        public void PutObject(String ObjectName, Stream aStream)
        {
            string directory = Path.Combine(AssetDirectory,Path.GetDirectoryName(ObjectName));
            try
            {
                if (!Directory.Exists(Path.Combine(directory)))
                    Directory.CreateDirectory(directory);

                using (var fileStream = File.Create(Path.Combine(AssetDirectory,ObjectName)))
                {
                    //reset stream position to 0 prior to copying to filestream;
                    aStream.Position = 0;
                    aStream.CopyTo(fileStream);
                }//end using

            }
            catch (Exception)
            {
                
            }
        }
                     

        //Download Object
        public Stream GetObject(String ObjectName)
        {
            string objfile = Path.Combine(AssetDirectory, ObjectName);
            try
            {
                return File.OpenRead(objfile);
            }
            catch (Exception)
            {
                return null;
            }

        }//end GetObject

        //Delete Object
        public Boolean DeleteObject(String ObjectName)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception)
            {
               return false;
            }
        }
    
        #endregion
        #region "Helper Methods"

        #endregion
    }//end class Storage
}//end namespace

