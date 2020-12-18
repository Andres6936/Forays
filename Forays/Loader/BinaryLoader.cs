using System.IO;

namespace Forays.Loader
{
    public class BinaryLoader : Loader
    {
        public BinaryLoader(string filename)
        {
            var binaryReader = new BinaryReader(new FileStream(filename, FileMode.Open));
        }
    }
}