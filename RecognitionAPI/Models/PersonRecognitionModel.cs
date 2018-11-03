using System;
using System.Drawing;
using System.Linq;
using WhosThat.Helper;


namespace WhosThat.Model
{
    public sealed class PersonRecognitionModel : IPersonRecognitionModel
    {
        private ItemCatalog<Bitmap> RecognizedPersons;

        private static readonly Lazy<PersonRecognitionModel> Model =
           new Lazy<PersonRecognitionModel>(() => new PersonRecognitionModel());

        public static PersonRecognitionModel Instance { get { return Model.Value; } }

        private PersonRecognitionModel()
        {
            RecognizedPersons = new ItemCatalog<Bitmap>();
        }
        public void AddPicture(Bitmap image)
        {
            RecognizedPersons.Add(image);
        }
        public Bitmap GetPicture()
        {
            if (RecognizedPersons.Count() != 0)
            {
                return RecognizedPersons.ToList().First();
            }
            return null;
        }
        public void ClearSelectedImage(string key)
        {
            //RecognizedPersons.ToList().Remove();
        }


    }
}
