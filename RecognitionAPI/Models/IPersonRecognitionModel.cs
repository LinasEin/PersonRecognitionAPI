using System.Drawing;
using WhosThat.Helper;

namespace WhosThat.Model
{
    public interface IPersonRecognitionModel
    {
        void AddPicture(Bitmap image);
        Bitmap GetPicture();
        void ClearSelectedImage(string key);
    }
}
