using Nym;
using Forays.Enums;

namespace Forays.Entity
{
    public class Player : Actor
    {
        /// <summary>
        /// Instance the Entity that represent to the user. The values for
        /// default for the Player are: Maximum Life: 100 points,
        /// Speed: 100 points, Symbol for default: '@', Color that
        /// represent the Symbol for default: White. 
        /// </summary>
        /// <param name="name">Dynamic name of Player.</param>
        /// <param name="attributeList">Attributes that the Player use.</param>
        public Player(Name name, params AttrType[] attributeList)
            : base(ActorType.PLAYER, name, '@', Color.White, 100, 100, 0, attributeList)
        {
        }
    }
}