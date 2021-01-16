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

        public static void DefineSingleRowSprite(Surface surface, int spriteWidthPx)
        {
            SpriteType s = new SpriteType();
            float texcoordWidth =
                (float) spriteWidthPx * 1.0f / (float) surface.Texture.TextureWidthPx;
            s.X = idx => idx * texcoordWidth;
            s.Y = idx => 0;
            s.SpriteWidth = texcoordWidth;
            s.SpriteHeight = 1.0f;
            surface.Texture.Sprite.Add(s);
        }

        /// <summary>
        /// Define the sprite sheet for a image with only a row of sprites.
        /// The width and padding of each sprite must be greater to zero.
        /// </summary>
        /// <param name="surface">The surface that content the image.</param>
        /// <param name="width">The width of each sprite (in pixels).</param>
        /// <param name="padding">The padding between each sprite (in pixels).</param>
        public static void DefineSingleRowSprite(Surface surface, int width, int padding)
        {
            SpriteType s = new SpriteType();
            float pxWidth = 1.0f / surface.Texture.TextureWidthPx;
            float texcoordWidth = width * pxWidth;
            float texcoordStart = texcoordWidth + padding * pxWidth;
            s.X = idx => idx * texcoordStart;
            s.Y = idx => 0;
            s.SpriteWidth = texcoordWidth;
            s.SpriteHeight = 1.0f;
            surface.Texture.Sprite.Add(s);
        }
    }
}