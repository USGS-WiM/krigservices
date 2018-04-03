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
    public class SitesController : KrigControllerBase
    {
        private ProjectionSettings projectionSettings { get; set; }
        public SitesController(IKrigAgent sa, IOptions<ProjectionSettings> resource) : base(sa)
        {
            this.projectionSettings = resource.Value;
        }
        #region METHODS
        [HttpGet("{state}/krig")]
        [HttpGet("/krig")]
        public async Task<IActionResult> Get(string state, double x, double y, string crs, [FromQuery] Int32 count = 5)
            {
                //ProjectionServiceAgent sa = null;
                List<Site> gageList = null;

                try
                {
                    if (x == 0 || y == 0 || String.IsNullOrEmpty(crs) || String.IsNullOrEmpty(state))                
                        return new BadRequestObjectResult("One or more of the parameters are invalid.");

                    if (!agent.Load(state, count)) throw new Exception("Krig failed to load.");

                    if (!string.Equals(crs.Trim(), agent.SR.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        var sa = new ProjectionServiceAgent(this.projectionSettings);
                        if (!sa.ProjectPointAsync(ref x, ref y, crs, agent.SR)) throw new Exception("Failed to project point. try passing in sr of " + agent.SR);
                    }//end if

                    if (agent.IndexGages.Count < 1) return new BadRequestObjectResult(new Error(errorEnum.e_error, "No Index Gages"));

                    if (!agent.Execute(x, y)) throw new Exception("Kriging Failed" );

                    gageList = agent.TopCorrelatedGages.Select(g => new Site(g.Value.ID,
                                                                             g.Value.Name,
                                                                             g.Value.X,
                                                                             g.Value.Y,
                                                                             g.Value.crs,
                                                                             g.Value.DrainageArea,
                                                                             g.Key
                                                                             )).ToList();

                    return Ok( gageList );
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
