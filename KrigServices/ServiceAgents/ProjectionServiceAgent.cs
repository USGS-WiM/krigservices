using System;
using WiM.Utilities.ServiceAgent;


namespace KrigServices.ServiceAgents
{
    public class ProjectionServiceAgent: ServiceAgentBase
    {
        #region Properties
        public bool HasGeometry { get; private set; }
        #endregion
        #region Constructors
        public ProjectionServiceAgent(string baseURL)
            : base(baseURL)
        {
            HasGeometry = false;
        }
        #endregion
        #region Methods
        public bool ProjectPointAsync(ref double x, ref double y, string fromSRC, string toSRC)
        {
            dynamic result = null;
            dynamic geom = null;
            String state = string.Empty;
            string msg;

            try
            {
                //project?inSR=4326&outSR=26915&geometries={geometries:[{x:-93.9508,y:42.0191}],geometryType:esriGeometryPoint}f=pjson
                string urlString = String.Format(getURI(serviceType.e_projection), fromSRC, toSRC, x, y);

                result = this.ExecuteAsync<dynamic>(new RequestInfo(urlString)).Result;

                if (isDynamicError(result, out msg)) throw new Exception(msg);

                geom = result["geometries"][0];

                x = geom.Value<double>("x");
                y = geom.Value<double>("y");

                return true;

            }
            catch (Exception ex)
            {
                x = -999;
                y = -999;
                return false;
            }
        }//end PostFeatures
        #endregion
        #region Helper Methods

        private String getURI(serviceType sType)
        {
            string uri = string.Empty;
            switch (sType)
            {
                case serviceType.e_projection:
                    uri = "project?inSR={0}&outSR={1}&geometries={{geometries:[{{x:{2},y:{3}}}],geometryType:esriGeometryPoint}}&f=pjson";
                    break;
            }

            return uri;
        }//end getURL

        private Boolean isDynamicError(dynamic obj, out string msg)
        {
            msg = string.Empty;
            try
            {
                var error = obj.error;
                if (error == null) throw new Exception();
                msg = error.message;
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }

        }

        #endregion
        #region Enumerations
        public enum serviceType
        {
            e_projection
        }

        #endregion
    }
}
