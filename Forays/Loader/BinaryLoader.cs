using System.IO;

namespace Forays.Loader
{
    public sealed class BinaryLoader : Loader
    {
        public BinaryLoader(string filename)
        {
            using (var reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                NamePlayer = reader.ReadString();
                CurrentLevelId = reader.ReadInt32();

                var numberLevelTypes = reader.ReadInt32();
                for (var index = 0; index < numberLevelTypes; index += 1)
                {
                    LevelTypes.Add((LevelType) reader.ReadInt32());
                }

                WizLite = reader.ReadBoolean();
                WizDark = reader.ReadBoolean();
            }
        }
    }
}