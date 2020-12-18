using Nym;
using Forays.Enums;

namespace Forays.Entity
{
    /// <summary>
    /// A player of a game is a participant therein. The term 'player' is used
    /// with this same meaning both in game theory and in ordinary recreational
    /// games. 
    ///
    /// "To become a player, one must voluntarily accept the rules and
    /// constraints of a game."
    /// Reference: Fullerton (2008). Game Design Workshop: A Play-centric
    /// Approach To Creating Innovative Games.
    ///
    /// A player character (also known as PC and playable character) is a
    /// fictional character in a video game or tabletop role-playing game whose
    /// actions are directly controlled by a player of the game rather than the
    /// rules of the game.
    ///
    /// The player character functions as a fictional, alternate body for the
    /// player controlling the character.
    /// </summary>
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