using System.IO;

namespace Forays.Loader
{
    public class BinaryLoader : Loader
    {
        public BinaryLoader(string filename)
        {
            using (var reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
            }
        }
    }
}