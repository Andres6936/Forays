using System.Collections.Generic;
using Forays.Enums;

namespace Forays.Loader
{
    public abstract class Loader
    {
        protected abstract int CurrentLevelId { get; }

        protected abstract int FinalLevelClock { get; }

        protected abstract int FinalLevelDemonCount { get; }

        protected abstract int[] FinalLevelCultistCount { get; }

        protected abstract bool WizLite { get; }

        protected abstract bool WizDark { get; }

        protected abstract char[,] LastCharacterSeen { get; }

        protected abstract string NamePlayer { get; }

        protected abstract List<FeatType> FeatTypes { get; }

        protected abstract List<SpellType> SpellTypes { get; }

        protected abstract List<LevelType> LevelTypes { get; }

        protected abstract Color[,] LastColorCharacterSeen { get; }

        protected abstract Color[,] LastBackgroundColorCharacterSeen { get; }
    }
}