using System.Collections.Generic;
using Forays.Enums;

namespace Forays.Loader
{
    public abstract class Loader
    {
        // Members

        protected int CurrentLevelId;

        protected int FinalLevelClock;

        protected int FinalLevelDemonCount;

        protected int[] FinalLevelCultistCount;

        protected bool WizLite;

        protected bool WizDark;

        protected char[,] LastCharacterSeen;

        protected string NamePlayer;

        protected List<LevelType> LevelTypes;

        protected List<FeatType> FeatTypes;

        protected List<SpellType> SpellTypes;

        protected Color[,] LastColorCharacterSeen;

        protected Color[,] LastBackgroundColorCharacterSeen;

        // Getters Abstract

        public abstract string GetNamePlayer();
    }
}