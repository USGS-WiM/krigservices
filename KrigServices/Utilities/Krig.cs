//------------------------------------------------------------------------------
//----- Krig -------------------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   
//           
//discussion:   Archfield S.A. and Vogel R.M 2010 Map correlation method: Selection of a reference streamgage
//		                to estimate daily streamflow at ungaged catchments, Water Resources Research Vol. 46
//              Isaaks, E. H., and R. M. Srivastava (1989), An Introduction to Applied
//		                Geostatistics, 1st ed., Oxford Univ. Press, New York. pg287-296
//

#region "Comments"
//12.20.2013 jkn - Created
#endregion
#region Imports
using System;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Configuration;
using System.Collections.Generic;

using KrigServices.Resources;
#endregion

namespace KrigServices.Utilities
{
    public class Krig
    {
        #region Properties
        public string State { get; private set; }
        public string SR { get; private set; }
        public Int32 ReturnCount { get; private set; }
        private const Double c_meter2kilometers = 0.001;
        #endregion
     
        #region Collections & Dictionaries
        #region CorralatedGageList
        private List<GageCorrelationStruc> CorralatedGageList { get; set; }
        #endregion
        #region IndexGages
        public List<KrigIndexSite> IndexGages { get; private set; }
        #endregion
        #region DistanceMatrix
        private Double[,] DistanceMatrix { get; set; }
        #endregion
        #region TopCorrelatedGages
        public Dictionary<double, KrigIndexSite> TopCorrelatedGages { get; private set; }
        private void AddCorrelationGage(GageCorrelationStruc IndexGage)
        {
            if (TopCorrelatedGages == null) TopCorrelatedGages = new Dictionary<double, KrigIndexSite>();
             
            //check if exists
            if(!CorrelatedGagesExists(IndexGage.gage.ID))
            {
                TopCorrelatedGages.Add(IndexGage.Correlation,IndexGage.gage);
            }//end if

            TopCorrelatedGages = TopCorrelatedGages.OrderByDescending(g => g.Key).Take(ReturnCount).ToDictionary(g=>g.Key,g=>g.Value);

        }//end AddCorrelationGage
        private Boolean CorrelatedGagesExists(string siteID)
        {
            KrigIndexSite gage = TopCorrelatedGages.FirstOrDefault(g => g.Value.ID == siteID).Value;

            if (gage == null) return false;

                return true;          
        }//end CorrelatedGagesExists
        #endregion

        #endregion
        
        #region Constructor
        public Krig(string state, Int32 returnCount)
        {
            State = state;
            ReturnCount = returnCount;
            //get required sr
            SR = GetcsvEnumberable(GetFileName(fileTypeEnum.e_stateRequiredSRC)).First()[0];

            //Load data
            if (!DataIsLoaded()) throw new Exception();

        }//end Krig
        #endregion
     
        #region Methods
        public Boolean StartKrigging(Double X, Double Y)
        {
            if (!SetDistanceToUngagedPoint(X, Y)) throw new Exception("Error setting distances");

            foreach (KrigIndexSite gage in this.IndexGages)
			{
				KrigGage(gage);
			}//next gage
            return false;
        }
        #endregion
      
        #region Helper Methods
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
        private Boolean SetDistanceToUngagedPoint(double X, double Y)
        {
            try
            {
                foreach (KrigIndexSite site in IndexGages)
                {
                    double varx =Math.Pow((site.LocationX - X),2);
					double vary = Math.Pow((site.LocationY - Y),2);

                    site.DistanceToUngagedPoint = Math.Sqrt(varx + vary) * getUnitConversion();
                }//next gage
                return true;
            }
            catch (Exception)
            {

                return false;
            }//end try
        }//end SetGageDistances
        private Double getUnitConversion()
        {
            switch (State)
            {
                case "PA":
                    return c_meter2kilometers;
                default:
                    return 1.0;
            }
        }
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
                                                GetCorrelatedGages(item[0],correlationFile));

                    IndexGages.Add(gage);
                }//next line

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }//end try
        }//end LoadIndexGages
        private Boolean LoadDistanceMatrix(IEnumerable<List<String>> file)
        {
            try
            {
                DistanceMatrix = new Double[IndexGages.Count,IndexGages.Count];
                var matrix = file.Skip(1).ToList();
                
                for (int r = 0; r < matrix.Count(); r++)
			    {
                    var columns = matrix[r].Skip(1).ToList();
                    for (int c = 0; c < columns.Count(); c++)
			        {
			            DistanceMatrix[r,c] = Convert.ToDouble(columns[c]);
			        }//next c		        
			    }//next r

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }//end try
        }//end LoadIndexGageDistanceMatrix
        private IDictionary<String,Double> GetCorrelatedGages(string gageId, IEnumerable<List<String>> File)
        {
            List<List<String>> correlationFile;
            List<String> siteList;
            IDictionary<Int32,string> sites;
            Int32 index;
            List<Int32> sitesIndexNotIncluded;
            List<Double> siteCorrelations;
            IDictionary<String, Double> correlationList;
            try
            {
                correlationFile = File.ToList();

                siteList = correlationFile.Skip(1).First().Where(i => !String.IsNullOrEmpty(i)).ToList();

                sites = correlationFile.First().AsEnumerable().Zip(siteList, (k,v)=> new{k,v}).ToDictionary(x=>Int32.Parse(x.k),x=>x.v);

                index = siteList.FindIndex(id => id.Trim().Equals(gageId, StringComparison.Ordinal));
 
                sitesIndexNotIncluded = correlationFile.Skip(2).TakeWhile(item => !item.Contains("CORRELATIONS"))
                                            .Where(s=>!String.IsNullOrEmpty(s[index])).Select(s=> Int32.Parse(s[index])).ToList();
 
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
        private IEnumerable<List<String>> GetcsvEnumberable(String ObjectName)
        {
            String AssetDataLocation = Path.Combine(new String[]{AppDomain.CurrentDomain.BaseDirectory,"Assets","Data",State});
            String objfile = Path.Combine(AssetDataLocation, ObjectName);
            Stream file;
            StreamReader sReader;
               
                file = File.OpenRead(objfile);
                using (sReader = new StreamReader(file))
                {
                    //This will correctly split on either type of line break, and preserve empty lines and spacing in the text:
                    var rows = sReader.ReadToEnd().Split(new String[]{"\r\n","\n"},StringSplitOptions.None);
                    foreach (String row in rows)
                    {
                      yield return row.Split(',').ToList();  
     
                    }//next row
                }//end using
    
        }//end GetObject
        private String GetFileName(fileTypeEnum filetype)
        {
            String result = string.Empty;
            switch (filetype)
            {
                case fileTypeEnum.e_distanceMatrix:
                    result = ConfigurationManager.AppSettings["distanceMatrixfile"];
                    break;
                case fileTypeEnum.e_gageProperties:
                    result = ConfigurationManager.AppSettings["propertiesfile"];
                    break;
                case fileTypeEnum.e_gageCorrelations:
                    result = ConfigurationManager.AppSettings["correlationfile"];
                    break;
                case fileTypeEnum.e_stateRequiredSRC:
                    result = ConfigurationManager.AppSettings["srcfile"];
                    break;
            }//end switch
            return result;
        }
        private void KrigGage(KrigIndexSite gage)
        {
            Double[,] invertedCovarianceMatrix = null;
            Double[,] ungagedCovarianceMatrix = null;
            //kriging multiplies the transposed covariance matrix by the matrix of covariances between the ungaged catchment and the gages
            //to obtain a set of weights. The weights are multiplied by each of the respective r values to obtain the unbiased minimum variance 
            //estimate of the correlation between the given study streamgage and the ungaged catchament. 
            //Archfield S.A. and Vogel R.M 2010 Map correlation method: Selection of a reference streamgage
            //		to estimate daily streamflow at ungaged catchments, Water Resources Research Vol. 46
            //Isaaks, E. H., and R. M. Srivastava (1989), An Introduction to Applied
            //		Geostatistics, 1st ed., Oxford Univ. Press, New York. pg287-296
            try
            {
                //remove SitesNotIncluded

                invertedCovarianceMatrix = InvertMatrixGJ(BuildCovarianceMatrix(ResizeDistanceMatrix(gage.Correlations.Keys), gage.rangeParameterA, gage.partialSillSigma));
                ungagedCovarianceMatrix = BuildCovarianceMatrix(ResizeSiteMatrix(gage.Correlations.Keys), gage.rangeParameterA, gage.partialSillSigma);
           
                Double val = GetKrigedGageCorrelations(invertedCovarianceMatrix, ungagedCovarianceMatrix, gage.Correlations.Select(v=>v.Value).ToList());

                AddCorrelationGage(new GageCorrelationStruc(val, gage));

            }
            catch (Exception e)
            {
                throw;
            }
        }//end KrigGage
        private Double[,] InvertMatrixGJ(Double[,] m)
        {
            // Inverts matrix using Gauss Jordan Elimination method
            //code adapted from: http://www.planet-source-code.com/vb/scripts/ShowCode.asp?txtCodeId=13618&lngWId=3
            Double[,] MIdentity = null;
            try
            {
                MIdentity = GetMatrixIdentiy(m.GetLength(1));
                int l = m.GetLength(0);
                for (int i = 0; i < l; i++)
                {
                    Double temp = m[i, i];
                    if(temp<0) 
						temp=temp*(-1); 
					int p = i;

                    for (int j = i + 1; j < l; j++)
                    {
                       Double tem;
						if(m[j,i]<0)
							tem=m[j,i]*(-1); 
						else 
							tem=m[j,i]; 
						
						if(temp<0) 
							temp=temp*(-1); 
						
						if(tem>temp) 
						{           
							p=j; 
							temp=m[j,i]; 
						}//end if 
                    }//next j

                    //row exchange in both the matricies 
                    for (Int32 j = 0; j < l; j++) 
					{ 
						Double temp1;
						Double temp2;

						temp1=m[i,j]; 
						m[i,j]=m[p,j]; 
						m[p,j]=temp1; 
						temp2=MIdentity[i,j]; 
						MIdentity[i,j]=MIdentity[p,j]; 
						MIdentity[p,j]=temp2; 
					}// next j 

                    Double temp4 =m[i,i]; 
					for(Int32 j=0;j < l; j++) 
					{ 
						m[i,j] = m[i,j]/temp4;
						MIdentity[i,j]=  MIdentity[i,j]/temp4; 
					}//next j 

                    //making other elements 0 in order to make the matrix a[][] 
                    //an identity matrix and obtaining a inverse b[][] matrix 
                    for (Int32 q = 0; q < l; q++) 
					{ 
						if(q==i) continue; //next q
						Double temp5 = m[q,i];
                        for (Int32 j = 0; j < l; j++)
						{
							m[q,j]=m[q,j]-(temp5*m[i,j]);
							MIdentity[q,j]=MIdentity[q,j]-(temp5*MIdentity[i,j]); 
						}//next j 
					}//next q 

                }//next i

                return MIdentity;
            }
            catch (Exception e)
            {
                
                throw;
            }
        }//end InvertMatrixGS
        private Double[,] GetMatrixIdentiy(Int32 l)
        {
            double[,] identityMatrix = new double[l, l];
            try
            {
                for (int r = 0; r < l; r++)
                {
                    for (int c = 0; c < l; c++)
                    {
                        if (r == c) identityMatrix[r, c] = 1;
                        else identityMatrix[r, c] = 0;
                    }//next c
                }//next r

                return identityMatrix;
            }
            catch (Exception e)
            {
                throw;
            }//end getMatrixIdentity
        }//end InvertMatrixGJ
        private Double[,] BuildCovarianceMatrix(Double[,] matrix, Double rangeParameterA, Double partialSillSigma)
        {
            double[,] covarianceMatrix = new double[matrix.GetLength(0),matrix.GetLength(1)];
            try
            {
                for (int r = 0; r < matrix.GetLength(0); r++)
                {
                    for (int c = 0; c < matrix.GetLength(1); c++)
                    {
                        covarianceMatrix[r, c] = ApplyCovarianceFunction(matrix[r, c], rangeParameterA, partialSillSigma);
                    }//nex c
                }//next r

                SetLagrangeParameter(ref covarianceMatrix);

                return covarianceMatrix;
            }
            catch (Exception e)
            {
                throw;
            }
        }//end BuildCovarianceMatrix
        private Double[,] ResizeDistanceMatrix(IEnumerable<string> sites)
        {
            List<string> siteList = IndexGages.Select(g => g.ID).ToList();
            List<Int32> NotIncludedSiteList = new List<Int32>();
            siteList.Except(sites).ToList().ForEach(i => NotIncludedSiteList.Add(siteList.FindIndex(id => id.Trim().Equals(i, StringComparison.Ordinal))));

            double[,] updatedDistMatrix = new double[this.IndexGages.Count - NotIncludedSiteList.Count, this.IndexGages.Count - NotIncludedSiteList.Count];
            Int32 row = 0;
            Int32 column = 0;
            try
            {
                for (int r = 0; r < DistanceMatrix.GetLength(0); r++)
                {
                    if (NotIncludedSiteList.Contains(r)) continue;
                    for (int c = 0; c < DistanceMatrix.GetLength(1); c++)
                    {
                        if (NotIncludedSiteList.Contains(c)) continue;
                        updatedDistMatrix[row, column] = DistanceMatrix[r, c];
                        column++;
                    }//next c
                    column = 0;
                    row++;
                }//next r  
            }
            catch (Exception e)
            {

                throw new Exception ("Error resizing distance matrix");
            }
            


            return updatedDistMatrix;
        }//end MakeGageMatrix
        private Double[,] ResizeSiteMatrix(IEnumerable<string> sites)
        {
            KrigIndexSite gage;
            try
            {
                IEnumerable<string> excludedSiteList = IndexGages.Select(g => g.ID).Except(sites);
                List<string> siteList = sites.Except(excludedSiteList).ToList();

                double[,] siteMatrix = new double[siteList.Count, 1];

                for (int i = 0; i < siteList.Count(); i++)
                {
                    gage = IndexGages.FirstOrDefault(g => siteList[i].Equals(g.ID, StringComparison.Ordinal));
                    siteMatrix[i, 0] = gage.DistanceToUngagedPoint;
                }//next gage

                return siteMatrix;
            }
            catch (Exception)
            {
                throw new Exception("Error resizing site Matrix");
            }
        }//end MakeGageMatrix
        private void SetLagrangeParameter(ref Double[,] m)
        {
            //The technique of Lagrange parameters is a procedure for converting a constrained minimization problem into an unconstrained one. 
			//Isaaks, E. H., and R. M. Srivastava (1989), An Introduction to Applied
			//		Geostatistics, 1st ed., Oxford Univ. Press, New York. pg284-285

            try
            {
                int rowLen = m.GetLength(0);
                int colLen = m.GetLength(1);
                double[,] newArray = null;

                if (colLen > 1)
                {
                    newArray = ResizeArray<Double>(m, rowLen + 1, rowLen + 1);

                    for (int i = 0; i < rowLen; i++)
                    {
                        newArray[rowLen, i] = 1;
                        newArray[i, colLen] = 1;
                    }//next r

                    newArray[rowLen, colLen] = 0;
                }
                else // 1 D array
                {
                    newArray = ResizeArray<Double>(m, rowLen + 1, colLen);
                    newArray[rowLen, 0] = 1;
                }//end if
                
                m = newArray;
            }
            catch (Exception e)
            {
                throw;
            }
        }//end setLagrangePramater
        private T[,] ResizeArray<T>(T[,] original, Int32 newNumberOfRows, Int32 newNumberOfColumns)
        {            
            var newArray = new T[newNumberOfRows, newNumberOfColumns];
            int columnCount = original.GetLength(1);
            int columnCount2 = newNumberOfColumns;
            int rows = original.GetUpperBound(0);

            for (int row = 0; row <= rows; row++)
                Array.Copy(original, row * columnCount, newArray, row * columnCount2, columnCount);

            return newArray;

        }//end ResizeArray
        private Double ApplyCovarianceFunction(Double separationDistanceH, Double rangeParameterA, Double partialSillSigma)
        {
            //The Covariance function estimates the covariance between 2 r 
            //values at any distance apart from one another.
            //C(h) = sigma^2*v(h)
            //where : 	sigma^2 = partial sill
            //			h = separation distance
            //					------------------------------------	
            //			v(h) = | 1-1.5(h/a)+0.5(h/a)^3 : if h<a    |
            //				   | 	0				   : otherwise |
            // 				   ------------------------------------
            //			a = range parameter
            // The Variogram model v(h) is a geostatistical method 
            // that specify that points in the variogram cloud (or rather points that have no relation between semivariance and separation distances)
            // can be binned within specified separation distaces of on another to obtain a sample variogram
            // Archfield S.A. and Vogel R.M 2010 Map correlation method: Selection of a reference streamgage
            //		to estimate daily streamflow at ungaged catchments, Water Resources Research Vol. 46

            double variogram = double.NaN;
            double distanceOverRange = double.NaN;
            try
            {
                //TODO: Determine everything is in correct units. 
                if (separationDistanceH >= rangeParameterA) return 0;

                distanceOverRange = separationDistanceH / rangeParameterA;

                variogram = 1 - 1.5 * distanceOverRange + 0.5 * Math.Pow(distanceOverRange, 3);

                return partialSillSigma * variogram;
            }
            catch (Exception e)
            {

                throw;
            }//end try
        }//end ApplyCovarianceFunction
        private Double GetKrigedGageCorrelations(Double[,] I, Double[,] D, List<double> correlations)
        {
            double[,] weights = null;
            double val = 0;
            try
            {
                weights = GetVarianceWeights(I, D);
                //weights have an n+1 due to the added LagrangeParameter
                if (weights.GetLength(0) - 1 != correlations.Count) throw new Exception("weights-1 != indexGages count");
                
                for (int i = 0; i < correlations.Count; i++)
                {
                    val = val + weights[i, 0] * correlations[i];
                }//next i
                return val;
            }
            catch (Exception)
            {
                return  double.NaN;
            }//end try
        }//end getKrigedGageValue
        private Double[,] GetVarianceWeights(Double[,] I, Double[,] D)
        {
            //the set of wieghts that provide unbiased estimates with a minimum estimation variance is calculated by 
            //multiplying the inverted covariance matrix(I or C^-1) by the ungaged covarinace matrix (D matrix)
            //Isaaks, E. H., and R. M. Srivastava (1989), An Introduction to Applied
            //		Geostatistics, 1st ed., Oxford Univ. Press, New York. pg295
            int matrixLength = 0;
            Double wSum = 0;
            Double[,] w = null;
            try
            {
                if (I.GetLength(0) != D.GetLength(0) ||
                    I.GetLength(1) != D.GetLength(0)) throw new Exception("rows != columns");

                matrixLength = I.GetLength(0);

                w = new Double[matrixLength, 1];
                for (int i = 0; i < matrixLength; i++)
                {
                    wSum = 0;
                    for (int j = 0; j < matrixLength; j++)
                    {
                        // matrix DOT product
                        wSum = wSum + I[i, j] * D[j, 0];
                    }//next r
                    w[i, 0] = wSum;
                }//next i

                return w;
            }
            catch (Exception e)
            {
                throw;
            }//end try
        }//end getVarianceWeights
       
        #endregion
       
        #region Enumerations
        private enum fileTypeEnum
        {
            e_distanceMatrix = 1,
            e_gageProperties = 2,
            e_gageCorrelations =3,
            e_stateRequiredSRC = 4
        }
        #endregion
      
        #region Structures
        private struct GageCorrelationStruc
        {
            public KrigIndexSite gage;
            public Double Correlation;
            #region Constructor
            public GageCorrelationStruc(double c, KrigIndexSite g)
            {
                gage = g;
                Correlation = c;
            }
            #endregion
        }//end indexGage
        #endregion
    }// end class 
}//end namespace