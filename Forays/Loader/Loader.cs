using System.Collections.Generic;

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

        protected List<LevelType> LevelType;

        protected Color[,] LastColorCharacterSeen;

        protected Color[,] LastBackgroundColorCharacterSeen;

        // Getters Abstract

        public abstract string GetNamePlayer();
    }
}