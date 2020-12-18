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

                for (var row = 0; row < Global.ROWS; row += 1)
                {
                    for (var col = 0; col < Global.COLS; col += 1)
                    {
                        LastCharacterSeen[row, col] = reader.ReadChar();
                        LastColorCharacterSeen[row, col] = (Color) reader.ReadInt32();
                        LastBackgroundColorCharacterSeen[row, col] = (Color) reader.ReadInt32();
                    }
                }

                for (var index = 0; index < 5; index += 1)
                {
                    FinalLevelCultistCount[index] = reader.ReadInt32();
                }
            }
        }
    }
}