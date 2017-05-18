//------------------------------------------------------------------------------
//----- JsonDotNetCodec -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//  
//   purpose:   Created a JSON Codec that works with EF. JsonDataContractCodec 
//              does not work because IsReference is set
//
//discussion:   A Codec is an enCOder/DECoder for a resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Codecs
//
//     

#region Comments
// 02.03.12 - JB - Created to properly de/serialize JSON
#endregion

using System;
using System.IO;
using System.Text;
using OpenRasta.TypeSystem;
using OpenRasta.Web;

using Newtonsoft.Json;

namespace OpenRasta.Codecs
{
    [MediaType("application/json;q=0.5", "json")]
    public class JsonDotNetCodec : IMediaTypeReader, IMediaTypeWriter
    {
        public object Configuration { get; set; }

        public object ReadFrom(IHttpEntity request, IType destinationType, string paramName)
        {
            if (destinationType.StaticType == null)
                throw new InvalidOperationException();

             // Create a serializer
            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader streamReader = new StreamReader(request.Stream, new UTF8Encoding(false, true) ))
            {
                using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                {
                    return serializer.Deserialize(jsonTextReader, destinationType.StaticType);
                }
            }
        }

        public void WriteTo(object entity, IHttpEntity response, string[] paramneters)
        {
            if (entity == null)
                return;

            using (JsonTextWriter jsonTextWriter = new JsonTextWriter(new StreamWriter(response.Stream, new UTF8Encoding(false, true))) { CloseOutput = false })
            {
                jsonTextWriter.Formatting = Formatting.Indented;
                
                // Create a serializer
                JsonSerializer serializer = new JsonSerializer();
                serializer.PreserveReferencesHandling = PreserveReferencesHandling.None;
                serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                serializer.MissingMemberHandling = MissingMemberHandling.Ignore;
                
                serializer.Serialize(jsonTextWriter, entity);
                jsonTextWriter.Flush();
            }
                        
        }

    }
}

