using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using WhosThat.BusinessLogic;

namespace RecognitionAPI.Controllers
{
    /// <summary>
    /// Face detection and recognition 
    /// </summary>
    [RoutePrefix("api/rec")]
    public class PersonRecController : ApiController
    {
        /// <summary>
        /// Posts frame and starts recognition process. 
        /// </summary>
        /// <returns>HttpResponseMessage - Status code or text/plain content</returns>
        [Route("post")]
        public HttpResponseMessage PostImage()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                foreach (string file in httpRequest.Files)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var postedFile = httpRequest.Files[file];
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        var result = PersonRecognition.Instance.FindFacesInPhoto(postedFile);
                        if (result == null)
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound);
                        }
                        byte[] bytes = System.Text.Encoding.ASCII.GetBytes(result);
                        MemoryStream stream = new MemoryStream(bytes);
                        response.Content = new StreamContent(stream);
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                        return response;
                    }
                }
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (HttpException)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
        }

    }
}