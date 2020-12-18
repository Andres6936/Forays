using System.Collections.Generic;
using Forays.Enums;

namespace Forays.Loader
{
    public abstract class Loader
    {
        protected abstract int CurrentLevelId { get; set; }

        protected abstract int FinalLevelClock { get; set; }

        protected abstract int FinalLevelDemonCount { get; set; }

        protected abstract int[] FinalLevelCultistCount { get; set; }

        protected abstract bool WizLite { get; set; }

        protected abstract bool WizDark { get; set; }

        protected abstract char[,] LastCharacterSeen { get; set; }

        protected abstract string NamePlayer { get; set; }

        protected abstract List<FeatType> FeatTypes { get; set; }

        protected abstract List<SpellType> SpellTypes { get; set; }

        protected abstract List<LevelType> LevelTypes { get; set; }

        protected abstract Color[,] LastColorCharacterSeen { get; set; }

        protected abstract Color[,] LastBackgroundColorCharacterSeen { get; set; }
    }
}