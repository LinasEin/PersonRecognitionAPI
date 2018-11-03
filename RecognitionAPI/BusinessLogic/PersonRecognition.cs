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
        private MCvFont Font;
        private Image<Gray, byte> Result;
        private ItemCatalog<FaceModel> Faces = new ItemCatalog<FaceModel>();
        private IPersonRecognitionModel Model;

        private static readonly Lazy<PersonRecognition> recognizer =
            new Lazy<PersonRecognition>(() => new PersonRecognition());

        public static PersonRecognition Instance { get { return recognizer.Value; } }

        private PersonRecognition()
        {
            Model = PersonRecognitionModel.Instance;
            //LoadTrainedFaces(new ReaderFromFile());
            FaceHaarCascade = new HaarCascade(HttpContext.Current.Server.MapPath("~/bin/")+WebConfigurationManager.AppSettings["haarcascade"]);
            Font = new MCvFont(
                FONT.CV_FONT_HERSHEY_DUPLEX,
                double.Parse(WebConfigurationManager.AppSettings["fontHorScale"]),
                double.Parse(WebConfigurationManager.AppSettings["fontVerScale"]));
        }

        public bool LoadTrainedFaces(IReader Reader)
        {
            string[] fileLabels = Reader.Read();

            if (fileLabels.Length == 0)
                return false;
            //System.Diagnostics.Debug.WriteLine(fileLabels[0]);
            fileLabels.ToList()
                .ConvertAll<string>(label => label.Split('\\').Last())
                .ForEach(
            fullFileName =>
            {
                var pathToFile = $"{HttpContext.Current.Server.MapPath("~/Images/")}{fullFileName}";

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

        private Bitmap DetectPerson(Image<Gray, Byte> gray, Image<Bgr, Byte> CurrentFrame)
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

                
                CurrentFrame.Draw(face.rect, new Bgr(Color.Gray), frameThickness);

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

                   if (name != String.Empty)
                    {
                      /*if (Model.CheckIfRecognized(name))
                        {
                            Model.AddLabel(name);
                        }*/
                        CurrentFrame.Draw(face.rect, new Bgr(Color.Green), frameThickness);
                    }
                    else
                    {
                        CurrentFrame.Draw(face.rect, new Bgr(Color.Red), frameThickness);
                        name = WebConfigurationManager.AppSettings["defaultLabel"];
                    }

                    //Draw the label for each recognized face
                    
                    var textSize = Font.GetTextSize(name, 0);
                    var x = face.rect.Left + (face.rect.Width - textSize.Width) / 2;
                    var y = face.rect.Bottom + textSize.Height;
                    CurrentFrame.Draw(name, ref Font, new Point(x, y + 5), new Bgr(Color.White));
                }
                else
                {
                    CurrentFrame.Draw(face.rect, new Bgr(Color.Red), frameThickness);
                }
            }
             return CurrentFrame.ToBitmap();
        }  

        public Bitmap FindFacesInPhoto(HttpPostedFile request)
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
