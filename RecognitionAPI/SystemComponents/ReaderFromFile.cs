using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Configuration;

namespace WhosThat.SystemComponents
{
    class ReaderFromFile: IReader
    {
        public string[] Read()
        {
            return Directory.GetFiles(
                    path: HttpContext.Current.Server.MapPath($"~{WebConfigurationManager.AppSettings["faceLocPath"]}"),
                    searchPattern: $"*{WebConfigurationManager.AppSettings["defaultFileFormat"]}",
                    searchOption: SearchOption.TopDirectoryOnly
                );
        }
    }
}
