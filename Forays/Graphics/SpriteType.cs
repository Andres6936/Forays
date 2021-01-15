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

        public static SpriteType DefineSingleRowSprite(Surface surface, int sprite_width_px)
        {
            SpriteType s = new SpriteType();
            float texcoord_width =
                (float) sprite_width_px * 1.0f / (float) surface.texture.TextureWidthPx;
            s.X = idx => idx * texcoord_width;
            s.Y = idx => 0;
            s.SpriteWidth = texcoord_width;
            s.SpriteHeight = 1.0f;
            if (surface != null)
            {
                surface.texture.Sprite.Add(s);
            }

            return s;
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
            float px_width = 1.0f / (float) surface.texture.TextureWidthPx;
            float texcoord_width = (float) width * px_width;
            float texcoord_start = texcoord_width + (float) padding * px_width;
            s.X = idx => idx * texcoord_start;
            s.Y = idx => 0;
            s.SpriteWidth = texcoord_width;
            s.SpriteHeight = 1.0f;
            if (surface != null)
            {
                surface.texture.Sprite.Add(s);
            }
        }

        public static SpriteType DefineSpriteAcross(Surface surface, int sprite_width_px,
            int sprite_height_px,
            int num_columns)
        {
            SpriteType s = new SpriteType();
            float texcoord_width =
                (float) sprite_width_px * 1.0f / (float) surface.texture.TextureWidthPx;
            float texcoord_height = (float) sprite_height_px * 1.0f /
                                    (float) surface.texture.TextureHeightPx;
            s.X = idx => (idx % num_columns) * texcoord_width;
            s.Y = idx => (idx / num_columns) * texcoord_height;
            s.SpriteWidth = texcoord_width;
            s.SpriteHeight = texcoord_height;
            if (surface != null)
            {
                surface.texture.Sprite.Add(s);
            }

            return s;
        }

        public static SpriteType DefineSpriteAcross(Surface surface, int sprite_width_px,
            int sprite_height_px,
            int num_columns, int h_offset_px, int v_offset_px)
        {
            SpriteType s = new SpriteType();
            float texcoord_width =
                (float) sprite_width_px * 1.0f / (float) surface.texture.TextureWidthPx;
            float texcoord_height = (float) sprite_height_px * 1.0f /
                                    (float) surface.texture.TextureHeightPx;
            s.X = idx => ((idx % num_columns) * sprite_width_px + h_offset_px) * 1.0f /
                         (float) surface.texture.TextureWidthPx;
            s.Y = idx => ((idx / num_columns) * sprite_height_px + v_offset_px) * 1.0f /
                         (float) surface.texture.TextureHeightPx;
            s.SpriteWidth = texcoord_width;
            s.SpriteHeight = texcoord_height;
            if (surface != null)
            {
                surface.texture.Sprite.Add(s);
            }

            return s;
        }

        public static SpriteType DefineSpriteDown(Surface surface, int sprite_width_px,
            int sprite_height_px,
            int num_rows)
        {
            SpriteType s = new SpriteType();
            float texcoord_width =
                (float) sprite_width_px * 1.0f / (float) surface.texture.TextureWidthPx;
            float texcoord_height = (float) sprite_height_px * 1.0f /
                                    (float) surface.texture.TextureHeightPx;
            s.X = idx => (idx / num_rows) * texcoord_width;
            s.Y = idx => (idx % num_rows) * texcoord_height;
            s.SpriteWidth = texcoord_width;
            s.SpriteHeight = texcoord_height;
            if (surface != null)
            {
                surface.texture.Sprite.Add(s);
            }

            return s;
        }

        public static SpriteType DefineSpriteDown(Surface surface, int sprite_width_px,
            int sprite_height_px,
            int num_rows, int h_offset_px, int v_offset_px)
        {
            SpriteType s = new SpriteType();
            float texcoord_width =
                (float) sprite_width_px * 1.0f / (float) surface.texture.TextureWidthPx;
            float texcoord_height = (float) sprite_height_px * 1.0f /
                                    (float) surface.texture.TextureHeightPx;
            s.X = idx => ((idx / num_rows) * sprite_width_px + h_offset_px) * 1.0f /
                         (float) surface.texture.TextureWidthPx;
            s.Y = idx => ((idx % num_rows) * sprite_height_px + v_offset_px) * 1.0f /
                         (float) surface.texture.TextureHeightPx;
            s.SpriteWidth = texcoord_width;
            s.SpriteHeight = texcoord_height;
            if (surface != null)
            {
                surface.texture.Sprite.Add(s);
            }

            return s;
        }
    }
}