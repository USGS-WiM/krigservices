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

namespace KrigServices.Controllers
{
    [Route("[controller]")]
    public class KrigController : KrigControllerBase
    {
        public KrigController(IKrigAgent sa) : base(sa)
        {}
        #region METHODS
        [HttpGet("test")]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok("This is an example result");
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync(ex);
            }

        }

        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] string state, [FromQuery] double x, [FromQuery] double y, [FromQuery] string wkid, [FromQuery] Int32 count = 5)
            {
                //ProjectionServiceAgent sa = null;
                List<Site> gageList = null;

                try
                {
                    if (x == 0 || y == 0 || String.IsNullOrEmpty(wkid)|| String.IsNullOrEmpty(state))                
                        return new BadRequestObjectResult("One or more of the parameters are invalid.");

                    if (!agent.Load(state, count)) throw new Exception("Krig failed to load.");

                    if (!string.Equals(wkid.Trim(), agent.SR.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        var sa = new ProjectionServiceAgent("https://gis.wim.usgs.gov/arcgis/rest/services/Utilities/Geometry/GeometryServer/");
                        if (!sa.ProjectPointAsync(ref x, ref y, wkid, agent.SR)) throw new Exception("Failed to project point. try passing in sr of " + agent.SR);
                    }//end if

                    if (agent.IndexGages.Count < 1) return new BadRequestObjectResult(new Error(errorEnum.e_error, "No Index Gages"));

                    if (!agent.Execute(x, y)) throw new Exception("Kriging Failed" );

                    gageList = agent.TopCorrelatedGages.Select(g => new Site(g.Value.ID,
                                                                             g.Value.Name,
                                                                             g.Value.LocationX,
                                                                             g.Value.LocationY,
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
        public string GetState(State state)
        {
            string result = string.Empty;
            switch (state)
            {
                case State.AL:
                    result = "ALABAMA";
                    break;
                case State.AK:
                    result = "ALASKA";
                    break;
                case State.AS:
                    result = "AMERICAN SAMOA";
                    break;
                case State.AZ:
                    result = "ARIZONA";
                    break;
                case State.AR:
                    result = "ARKANSAS";
                    break;
                case State.CA:
                    result = "CALIFORNIA";
                    break;
                case State.CO:
                    result = "COLORADO";
                    break;
                case State.CT:
                    result = "CONNECTICUT";
                    break;
                case State.DE:
                    result = "DELAWARE";
                    break;
                case State.DC:
                    result = "DISTRICT OF COLUMBIA";
                    break;
                case State.FM:
                    result = "FEDERATED STATES OF MICRONESIA";
                    break;
                case State.FL:
                    result = "FLORIDA";
                    break;
                case State.GA:
                    result = "GEORGIA";
                    break;
                case State.GU:
                    result = "GUAM";
                    break;
                case State.HI:
                    result = "HAWAII";
                    break;
                case State.ID:
                    result = "IDAHO";
                    break;
                case State.IL:
                    result = "ILLINOIS";
                    break;
                case State.IN:
                    result = "INDIANA";
                    break;
                case State.IA:
                    result = "IOWA";
                    break;
                case State.KS:
                    result = "KANSAS";
                    break;
                case State.KY:
                    result = "KENTUCKY";
                    break;
                case State.LA:
                    result = "LOUISIANA";
                    break;
                case State.ME:
                    result = "MAINE";
                    break;
                case State.MH:
                    result = "MARSHALL ISLANDS";
                    break;
                case State.MD:
                    result = "MARYLAND";
                    break;
                case State.MA:
                    result = "MASSACHUSETTS";
                    break;
                case State.MI:
                    result = "MICHIGAN";
                    break;
                case State.MN:
                    result = "MINNESOTA";
                    break;
                case State.MS:
                    result = "MISSISSIPPI";
                    break;
                case State.MO:
                    result = "MISSOURI";
                    break;
                case State.MT:
                    result = "MONTANA";
                    break;
                case State.NE:
                    result = "NEBRASKA";
                    break;
                case State.NV:
                    result = "NEVADA";
                    break;
                case State.NH:
                    result = "NEW HAMPSHIRE";
                    break;
                case State.NJ:
                    result = "NEW JERSEY";
                    break;
                case State.NM:
                    result = "NEW MEXICO";
                    break;
                case State.NY:
                    result = "NEW YORK";
                    break;
                case State.NC:
                    result = "NORTH CAROLINA";
                    break;
                case State.ND:
                    result = "NORTH DAKOTA";
                    break;
                case State.MP:
                    result = "NORTHERN MARIANA ISLANDS";
                    break;
                case State.OH:
                    result = "OHIO";
                    break;
                case State.OK:
                    result = "OKLAHOMA";
                    break;
                case State.OR:
                    result = "OREGON";
                    break;
                case State.PW:
                    result = "PALAU";
                    break;
                case State.PA:
                    result = "PENNSYLVANIA";
                    break;
                case State.PR:
                    result = "PUERTO RICO";
                    break;
                case State.RI:
                    result = "RHODE ISLAND";
                    break;
                case State.SC:
                    result = "SOUTH CAROLINA";
                    break;
                case State.SD:
                    result = "SOUTH DAKOTA";
                    break;
                case State.TN:
                    result = "TENNESSEE";
                    break;
                case State.TX:
                    result = "TEXAS";
                    break;
                case State.UT:
                    result = "UTAH";
                    break;
                case State.VT:
                    result = "VERMONT";
                    break;
                case State.VI:
                    result = "VIRGIN ISLANDS";
                    break;
                case State.VA:
                    result = "VIRGINIA";
                    break;
                case State.WA:
                    result = "WASHINGTON";
                    break;
                case State.WV:
                    result = "WEST VIRGINIA";
                    break;
                case State.WI:
                    result = "WISCONSIN";
                    break;
                case State.WY:
                    result = "WYOMING";
                    break;
                default:
                    result = string.Empty;
                    break;
            }//end switch

            return result;
        }
        public State GetStateByName(string name)
        {
            try
            {
                switch (name.ToUpper())
                {
                    case "ALABAMA":
                    case "AL":
                    case "ALA":
                        return State.AL;

                    case "ALASKA":
                    case "AK":
                        return State.AK;

                    case "AMERICAN SAMOA":
                    case "AS":
                        return State.AS;

                    case "ARIZONA":
                    case "AZ":
                    case "ARIZ":
                        return State.AZ;

                    case "ARKANSAS":
                    case "AR":
                    case "ARK":
                        return State.AR;

                    case "CALIFORNIA":
                    case "CA":
                    case "CALIF":
                        return State.CA;

                    case "COLORADO":
                    case "CO":
                    case "COLO":
                        return State.CO;

                    case "CONNECTICUT":
                    case "CT":
                    case "CONN":
                        return State.CT;

                    case "DELAWARE":
                    case "DE":
                    case "DEL":
                        return State.DE;

                    case "DISTRICT OF COLUMBIA":
                    case "DC":
                    case "D.C.":
                        return State.DC;

                    case "FEDERATED STATES OF MICRONESIA":
                    case "FM":
                    case "FSM":
                        return State.FM;

                    case "FLORIDA":
                    case "FL":
                    case "FLA":
                        return State.FL;

                    case "GEORGIA":
                    case "GA":
                        return State.GA;

                    case "GUAM":
                    case "GU":
                        return State.GU;

                    case "HAWAII":
                    case "HI":
                        return State.HI;

                    case "IDAHO":
                    case "ID":
                        return State.ID;

                    case "ILLINOIS":
                    case "IL":
                    case "ILL.":
                        return State.IL;

                    case "INDIANA":
                    case "IN":
                    case "IND":
                        return State.IN;

                    case "IOWA":
                    case "IA":
                        return State.IA;

                    case "KANSAS":
                    case "KS":
                    case "KANS":
                        return State.KS;

                    case "KENTUCKY":
                    case "KY":
                        return State.KY;

                    case "LOUISIANA":
                    case "LA":
                        return State.LA;

                    case "MAINE":
                    case "ME":
                        return State.ME;

                    case "MARSHALL ISLANDS":
                    case "MH":
                        return State.MH;

                    case "MARYLAND":
                    case "MD":
                        return State.MD;

                    case "MASSACHUSETTS":
                    case "MA":
                    case "MASS":
                        return State.MA;

                    case "MICHIGAN":
                    case "MI":
                    case "MICH":
                        return State.MI;

                    case "MINNESOTA":
                    case "MN":
                    case "MINN":
                        return State.MN;

                    case "MISSISSIPPI":
                    case "MS":
                    case "MISS":
                        return State.MS;

                    case "MISSOURI":
                    case "MO":
                        return State.MO;

                    case "MONTANA":
                    case "MT":
                    case "MONT":
                        return State.MT;

                    case "NEBRASKA":
                    case "NE":
                    case "NEBR":
                        return State.NE;

                    case "NEVADA":
                    case "NV":
                    case "NEV":
                        return State.NV;

                    case "NEW HAMPSHIRE":
                    case "NH":
                        return State.NH;

                    case "NEW JERSEY":
                    case "NJ":
                        return State.NJ;

                    case "NEW MEXICO":
                    case "NM":
                        return State.NM;

                    case "NEW YORK":
                    case "NY":
                        return State.NY;

                    case "NORTH CAROLINA":
                    case "NC":
                        return State.NC;

                    case "NORTH DAKOTA":
                    case "ND":
                        return State.ND;

                    case "NORTHERN MARIANA ISLANDS":
                    case "MP":
                        return State.MP;

                    case "OHIO":
                    case "OH":
                        return State.OH;

                    case "OKLAHOMA":
                    case "OK":
                    case "OKLA":
                        return State.OK;

                    case "OREGON":
                    case "OR":
                    case "ORE":
                        return State.OR;

                    case "PALAU":
                    case "PW":
                        return State.PW;

                    case "PENNSYLVANIA":
                    case "PA":
                        return State.PA;

                    case "PUERTO RICO":
                    case "PR":
                        return State.PR;

                    case "RHODE ISLAND":
                    case "RI":
                        return State.RI;

                    case "SOUTH CAROLINA":
                    case "SC":
                        return State.SC;

                    case "SOUTH DAKOTA":
                    case "SD":
                        return State.SD;

                    case "TENNESSEE":
                    case "TN":
                    case "TENN":
                        return State.TN;

                    case "TEXAS":
                    case "TX":
                    case "TEX":
                        return State.TX;

                    case "UTAH":
                    case "UT":
                        return State.UT;

                    case "VERMONT":
                    case "VT":
                        return State.VT;

                    case "VIRGIN ISLANDS":
                    case "VI":
                        return State.VI;

                    case "VIRGINIA":
                    case "VA":
                        return State.VA;

                    case "WASHINGTON":
                    case "WA":
                    case "WASH":
                        return State.WA;

                    case "WEST VIRGINIA":
                    case "WV":
                    case "W.VA":
                        return State.WV;

                    case "WISCONSIN":
                    case "WI":
                    case "WIS":
                        return State.WI;

                    case "WYOMING":
                    case "WY":
                    case "WYO":
                        return State.WY;
                }// end switch

                throw new Exception("Not Available");

            }
            catch (Exception)
            {
                return State.UNSPECIFIED;
            }//end try
        }
        public enum State
        {

            UNSPECIFIED,
            AL,
            AK,
            AS,
            AZ,
            AR,
            CA,
            CO,
            CT,
            DE,
            DC,
            FM,
            FL,
            GA,
            GU,
            HI,
            ID,
            IL,
            IN,
            IA,
            KS,
            KY,
            LA,
            ME,
            MH,
            MD,
            MA,
            MI,
            MN,
            MS,
            MO,
            MT,
            NE,
            NV,
            NH,
            NJ,
            NM,
            NY,
            NC,
            ND,
            MP,
            OH,
            OK,
            OR,
            PW,
            PA,
            PR,
            RI,
            SC,
            SD,
            TN,
            TX,
            UT,
            VT,
            VI,
            VA,
            WA,
            WV,
            WI,
            WY
        }
    #endregion
    }
}
