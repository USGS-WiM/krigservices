//------------------------------------------------------------------------------
//----- SiteResource -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Site resources.
//              Equivalent to the model in MVC.
//
//discussion:   Resources are plain-old CLR objects (POCO) the resources are POCO classes derived from the EF
//              SiteResource contains additional rederers of the derived EF POCO classes. 
//              https://github.com/openrasta/openrasta/wiki/Resources
//
//     

#region Comments
// 12.09.13 - jkn - Created
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Xml.Serialization;

namespace KrigServices.Resources
{
   
    public class Site
    {
        #region Properties
        [XmlElement(typeof(String), ElementName = "ID")]
        public String ID { get; set; }
        //[XmlElement(typeof(String), ElementName = "NAME")]
        public String Name { get; set; }
        //[XmlElement(typeof(Double), ElementName = "LOCATION_X")]
        public Double LocationX { get; set; }
        //[XmlElement(typeof(Double), ElementName = "LOCATION_Y")]
        public Double LocationY { get; set; }
        //[XmlElement(typeof(Double), ElementName = "DRAINAGE_AREA")]
        public Double DrainageArea { get; set; }
        public Double Correlation { get; set; }
        #endregion
        #region Constructor
        public Site()
            : this(String.Empty, String.Empty, Double.NaN, Double.NaN, Double.NaN)
        { }// end Site
        
        public Site(String SID, String name, double X,
                    double Y, double DA)
            : this(SID, name, X, Y, DA,Double.NaN)
        { }//end Site

        public Site(String SID, String name, double X,
            double Y, double DA, Double correlation)
        {
            this.ID = SID;
            this.Name = name;
            this.LocationX = X;
            this.LocationY = Y;
            this.DrainageArea = DA;
            this.Correlation = correlation;

        }//end Site
        #endregion  
    }//end Class

    public class KrigIndexSite:Site
    {
        #region Properties
        public double DistanceToUngagedPoint { get; set; }
        public double partialSillSigma { get; private set; }
        public double rangeParameterA { get; private set; }
        public IDictionary<String, Double> Correlations { get; private set; }
        #endregion
        #region Constructor
        public KrigIndexSite()
            : this(String.Empty, String.Empty, Double.NaN, Double.NaN, Double.NaN,Double.NaN,Double.NaN, null)
        { }
        public KrigIndexSite(String id, String name, Double X,
                        Double Y, Double DA, Double sigma,
                        Double rangeParam, IDictionary<String, Double> correlationList)
            :base(id,name,X,Y, DA)
        {
            this.partialSillSigma = sigma;
            this.rangeParameterA = rangeParam;
            this.Correlations = correlationList;
        }
        #endregion
    }//end Class SiteDetails
}//end Namespace