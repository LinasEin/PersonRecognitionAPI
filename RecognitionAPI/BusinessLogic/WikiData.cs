using System;
using System.Net;
using System.Configuration;
using System.Xml;
using System.Text.RegularExpressions;

namespace WhosThat.BusinessLogic
{
    /// <summary>
    /// Description about given person collector using wiki API
    /// </summary>
    class WikiData: IDescription
    {
        readonly WebClient client;
        public WikiData()
        {
            client = new WebClient
            {
                Encoding = System.Text.Encoding.UTF8
            };
        }
        public bool CheckConnection()
        {
            try
            {
                using (client.OpenRead(address: ConfigurationManager.AppSettings["connectionCheck"]))
                {
                    return true;
                }
            }
            catch(InvalidOperationException)
            {                
                return false;
            }
        }
        /// <summary>
        /// Gets intro description from Wikipedia.
        /// </summary>
        /// <param name="name">The name to be searched</param>
        /// <param name="countryCode">Determines the query language</param>
        /// <returns>
        /// String.Empty, if exception occurred;
        /// NoConnection message, if there is no internet connection;
        /// NoData message, if no data found;
        /// Description of the corresponding person, otherwise
        /// </returns>
        public string DownloadString(string name)
        {           
             string result;
             if(!CheckConnection())
             {
                 return GlobalStrings.NoConnection;
             }

             string allText = client.DownloadString(address: ConfigurationManager.AppSettings["defaultProtocol"] +
                                                    GlobalStrings.CountryCode +ConfigurationManager.AppSettings["Query"]+name);
             XmlDocument file = new XmlDocument();
             file.LoadXml(allText);
             XmlNodeList xmlList = file.GetElementsByTagName("extract");
             XmlNode xmlNode;

             if (xmlList.Count == 0)
             {
                return GlobalStrings.NoData;
             }
             else xmlNode = xmlList[0];

             try
             {
                result = xmlNode.InnerText;
                //excluded " ( listen);" from query result
                result = Regex.Replace(result, @"\s+\(\s+\b(listen)\b\)\;", "");

                //added one more new line symbol (for text readability) 
                result = Regex.Replace(result, @"\.\n", "$&\n");
                result = result + Environment.NewLine + Environment.NewLine +
                                  GlobalStrings.AdditionalDesclabel +
                                  Environment.NewLine +
                                  ConfigurationManager.AppSettings["defaultProtocol"] +
                                  GlobalStrings.CountryCode +
                                  ConfigurationManager.AppSettings["MoreInfoURL"] + name;
             }
             catch (ArgumentNullException)
             {
                 return String.Empty;
             }
             
            return result;    
        }

    }
}
