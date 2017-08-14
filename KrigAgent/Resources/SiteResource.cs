//------------------------------------------------------------------------------
//----- Example ----------------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2017 WiM - USGS

//    authors:  Jeremy K. Newson USGS Web Informatics and Mapping
//              
//  
//   purpose:   Represents Wateruse Configureation
//
//discussion:   Simple POCO object class  
//
// 
using System;
using System.Collections.Generic;


namespace KrigAgent.Resources
{
    public class SiteResource
    {
        public bool Prop1 { get; set; }
        public Int32 Prop2 { get; set; }
    }

    public class Site
    {
        #region Properties
        
        public String ID { get; set; }
        public String Name { get; set; }
        public Double LocationX { get; set; }
        public Double LocationY { get; set; }
        public Double DrainageArea { get; set; }
        public Double Correlation { get; set; }
        #endregion
        #region Constructor
        public Site()
            : this(String.Empty, String.Empty, Double.NaN, Double.NaN, Double.NaN)
        { }// end Site

        public Site(String SID, String name, double X,
                    double Y, double DA)
            : this(SID, name, X, Y, DA, Double.NaN)
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
    public class KrigIndexSite : Site
    {
        #region Properties
        public double DistanceToUngagedPoint { get; set; }
        public double partialSillSigma { get; private set; }
        public double rangeParameterA { get; private set; }
        public IDictionary<String, Double> Correlations { get; private set; }
        #endregion
        #region Constructor
        public KrigIndexSite()
            : this(String.Empty, String.Empty, Double.NaN, Double.NaN, Double.NaN, Double.NaN, Double.NaN, null)
        { }
        public KrigIndexSite(String id, String name, Double X,
                        Double Y, Double DA, Double sigma,
                        Double rangeParam, IDictionary<String, Double> correlationList)
            : base(id, name, X, Y, DA)
        {
            this.partialSillSigma = sigma;
            this.rangeParameterA = rangeParam;
            this.Correlations = correlationList;
        }
        #endregion
    }//end Class SiteDetails
}
