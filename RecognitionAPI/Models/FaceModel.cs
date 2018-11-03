using Emgu.CV;
using Emgu.CV.Structure;

namespace WhosThat.Model
{
    public struct FaceModel
    {
        public string Label { get; set; }
        public string FileName { get; set; }
        public Image<Gray, byte> Image { get; set; }
    }
}
