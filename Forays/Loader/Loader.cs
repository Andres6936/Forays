using System.Collections.Generic;
using Forays.Enums;

namespace Forays.Loader
{
    public abstract class Loader
    {
        protected int CurrentLevelId { get; set; }

        protected int FinalLevelClock { get; set; }

        protected int FinalLevelDemonCount { get; set; }

        protected int[] FinalLevelCultistCount { get; set; }

        protected bool WizLite { get; set; }

        protected bool WizDark { get; set; }

        protected char[,] LastCharacterSeen { get; set; }

        protected string NamePlayer { get; set; }

        protected List<FeatType> FeatTypes { get; set; }

        protected List<SpellType> SpellTypes { get; set; }

        protected List<LevelType> LevelTypes { get; set; }

        protected Color[,] LastColorCharacterSeen { get; set; }

        protected Color[,] LastBackgroundColorCharacterSeen { get; set; }
    }
}