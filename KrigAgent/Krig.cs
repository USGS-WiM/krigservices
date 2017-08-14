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
using System.Linq;
using System.IO;
using System.Collections.Generic;

using KrigAgent.Resources;


namespace KrigAgent
{
    public interface IKrigAgent
    {
        Boolean example {set; }
    }
    public class Krig : IKrigAgent
    {
        #region Properties
        public bool example { private get; set; }
        public string State { get; private set; }
        public string SR { get; private set; }
        public Int32 ReturnCount { get; private set; }
        private const Double c_meter2kilometers = 0.001;
        #endregion

        #region IndexGages
        public List<KrigIndexSite> IndexGages { get; private set; }
        #endregion
        #region DistanceMatrix
        private Double[,] DistanceMatrix { get; set; }
        #endregion

        public Krig(string state, Int32 returnCount)
        {
            //initiallizations happen here
            State = state;
            ReturnCount = returnCount;
            //get required sr
            SR = GetcsvEnumberable(GetFileName(fileTypeEnum.e_stateRequiredSRC)).First()[0];

            //Load data
            if (!DataIsLoaded()) throw new Exception();
        }

        #region HELPER METHODS
        private Boolean DataIsLoaded()
        {
            Boolean isOK = false;
            try
            {
                isOK = (LoadIndexProperties(GetcsvEnumberable(GetFileName(fileTypeEnum.e_gageProperties)), GetcsvEnumberable(GetFileName(fileTypeEnum.e_gageCorrelations))) &
                         LoadDistanceMatrix(GetcsvEnumberable(GetFileName(fileTypeEnum.e_distanceMatrix))));

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return isOK;

        }//end DataIsLoaded
        private Boolean LoadDistanceMatrix(IEnumerable<List<String>> file)
        {
            try
            {
                DistanceMatrix = new Double[IndexGages.Count, IndexGages.Count];
                var matrix = file.Skip(1).ToList();

                for (int r = 0; r < matrix.Count(); r++)
                {
                    var columns = matrix[r].Skip(1).ToList();
                    for (int c = 0; c < columns.Count(); c++)
                    {
                        DistanceMatrix[r, c] = Convert.ToDouble(columns[c]);
                    }//next c		        
                }//next r

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }//end try
        }//end LoadIndexGageDistanceMatrix
        private IDictionary<String, Double> GetCorrelatedGages(string gageId, IEnumerable<List<String>> File)
        {
            List<List<String>> correlationFile;
            List<String> siteList;
            IDictionary<Int32, string> sites;
            Int32 index;
            List<Int32> sitesIndexNotIncluded;
            List<Double> siteCorrelations;
            IDictionary<String, Double> correlationList;
            try
            {
                correlationFile = File.ToList();

                siteList = correlationFile.Skip(1).First().Where(i => !String.IsNullOrEmpty(i)).ToList();

                sites = correlationFile.First().AsEnumerable().Zip(siteList, (k, v) => new { k, v }).ToDictionary(x => Int32.Parse(x.k), x => x.v);

                index = siteList.FindIndex(id => id.Trim().Equals(gageId, StringComparison.Ordinal));

                sitesIndexNotIncluded = correlationFile.Skip(2).TakeWhile(item => !item.Contains("CORRELATIONS"))
                                            .Where(s => !String.IsNullOrEmpty(s[index])).Select(s => Int32.Parse(s[index])).ToList();

                siteCorrelations = correlationFile.Skip(2).SkipWhile(item => !item.Contains("CORRELATIONS")).Skip(1)
                                            .Where(s => !String.IsNullOrEmpty(s[index])).AsEnumerable().Select(s => Double.Parse(s[index])).ToList();

                //remove from list
                sitesIndexNotIncluded.ForEach(x => siteList.Remove(sites.FirstOrDefault(d => d.Key == x).Value));

                correlationList = siteList.Zip(siteCorrelations, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);

                return correlationList;
            }
            catch (Exception e)
            {

                throw new Exception("Error parsing correlation file");
            }
        }//end GetCorrelatedGages
        private Boolean LoadIndexProperties(IEnumerable<List<String>> file, IEnumerable<List<String>> correlationFile)
        {
            KrigIndexSite gage = null;
            try
            {
                IndexGages = new List<KrigIndexSite>();

                foreach (var item in file.Skip(1).ToList())
                {

                    gage = new KrigIndexSite(item[0], item[1], Convert.ToDouble(item[7]),
                                                Convert.ToDouble(item[6]), Convert.ToDouble(item[8]),
                                                Convert.ToDouble(item[2]), Convert.ToDouble(item[3]),
                                                GetCorrelatedGages(item[0], correlationFile));

                    IndexGages.Add(gage);
                }//next line

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }//end try
        }//end LoadIndexGages
        private String GetFileName(fileTypeEnum filetype)
        {
            String result = string.Empty;
            switch (filetype)
            {
                case fileTypeEnum.e_distanceMatrix:
                    result = "SiteDistanceMatrix.csv";
                    break;
                case fileTypeEnum.e_gageProperties:
                    result = "SiteProperties.csv";
                    break;
                case fileTypeEnum.e_gageCorrelations:
                    result = "SiteCorrelation.csv";
                    break;
                case fileTypeEnum.e_stateRequiredSRC:
                    result = "SRC.csv";
                    break;
            }//end switch
            return result;
        }
        private IEnumerable<List<String>> GetcsvEnumberable(String ObjectName)
        {
            String AssetDataLocation = Path.Combine(new String[] { AppContext.BaseDirectory, "Assets", "Data", State });
            String objfile = Path.Combine(AssetDataLocation, ObjectName);
            Stream file;
            StreamReader sReader;

            file = File.OpenRead(objfile);
            using (sReader = new StreamReader(file))
            {
                //This will correctly split on either type of line break, and preserve empty lines and spacing in the text:
                var rows = sReader.ReadToEnd().Split(new String[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (String row in rows)
                {
                    yield return row.Split(',').ToList();

                }//next row
            }//end using

        }//end GetObject
        #endregion
        #region Enumerations
        private enum fileTypeEnum
        {
            e_distanceMatrix = 1,
            e_gageProperties = 2,
            e_gageCorrelations = 3,
            e_stateRequiredSRC = 4
        }
        #endregion
    }
}
