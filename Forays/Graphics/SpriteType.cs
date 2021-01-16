using GLDrawing;

namespace Forays
{
    /// <summary>
    /// Each different arrangement of sprites on a sheet gets its own SpriteType. Many, like fonts, will use only a single SpriteType for the whole sheet.
    /// </summary>
    public class SpriteType
    {
        public PositionFromIndex
            X; //SpriteType is pretty similar to CellLayout. Any chance they could ever be combined?

        public PositionFromIndex Y;
        public float SpriteHeight; //0 to 1, not pixels
        public float SpriteWidth;

        // Constructs

        public SpriteType(int width, int textureWidth)
        {
            float texcoordWidth = width * 1.0f / textureWidth;
            X = idx => idx * texcoordWidth;
            Y = idx => 0;
            SpriteWidth = texcoordWidth;
            SpriteHeight = 1.0f;
        }

        /// <summary>
        /// Define the sprite sheet for a image with only a row of sprites.
        /// The width and padding of each sprite must be greater to zero.
        /// </summary>
        /// <param name="width">The width of each sprite (in pixels).</param>
        /// <param name="padding">The padding between each sprite (in pixels).</param>
        /// <param name="textureWidth">The width of texture in pixels.</param>
        public SpriteType(int width, int padding, int textureWidth)
        {
            float pxWidth = 1.0f / textureWidth;
            float texcoordWidth = width * pxWidth;
            float texcoordStart = texcoordWidth + padding * pxWidth;
            X = idx => idx * texcoordStart;
            Y = idx => 0;
            SpriteWidth = texcoordWidth;
            SpriteHeight = 1.0f;
        }
    }
}