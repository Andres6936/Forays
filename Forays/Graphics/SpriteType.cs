using GLDrawing;

namespace Forays
{
    public class SpriteType
    {
        //each different arrangement of sprites on a sheet gets its own SpriteType. Many, like fonts, will use only a single SpriteType for the whole sheet.
        public PositionFromIndex
            X; //SpriteType is pretty similar to CellLayout. Any chance they could ever be combined?

        public PositionFromIndex Y;
        public float SpriteHeight; //0 to 1, not pixels
        public float SpriteWidth;
        public int DefaultSpriteIndex;

        public static void DefineSingleRowSprite(Surface surface, int sprite_width_px)
        {
            SpriteType s = new SpriteType();
            float texcoord_width =
                (float) sprite_width_px * 1.0f / (float) surface.Texture.TextureWidthPx;
            s.X = idx => idx * texcoord_width;
            s.Y = idx => 0;
            s.SpriteWidth = texcoord_width;
            s.SpriteHeight = 1.0f;
            if (surface != null)
            {
                surface.Texture.Sprite.Add(s);
            }
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
            float px_width = 1.0f / surface.Texture.TextureWidthPx;
            float texcoord_width = width * px_width;
            float texcoord_start = texcoord_width + padding * px_width;
            s.X = idx => idx * texcoord_start;
            s.Y = idx => 0;
            s.SpriteWidth = texcoord_width;
            s.SpriteHeight = 1.0f;
            surface.Texture.Sprite.Add(s);
        }
    }
}