//------------------------------------------------------------------------------
//----- Configuration -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Jeremy Newson          
//  
//   purpose:   Configuration implements the IConfiurationSource interface. OpenRasta
//              will call the Configure method and use it to configure the application 
//              through a fluent interface using the Resource space as root objects. 
//
//discussion:   The ResourceSpace is where you can define the resources in the application and what
//              handles them and how thy are represented. 
//              https://github.com/openrasta/openrasta/wiki/Configuration
//
//     
#region Comments
// 12.09.13 - JKN - Created
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using OpenRasta.Authentication;
using OpenRasta.Authentication.Basic;
using OpenRasta.Codecs;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.IO;
using OpenRasta.Pipeline.Contributors;
using OpenRasta.Web.UriDecorators;

using KrigServices.Resources;
using KrigServices.Handlers;
using KrigServices.Codecs;

using WiM.PipeLineContributors;

namespace KrigServices
{
    public class Configuration : IConfigurationSource
    {
        public void Configure()
        {
            using (OpenRastaConfiguration.Manual)
            {
                // Allow codec choice by extension 
                ResourceSpace.Uses.PipelineContributor<CrossDomainPipelineContributor>();

                //krig
                ResourceSpace.Has.ResourcesOfType<List<Site>>()
                .AtUri("/krig?state={state}&xlocation={x}&ylocation={Y}&sr={wkid}&count={count}")
                .HandledBy<KrigHandler>()
                .TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");
                //.TranscodedBy<UTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")            
            }//end using
        }//end Configure
    }//end class Configuration
}//end Namespace