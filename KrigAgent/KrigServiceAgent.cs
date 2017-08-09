//------------------------------------------------------------------------------
//----- ServiceAgent -------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2017 WiM - USGS

//    authors:  Jeremy K. Newson USGS Web Informatics and Mapping
//              
//  
//   purpose:   The service agent is responsible for initiating the service call, 
//              capturing the data that's returned and forwarding the data back to 
//              the requestor.
//
//discussion:   delegated hunting and gathering responsibilities.   
//
// 

using System;


namespace KrigAgent
{
    public interface IKrigAgent
    {
        Boolean example {set; }
    }
    public class KrigServiceAgent : IKrigAgent
    {
        #region Properties
        public bool example { private get; set; }
        #endregion
        

        public KrigServiceAgent() {
            //initiallizations happen here
        }

        #region HELPER METHODS
        #endregion
        #region Enumerations
        #endregion
    }
}
