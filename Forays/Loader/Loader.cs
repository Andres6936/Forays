using System.Collections.Generic;
using Forays.Enums;

namespace Forays.Loader
{
    public abstract class Loader
    {
        // Members

        protected int CurrentLevelId { get; }

        protected int FinalLevelClock { get; }

        protected int FinalLevelDemonCount { get; }

        protected int[] FinalLevelCultistCount { get; }

        protected bool WizLite { get; }

        protected bool WizDark { get; }

        protected char[,] LastCharacterSeen { get; }

        protected string NamePlayer { get; }

        protected List<FeatType> FeatTypes { get; }

        protected List<SpellType> SpellTypes { get; }

        protected List<LevelType> LevelTypes { get; }

        protected Color[,] LastColorCharacterSeen { get; }

        protected Color[,] LastBackgroundColorCharacterSeen { get; }
    }
}