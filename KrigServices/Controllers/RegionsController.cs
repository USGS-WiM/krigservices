//------------------------------------------------------------------------------
//----- HttpController ---------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2017 WiM - USGS

//    authors:  Jeremy K. Newson USGS Web Informatics and Mapping
//              
//  
//   purpose:   Handles resources through the HTTP uniform interface.
//
//discussion:   Controllers are objects which handle all interaction with resources. 
//              
//
// 

using Microsoft.AspNetCore.Mvc;
using System;
using KrigAgent;
using System.Threading.Tasks;
using System.Collections.Generic;
using KrigAgent.Resources;
using System.Linq;
using KrigServices.ServiceAgents;
using Microsoft.Extensions.Options;
using KrigServices.Resources;

namespace KrigServices.Controllers
{
    [Route("[controller]")]
    public class RegionsController : KrigControllerBase
    {
        public RegionsController(IKrigAgent sa) : base(sa)
        {
        }
        #region METHODS
        [HttpGet()]
        public async Task<IActionResult> Get()
            {
                try
                {
                    return Ok(agent.AvailableResources());
                }
                catch (Exception ex)
                {
                    return HandleException(ex);
                }
            }
        #endregion
        #region HELPER METHODS
        #endregion
    }
}
