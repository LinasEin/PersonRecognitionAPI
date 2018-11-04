using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;
using WhosThat.Model;
using System.Linq;
using WhosThat.Helper;
using WhosThat.SystemComponents;
using System.Web;
using System.Web.Configuration;

namespace WhosThat.BusinessLogic
{
    public sealed class PersonRecognition
    {
        private HaarCascade FaceHaarCascade;
        private Image<Gray, byte> Result;
        private ItemCatalog<FaceModel> Faces = new ItemCatalog<FaceModel>();
        private IPersonRecognitionModel Model;

        private static readonly Lazy<PersonRecognition> recognizer =
            new Lazy<PersonRecognition>(() => new PersonRecognition());

        public static PersonRecognition Instance { get { return recognizer.Value; } }

        private PersonRecognition()
        {
            Model = PersonRecognitionModel.Instance;
            LoadTrainedFaces(new ReaderFromFile());
            FaceHaarCascade = new HaarCascade(HttpContext.Current.Server.MapPath("~/bin/")+WebConfigurationManager.AppSettings["haarcascade"]);
        }

        public bool LoadTrainedFaces(IReader Reader)
        {
            string[] fileLabels = Reader.Read();
            if (fileLabels.Length == 0)
                return false;
            fileLabels.ToList()
                .ConvertAll<string>(label => label.Split('\\').Last())
                .ForEach(
            fullFileName =>
            {
                var pathToFile = $"{HttpContext.Current.Server.MapPath($"~{WebConfigurationManager.AppSettings["faceLocPath"]}")}{fullFileName}";

                var fileName = System.Text.RegularExpressions.Regex.Replace(fullFileName, @"_[^_]+\.bmp", "");
                var label = fileName.Replace('_', ' ');
                Faces.Add(new FaceModel
                {
                    Label = label,
                    FileName = fileName,
                    Image = new Image<Gray, byte>(pathToFile)
                });
            });
            
            return true;
        }

        private string DetectPerson(Image<Gray, Byte> gray, Image<Bgr, Byte> CurrentFrame)
        {           
            var facesDetected = gray.DetectHaarCascade(
              haarObj: FaceHaarCascade,
              scaleFactor: double.Parse(WebConfigurationManager.AppSettings["haarcascadeScaleFactor"]),
              minNeighbors: int.Parse(WebConfigurationManager.AppSettings["minNeighbors"]),
              flag: HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
              minSize: new Size(int.Parse(WebConfigurationManager.AppSettings["FaceWindowSize"]),
                                int.Parse(WebConfigurationManager.AppSettings["FaceWindowSize"])))[0];

            int frameThickness = int.Parse(WebConfigurationManager.AppSettings["frameThickness"]);

            //Action for each element detected
            foreach (var face in facesDetected)
            {
                Result = CurrentFrame
                    .Copy(face.rect)
                    .Convert<Gray, byte>()
                    .Resize(int.Parse(WebConfigurationManager.AppSettings["resheight"]),
                            int.Parse(WebConfigurationManager.AppSettings["reswidth"]),
                            INTER.CV_INTER_CUBIC);

                if (Faces.Any())
                {
                    var termCriteria = new MCvTermCriteria(
                        Faces.Count(),
                        double.Parse(WebConfigurationManager.AppSettings["epsilon"]));

                    var facesList = Faces.ToList();

                    var recognizer = new EigenObjectRecognizer(
                       facesList.Select(f => f.Image).ToArray(),
                       facesList.Select(f => f.Label).ToArray(),
                       int.Parse(WebConfigurationManager.AppSettings["distanceThreshold"]),
                       ref termCriteria);

                    string name = recognizer.Recognize(Result);
                    return name;
                }
            }
            return string.Empty;
        }  

        public string FindFacesInPhoto(HttpPostedFile request)
        {
            var stream = request.InputStream;
            var image = Bitmap.FromStream(stream);
           
            var maxEdgeSize = int.Parse(WebConfigurationManager.AppSettings["imageWidth"]);
            var scaleFactor = Math.Min(
                (float)maxEdgeSize / image.Width,
                (float)maxEdgeSize / image.Height);

            var scaledSize = new Size(
                (int) Math.Floor(image.Width * scaleFactor),
                (int) Math.Floor(image.Height * scaleFactor));

            var rgbImage = new Image<Bgr, byte>(new Bitmap(image, scaledSize));
            var grayPhoto = rgbImage.Convert<Gray, byte>();

            return DetectPerson(grayPhoto, rgbImage);
        }
        
    }
}
