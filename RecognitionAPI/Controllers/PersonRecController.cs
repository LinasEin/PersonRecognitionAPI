using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using WhosThat.BusinessLogic;
using WhosThat.Model;

namespace RecognitionAPI.Controllers
{
    /// <summary>
    /// Face detection and recognition 
    /// </summary>
    [RoutePrefix("api/rec")]
    public class PersonRecController : ApiController
    {
        /// <summary>
        /// Posts frame to memory and starts recognition process. 
        /// </summary>
        /// <returns>HttpResponseMessage - Status code</returns>
        [Route("Post")]
        [AllowAnonymous]
        public HttpResponseMessage PostImage()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            try
            {
                var httpRequest = HttpContext.Current.Request;
                foreach (string file in httpRequest.Files)
                {

                    //TODO: not sure about this
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var postedFile = httpRequest.Files[file];
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        PersonRecognition.Instance.FindFacesInPhoto(postedFile);               
                    }
                    var image = PersonRecognitionModel.Instance.GetPicture();
                    if (image == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                    }

                    using (MemoryStream convertionStream = new MemoryStream())
                    {
                        image.Save(convertionStream, ImageFormat.Bmp);
                        MemoryStream stream = new MemoryStream(convertionStream.ToArray());
                        response.Content = new StreamContent(stream);
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/bmp");
                    }
                    return response;
                }
                var res = string.Format("Please Upload an image.");
                dict.Add("error", res);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
            catch (HttpException)
            {
                var errMessage = string.Format("Request failed.");
                dict.Add("error", errMessage);
                return Request.CreateResponse(HttpStatusCode.NotFound, dict);
            }
        }
    
        /*/// <summary>
        /// Gets processed image.
        /// </summary>
        /// <returns>HttpResponseMessage - Content</returns>
        [Route("Get")]
        [HttpGet]
        public HttpResponseMessage GetImage()
        {
            var image = PersonRecognitionModel.Instance.GetPicture();
            if (image == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            using (MemoryStream convertionStream = new MemoryStream())
            {
                image.Save(convertionStream, ImageFormat.Bmp);
                MemoryStream stream = new MemoryStream(convertionStream.ToArray());
                HttpResponseMessage response = new HttpResponseMessage();
                response.Content = new StreamContent(stream);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/bmp");
                return response;
            }
        }
        */
    }
}