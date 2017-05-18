//------------------------------------------------------------------------------
//----- ServiceAgent -------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
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
using System.Configuration;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Threading;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RestSharp.Serializers;
using RestSharp;

namespace KrigServices.Utilities
{
    public class ProjectionServiceAgent : ServiceAgentBase
    {
        #region Properties
        public bool HasGeometry { get; private set; }
        #endregion

        #region Constructors
        public ProjectionServiceAgent()
            : base(ConfigurationManager.AppSettings["geomService"])
        {
            HasGeometry = false;
        }
        #endregion
       
        #region Methods
        public Boolean ProjectPoint(ref double x, ref double y, string fromSRC, string toSRC)
        {
            JObject result = null;
            JToken geom = null;
            String state = string.Empty;
            string msg;

            try
            {
                //project?inSR=4326&outSR=26915&geometries={geometries:[{x:-93.9508,y:42.0191}],geometryType:esriGeometryPoint}f=pjson
                //project?inSR=4326&outSR=26915&geometries={geometries:[{x:-93.9508,y:42.0191}],geometryType:esriGeometryPoint}&transformation=&transformForward=false&f=pjson

                string urlString = String.Format(getURI(serviceType.e_projection),fromSRC, toSRC, x,y);

                 result = Execute(new RestSharp.RestRequest(urlString)) as JObject;
                
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
                    uri = ConfigurationManager.AppSettings["pointProjection"];
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
    }//end sssServiceAgent

    public abstract class ServiceAgentBase
    {
        #region "Events"

        #endregion

        #region Properties & Fields

        readonly string _accountSid;
        readonly string _secretKey;

        private RestClient client = new RestClient();
        #endregion

        #region Constructors
        public ServiceAgentBase(string BaseUrl) 
        {
            client.BaseUrl = BaseUrl; 
        }

        public ServiceAgentBase(string accountSid, string secretKey, string baseUrl):this(baseUrl)
         {            
            _accountSid = accountSid;
            _secretKey = secretKey;
        }
        #endregion

        #region Methods
        public void ExecuteAsync<T>(RestRequest request, Action<T> CallBackOnSuccess, Action<string> CallBackOnFail) where T : new()
        {
            // request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request
        
            client.ExecuteAsync<T>(request, (response) =>
                {
                    if (response.ResponseStatus == ResponseStatus.Error)
                    {
                        CallBackOnFail(response.ErrorMessage);
                    }
                    else
                    {
                       CallBackOnSuccess(response.Data);
                    }
                });

                 
        }//end ExecuteAsync<T>

        public IRestResponse<T> Execute<T>(IRestRequest request) where T : new()
        {
            IRestResponse<T> result = null;
            if (request == null) throw new ArgumentNullException("request");
            
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            Exception exception = null;
            
            client.ExecuteAsync<T>(request, (response) =>
                {
                    if (response.ResponseStatus == ResponseStatus.Error)
                    {
                        exception = new Exception(response.ResponseStatus.ToString());
                        //release the Event
                        waitHandle.Set();
                    }
                    else
                    {
                        result = response;
                        //release the Event
                        waitHandle.Set();
                    }
                });
  
           //wait until the thread returns
            waitHandle.WaitOne();

            return result;
        }//end Execute<T>

        public Object Execute(IRestRequest request)
        {
            IRestResponse response = null;
            if (request == null) throw new ArgumentNullException("request");

            response = client.Execute(request) as IRestResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject(response.Content);
            }//else
            else
            {
                throw new Exception(response.ErrorMessage);
            }
        }//endExecute

        protected RestRequest getPOSTRestRequest(string URI, object Body)
        {
            RestRequest request = new RestRequest(URI);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded",Body, ParameterType.RequestBody);
            request.Method = Method.POST;
            request.Timeout = 600000;
      
            return request;
        }//end BuildRestRequest

        #endregion

    }//end class ServiceAgentBase

    
}//end namespace
