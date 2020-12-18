using System.Collections.Generic;
using Forays.Enums;

namespace Forays.Loader
{
    public abstract class Loader
    {
        public int CurrentLevelId { get; set; }

        public int FinalLevelClock { get; set; }

        public int FinalLevelDemonCount { get; set; }

        public int[] FinalLevelCultistCount { get; set; }

        public bool WizLite { get; set; }

        public bool WizDark { get; set; }

        public string NamePlayer { get; set; }

        public List<Actor> Tiebreakers { get; set; }

        public List<FeatType> FeatTypes { get; set; }

        public List<SpellType> SpellTypes { get; set; }

        public List<LevelType> LevelTypes { get; set; }

        public colorchar[,] LastSeen { get; set; }
    }
}