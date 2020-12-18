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

        protected List<FeatType> FeatTypes;

        protected List<SpellType> SpellTypes;

        protected List<LevelType> LevelTypes;

        protected Color[,] LastColorCharacterSeen;

        protected Color[,] LastBackgroundColorCharacterSeen;

        // Getters Abstract

        public abstract int GetCurrentLevelId();

        public abstract int GetFinalLevelClock();

        public abstract int GetFinalLevelDemonCount();

        public abstract int[] GetFinalLevelCultistCount();

        public abstract bool GetWizLite();

        public abstract bool GetWizDark();

        public abstract string GetNamePlayer();

        public abstract List<FeatType> GetFeatTypes();

        public abstract List<SpellType> GetSpellTypes();

        public abstract List<LevelType> GetLevelTypes();

        public abstract Color[,] GetLastColorCharacterSeen();

        public abstract Color[,] GetLastBackgroundColorCharacterSeen();
    }
}