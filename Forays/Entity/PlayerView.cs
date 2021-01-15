using Forays.Enums;

namespace Forays.Entity
{
    public class PlayerView
    {
        public static Actor Player = new Player(
            new Nym.Name("you", noArticles: true, secondPerson: true), 0,
            AttrType.HUMANOID_INTELLIGENCE);
    }
}