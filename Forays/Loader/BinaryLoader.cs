using System.IO;
using Forays.Enums;

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

                if (LevelTypes[CurrentLevelId] == LevelType.Final)
                {
                    for (var index = 0; index < 5; index += 1)
                    {
                        FinalLevelCultistCount[index] = reader.ReadInt32();
                    }

                    FinalLevelDemonCount = reader.ReadInt32();
                    FinalLevelClock = reader.ReadInt32();
                }

                var numberFeatList = reader.ReadInt32();
                for (var index = 0; index < numberFeatList; index += 1)
                {
                    FeatTypes.Add((FeatType) reader.ReadInt32());
                }

                // 13. Number Spell List (Int32)
                var numberSpellList = reader.ReadInt32();
                for (var index = 0; index < numberSpellList; index += 1)
                {
                    // 13.1 Spell Type (Int32) -> Depend of 13
                    SpellTypes.Add((SpellType) reader.ReadInt32());
                }

                // 14. Num Actor TieBreakers (Int32)
                var numberTiebreakers = reader.ReadInt32();
                for (var index = 0; index < numberTiebreakers; index += 1)
                {
                }
            }
        }
    }
}