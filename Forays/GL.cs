/*Copyright (c) 2014-2015  Derrick Creamer
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Reflection;
using Forays;
using OpenTK.Graphics.OpenGL;

namespace GLDrawing
{
    public enum ResizeOption
    {
        StretchToFit,
        AddBorder,
        SnapWindow,
        NoResize
    };

    public delegate float PositionFromIndex(int idx);

    public delegate void SurfaceUpdateMethod(SurfaceDefaults defaults);

    public class SurfaceDefaults
    {
        public List<int> positions = null;
        public List<int> layouts = null;
        public List<int> sprites = null;
        public List<int> sprite_types = null;
        public List<float>[] other_data = null;

        public int single_position = -1;
        public int single_layout = -1;
        public int single_sprite = -1;
        public int single_sprite_type = -1;
        public List<float>[] single_other_data = null;

        public int fill_count = -1;

        public SurfaceDefaults()
        {
        }

        public SurfaceDefaults(SurfaceDefaults other)
        {
            if (other.positions != null)
            {
                positions = new List<int>(other.positions);
            }

            if (other.layouts != null)
            {
                layouts = new List<int>(other.layouts);
            }

            if (other.sprites != null)
            {
                sprites = new List<int>(other.sprites);
            }

            if (other.sprite_types != null)
            {
                sprite_types = new List<int>(other.sprite_types);
            }

            if (other.other_data != null)
            {
                other_data = new List<float>[other.other_data.GetLength(0)];
                int idx = 0;
                foreach (List<float> l in other.other_data)
                {
                    other_data[idx] = new List<float>(l);
                    ++idx;
                }
            }

            single_position = other.single_position;
            single_layout = other.single_layout;
            single_sprite = other.single_sprite;
            single_sprite_type = other.single_sprite_type;
            single_other_data = other.single_other_data;
            fill_count = other.fill_count;
        }

        public void FillValues(bool fill_positions, bool fill_other_data)
        {
            if (fill_count <= 0)
            {
                return;
            }

            if (fill_positions)
            {
                if (single_position != -1)
                {
                    //if this value is -1, nothing can be added anyway.
                    if (positions == null)
                    {
                        positions = new List<int>();
                    }

                    while (positions.Count < fill_count)
                    {
                        positions.Add(single_position);
                    }
                }

                if (single_layout != -1)
                {
                    if (layouts == null)
                    {
                        layouts = new List<int>();
                    }

                    while (layouts.Count < fill_count)
                    {
                        layouts.Add(single_layout);
                    }
                }
            }

            if (fill_other_data)
            {
                if (single_sprite != -1)
                {
                    if (sprites == null)
                    {
                        sprites = new List<int>();
                    }

                    while (sprites.Count < fill_count)
                    {
                        sprites.Add(single_sprite);
                    }
                }

                if (single_sprite_type != -1)
                {
                    if (sprite_types == null)
                    {
                        sprite_types = new List<int>();
                    }

                    while (sprite_types.Count < fill_count)
                    {
                        sprite_types.Add(single_sprite_type);
                    }
                }

                if (single_other_data != null)
                {
                    if (other_data == null)
                    {
                        other_data = new List<float>[single_other_data.GetLength(0)];
                        for (int i = 0; i < other_data.GetLength(0); ++i)
                        {
                            other_data[i] = new List<float>();
                        }
                    }

                    int idx = 0;
                    foreach (List<float> l in other_data)
                    {
                        while (l.Count < fill_count * single_other_data[idx].Count)
                        {
                            foreach (float f in single_other_data[idx])
                            {
                                l.Add(f);
                            }
                        }

                        ++idx;
                    }
                }
            }
        }
    }

    public class Surface
    {
        public OpenTkRender window;
        public VBO vbo;
        public Texture texture;
        public Shader shader;
        public List<CellLayout> layouts = new List<CellLayout>();
        protected SurfaceDefaults defaults = new SurfaceDefaults();
        public float raw_x_offset = 0.0f;
        public float raw_y_offset = 0.0f; //todo: should be properties
        private int x_offset_px;
        private int y_offset_px;
        public bool UseDepthBuffer = false;
        public bool Disabled = false;
        public SurfaceUpdateMethod UpdateMethod = null;
        public SurfaceUpdateMethod UpdatePositionsOnlyMethod = null;
        public SurfaceUpdateMethod UpdateOtherDataOnlyMethod = null;

        protected Surface()
        {
        }

        public static Surface Create(OpenTkRender window_, string texture_filename, params int[] vertex_attrib_counts)
        {
            return Create(window_, texture_filename, false, Shader.DefaultFS(), false, vertex_attrib_counts);
        }

        public static Surface Create(OpenTkRender window_, string texture_filename,
            bool loadTextureFromEmbeddedResource,
            string frag_shader, bool has_depth, params int[] vertex_attrib_counts)
        {
            Surface s = new Surface();
            s.window = window_;
            int dims = has_depth ? 3 : 2;
            s.UseDepthBuffer = has_depth;
            VertexAttributes attribs = VertexAttributes.Create(vertex_attrib_counts);
            s.vbo = VBO.Create(dims, attribs);
            s.texture = Texture.Create(texture_filename, null, loadTextureFromEmbeddedResource);
            s.shader = Shader.Create(frag_shader);
            if (window_ != null)
            {
                window_.Surfaces.Add(s);
            }

            return s;
        }

        public void RemoveFromWindow()
        {
            if (window != null)
            {
                window.Surfaces.Remove(this);
            }
        }

        public void SetOffsetInPixels(int x_offset_px, int y_offset_px)
        {
            this.x_offset_px = x_offset_px;
            this.y_offset_px = y_offset_px;
            raw_x_offset = (float) (x_offset_px * 2) / (float) window.Viewport.Width;
            raw_y_offset = (float) (y_offset_px * 2) / (float) window.Viewport.Height;
        }

        public void ChangeOffsetInPixels(int dx_offset_px, int dy_offset_px)
        {
            x_offset_px += dx_offset_px;
            y_offset_px += dy_offset_px;
            raw_x_offset += (float) (dx_offset_px * 2) / (float) window.Viewport.Width;
            raw_y_offset += (float) (dy_offset_px * 2) / (float) window.Viewport.Height;
        }

        //public void XOffsetPx(){ return x_offset_px; }
        //public void YOffsetPx(){ return y_offset_px; }
        public int TotalXOffsetPx(CellLayout layout)
        {
            return x_offset_px + layout.HorizontalOffsetPx;
        }

        public int TotalXOffsetPx()
        {
            return x_offset_px + layouts[0].HorizontalOffsetPx;
        }

        public int TotalYOffsetPx(CellLayout layout)
        {
            return y_offset_px + layout.VerticalOffsetPx;
        }

        public int TotalYOffsetPx()
        {
            return y_offset_px + layouts[0].VerticalOffsetPx;
        }

        public void SetEasyLayoutCounts(params int[] counts_per_layout)
        {
            if (counts_per_layout.GetLength(0) != layouts.Count)
            {
                throw new ArgumentException("SetEasyLayoutCounts: Number of arguments (" +
                                            counts_per_layout.GetLength(0) + ") must match number of layouts (" +
                                            layouts.Count + ").");
            }

            defaults.positions = new List<int>(); //this method creates the default lists used by Update()
            defaults.layouts = new List<int>();
            int idx = 0;
            foreach (int count in counts_per_layout)
            {
                for (int i = 0; i < count; ++i)
                {
                    defaults.positions.Add(i);
                    defaults.layouts.Add(idx);
                }

                ++idx;
            }

            defaults.fill_count = defaults.positions.Count;
        }

        public void SetDefaultPosition(int position_index)
        {
            defaults.single_position = position_index;
        }

        public void SetDefaultLayout(int layout_index)
        {
            defaults.single_layout = layout_index;
        }

        public void SetDefaultSprite(int sprite_index)
        {
            defaults.single_sprite = sprite_index;
        }

        public void SetDefaultSpriteType(int sprite_type_index)
        {
            defaults.single_sprite_type = sprite_type_index;
        }

        public void SetDefaultOtherData(params List<float>[] other_data)
        {
            defaults.single_other_data = other_data;
        }

        /*public void SetDefaults(IList<int> positions,IList<int> layouts,IList<int> sprites,IList<int> sprite_types,params IList<float>[] other_data){
            defaults = new SurfaceDefaults();
            if(positions != null){
                defaults.positions = new List<int>(positions);
            }
            if(layouts != null){
                defaults.layouts = new List<int>(layouts);
            }
            if(sprites != null){
                defaults.sprites = new List<int>(sprites);
            }
            if(sprite_types != null){
                defaults.sprite_types = new List<int>(sprite_types);
            }
            if(other_data != null){
                defaults.other_data = new List<float>[other_data.GetLength(0)];
                int idx = 0;
                foreach(List<float> l in other_data){
                    other_data[idx] = new List<float>(l);
                    ++idx;
                }
            }
        }*/
        public void SetDefaults(SurfaceDefaults new_defaults)
        {
            defaults = new_defaults;
        }

        public void DefaultUpdate()
        {
            SurfaceDefaults d = new SurfaceDefaults(defaults);
            d.FillValues(true, true);
            window.UpdatePositionVertexArray(this, d.positions, d.layouts);
            window.UpdateOtherVertexArray(this, -1, d.sprites, d.sprite_types, d.other_data);
        }

        public void DefaultUpdatePositions()
        {
            SurfaceDefaults d = new SurfaceDefaults(defaults);
            d.FillValues(true, false);
            window.UpdatePositionVertexArray(this, d.positions, d.layouts);
        }

        public void DefaultUpdateOtherData()
        {
            SurfaceDefaults d = new SurfaceDefaults(defaults);
            d.FillValues(false, true);
            window.UpdateOtherVertexArray(this, -1, d.sprites, d.sprite_types, d.other_data);
        }

        public void Update()
        {
            if (UpdateMethod != null)
            {
                SurfaceDefaults d = new SurfaceDefaults(defaults);
                UpdateMethod(d);
                d.FillValues(true, true);
                window.UpdatePositionVertexArray(this, d.positions, d.layouts);
                window.UpdateOtherVertexArray(this, -1, d.sprites, d.sprite_types, d.other_data);
            }
        }

        public void UpdatePositionsOnly()
        {
            if (UpdatePositionsOnlyMethod != null)
            {
                SurfaceDefaults d = new SurfaceDefaults(defaults);
                UpdatePositionsOnlyMethod(d);
                d.FillValues(true, false);
                window.UpdatePositionVertexArray(this, d.positions, d.layouts);
            }
        }

        public void UpdateOtherDataOnly()
        {
            if (UpdateOtherDataOnlyMethod != null)
            {
                SurfaceDefaults d = new SurfaceDefaults(defaults);
                UpdateOtherDataOnlyMethod(d);
                d.FillValues(false, true);
                window.UpdateOtherVertexArray(this, -1, d.sprites, d.sprite_types, d.other_data);
            }
        }
    }

    public class VBO
    {
        public int PositionArrayBufferID;
        public int OtherArrayBufferID;
        public int ElementArrayBufferID;
        public VertexAttributes VertexAttribs;
        public int PositionDimensions = 2; //this value controls whether 2 or 3 values are stored for position.
        public int NumElements = 0;
        public int PositionDataSize = 0; //these 2 values track the number of float values in the VBOs.
        public int OtherDataSize = 0;

        protected VBO()
        {
        }

        public static VBO Create()
        {
            VBO v = new VBO();
            GL.GenBuffers(1, out v.PositionArrayBufferID);
            GL.GenBuffers(1, out v.OtherArrayBufferID);
            GL.GenBuffers(1, out v.ElementArrayBufferID);
            return v;
        }

        public static VBO Create(int position_dimensions, VertexAttributes attribs)
        {
            VBO v = new VBO();
            GL.GenBuffers(1, out v.PositionArrayBufferID);
            GL.GenBuffers(1, out v.OtherArrayBufferID);
            GL.GenBuffers(1, out v.ElementArrayBufferID);
            v.PositionDimensions = position_dimensions;
            v.VertexAttribs = attribs;
            return v;
        }
    }

    public class VertexAttributes
    {
        public float[][] Defaults;
        public int[] Size;
        public int TotalSize;

        public static VertexAttributes Create(params float[][] defaults)
        {
            VertexAttributes v = new VertexAttributes();
            int count = defaults.GetLength(0);
            v.Defaults = new float[count][];
            v.Size = new int[count];
            v.TotalSize = 0;
            int idx = 0;
            foreach (float[] f in defaults)
            {
                v.Defaults[idx] = f;
                v.Size[idx] = f.GetLength(0);
                v.TotalSize += v.Size[idx];
                ++idx;
            }

            return v;
        }

        public static VertexAttributes Create(params int[] counts)
        {
            //makes zeroed arrays in the given counts.
            VertexAttributes v = new VertexAttributes();
            int count = counts.GetLength(0);
            v.Defaults = new float[count][];
            v.Size = new int[count];
            v.TotalSize = 0;
            int idx = 0;
            foreach (int i in counts)
            {
                v.Defaults[idx] =
                    new float[i]; //todo: this method needs a note:  which attribs are assumed to be here already? if you Create(2), is that texcoords? and what?
                v.Size[idx] = i;
                v.TotalSize += i;
                ++idx;
            }

            return v;
        }
    }

    public class Texture
    {
        public int TextureIndex;
        public int TextureHeightPx;
        public int TextureWidthPx;
        public int DefaultSpriteTypeIndex = 0;
        public List<SpriteType> Sprite = null;

        protected static int next_texture = 0;

        protected static int
            max_textures =
                -1; //Currently, max_textures serves only to crash in a better way. Eventually I'll figure out how to swap texture units around, todo!

        protected static Dictionary<string, Texture>
            texture_info =
                new Dictionary<string, Texture>(); //the Textures contained herein are used only to store index/height/width

        public static Texture Create(string filename, string textureToReplace = null,
            bool loadFromEmbeddedResource = false)
        {
            Texture t = new Texture();
            t.Sprite = new List<SpriteType>();
            if (textureToReplace != null)
            {
                t.ReplaceTexture(filename, textureToReplace, loadFromEmbeddedResource);
            }
            else
            {
                t.LoadTexture(filename, loadFromEmbeddedResource);
            }

            return t;
        }

        protected Texture()
        {
        }

        protected void LoadTexture(string filename, bool loadFromEmbeddedResource = false)
        {
            if (String.IsNullOrEmpty(filename))
            {
                throw new ArgumentException(filename);
            }

            if (texture_info.ContainsKey(filename))
            {
                Texture t = texture_info[filename];
                TextureIndex = t.TextureIndex;
                TextureHeightPx = t.TextureHeightPx;
                TextureWidthPx = t.TextureWidthPx;
            }
            else
            {
                if (max_textures == -1)
                {
                    GL.GetInteger(GetPName.MaxTextureImageUnits, out max_textures);
                }

                int num = next_texture++;
                if (num == max_textures)
                {
                    //todo: eventually fix this
                    throw new NotSupportedException("This machine only supports " + num +
                                                    " texture units, and this GL code isn't smart enough to switch them out yet, sorry.");
                }

                GL.ActiveTexture(TextureUnit.Texture0 + num);
                int
                    id = GL.GenTexture(); //todo: eventually i'll want to support more than 16 or 32 textures. At that time I'll need to store this ID somewhere.
                GL.BindTexture(TextureTarget.Texture2D,
                    id); //maybe a list of Scenes which are lists of textures needed, and then i'll bind all those and make sure to track their texture units.
                Bitmap bmp;
                if (loadFromEmbeddedResource)
                {
                    bmp = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(filename));
                }
                else
                {
                    bmp = new Bitmap(filename);
                }

                BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                bmp.UnlockBits(bmp_data);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int) TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int) TextureMagFilter.Nearest);
                TextureIndex = num;
                TextureHeightPx = bmp.Height;
                TextureWidthPx = bmp.Width;
                Texture
                    t = new Texture(); //this one goes into the dictionary as an easy way to store the index/height/width of this filename.
                t.TextureIndex = num;
                t.TextureHeightPx = bmp.Height;
                t.TextureWidthPx = bmp.Width;
                texture_info.Add(filename, t);
            }
        }

        protected void ReplaceTexture(string filename, string replaced, bool loadFromEmbeddedResource = false)
        {
            if (String.IsNullOrEmpty(filename))
            {
                throw new ArgumentException(filename);
            }

            if (texture_info.ContainsKey(filename))
            {
                Texture t = texture_info[filename];
                TextureIndex = t.TextureIndex;
                TextureHeightPx = t.TextureHeightPx;
                TextureWidthPx = t.TextureWidthPx;
            }
            else
            {
                int num;
                if (texture_info.ContainsKey(replaced))
                {
                    num = texture_info[replaced].TextureIndex;
                    texture_info.Remove(replaced);
                }
                else
                {
                    if (max_textures == -1)
                    {
                        GL.GetInteger(GetPName.MaxTextureImageUnits, out max_textures);
                    }

                    num = next_texture++;
                    if (num == max_textures)
                    {
                        //todo: eventually fix this
                        throw new NotSupportedException("This machine only supports " + num +
                                                        " texture units, and this GL code isn't smart enough to switch them out yet, sorry.");
                    }
                }

                GL.ActiveTexture(TextureUnit.Texture0 + num);
                int
                    id = GL.GenTexture(); //todo: eventually i'll want to support more than 16 or 32 textures. At that time I'll need to store this ID somewhere.
                GL.BindTexture(TextureTarget.Texture2D,
                    id); //maybe a list of Scenes which are lists of textures needed, and then i'll bind all those and make sure to track their texture units.
                Bitmap bmp;
                if (loadFromEmbeddedResource)
                {
                    bmp = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(filename));
                }
                else
                {
                    bmp = new Bitmap(filename);
                }

                BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                bmp.UnlockBits(bmp_data);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int) TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int) TextureMagFilter.Nearest);
                TextureIndex = num;
                TextureHeightPx = bmp.Height;
                TextureWidthPx = bmp.Width;
                Texture
                    t = new Texture(); //this one goes into the dictionary as an easy way to store the index/height/width of this filename.
                t.TextureIndex = num;
                t.TextureHeightPx = bmp.Height;
                t.TextureWidthPx = bmp.Width;
                texture_info.Add(filename, t);
            }
        }
    }

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
            float texcoord_width = (float) sprite_width_px * 1.0f / (float) surface.texture.TextureWidthPx;
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

        public static SpriteType DefineSingleRowSprite(Surface surface, int sprite_width_px,
            int padding_between_sprites_px)
        {
            SpriteType s = new SpriteType();
            float px_width = 1.0f / (float) surface.texture.TextureWidthPx;
            float texcoord_width = (float) sprite_width_px * px_width;
            float texcoord_start = texcoord_width + (float) padding_between_sprites_px * px_width;
            s.X = idx => idx * texcoord_start;
            s.Y = idx => 0;
            s.SpriteWidth = texcoord_width;
            s.SpriteHeight = 1.0f;
            if (surface != null)
            {
                surface.texture.Sprite.Add(s);
            }

            return s;
        }

        public static SpriteType DefineSpriteAcross(Surface surface, int sprite_width_px, int sprite_height_px,
            int num_columns)
        {
            SpriteType s = new SpriteType();
            float texcoord_width = (float) sprite_width_px * 1.0f / (float) surface.texture.TextureWidthPx;
            float texcoord_height = (float) sprite_height_px * 1.0f / (float) surface.texture.TextureHeightPx;
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

        public static SpriteType DefineSpriteAcross(Surface surface, int sprite_width_px, int sprite_height_px,
            int num_columns, int h_offset_px, int v_offset_px)
        {
            SpriteType s = new SpriteType();
            float texcoord_width = (float) sprite_width_px * 1.0f / (float) surface.texture.TextureWidthPx;
            float texcoord_height = (float) sprite_height_px * 1.0f / (float) surface.texture.TextureHeightPx;
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

        public static SpriteType DefineSpriteDown(Surface surface, int sprite_width_px, int sprite_height_px,
            int num_rows)
        {
            SpriteType s = new SpriteType();
            float texcoord_width = (float) sprite_width_px * 1.0f / (float) surface.texture.TextureWidthPx;
            float texcoord_height = (float) sprite_height_px * 1.0f / (float) surface.texture.TextureHeightPx;
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

        public static SpriteType DefineSpriteDown(Surface surface, int sprite_width_px, int sprite_height_px,
            int num_rows, int h_offset_px, int v_offset_px)
        {
            SpriteType s = new SpriteType();
            float texcoord_width = (float) sprite_width_px * 1.0f / (float) surface.texture.TextureWidthPx;
            float texcoord_height = (float) sprite_height_px * 1.0f / (float) surface.texture.TextureHeightPx;
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

    public class CellLayout
    {
        public PositionFromIndex X;
        public PositionFromIndex Y;
        public PositionFromIndex Z = null; //Z isn't used unless the VBO object has PositionDimensions set to 3.
        public int CellHeightPx; //in pixels
        public int CellWidthPx;
        public int VerticalOffsetPx;
        public int HorizontalOffsetPx;

        public static CellLayout CreateGrid(Surface s, int rows, int cols, int cell_height_px, int cell_width_px,
            int v_offset_px, int h_offset_px, PositionFromIndex z = null)
        {
            CellLayout c = new CellLayout();
            c.CellHeightPx = cell_height_px;
            c.CellWidthPx = cell_width_px;
            c.VerticalOffsetPx = v_offset_px;
            c.HorizontalOffsetPx = h_offset_px;
            c.X = idx => (idx % cols) * c.CellWidthPx;
            c.Y = idx => (idx / cols) * c.CellHeightPx;
            c.Z = z;
            if (s != null)
            {
                s.layouts.Add(c);
            }

            return c;
        }

        public static CellLayout CreateIso(Surface s, int rows, int cols, int cell_height_px, int cell_width_px,
            int v_offset_px, int h_offset_px, int cell_v_offset_px, int cell_h_offset_px, PositionFromIndex z = null,
            PositionFromIndex elevation = null)
        {
            CellLayout c = new CellLayout();
            c.CellHeightPx = cell_height_px;
            c.CellWidthPx = cell_width_px;
            c.VerticalOffsetPx = v_offset_px;
            c.HorizontalOffsetPx = h_offset_px;
            c.X = idx => (rows - 1 - (idx / cols) + (idx % cols)) * cell_h_offset_px;
            if (elevation == null)
            {
                c.Y = idx => ((idx / cols) + (idx % cols)) * cell_v_offset_px;
            }
            else
            {
                c.Y = idx => ((idx / cols) + (idx % cols)) * cell_v_offset_px + elevation(idx);
            }

            c.Z = z;
            if (s != null)
            {
                s.layouts.Add(c);
            }

            return c;
        }

        public static CellLayout CreateIsoAtOffset(Surface s, int rows, int cols, int base_start_row,
            int base_start_col, int base_rows, int cell_height_px, int cell_width_px, int v_offset_px, int h_offset_px,
            int cell_v_offset_px, int cell_h_offset_px, PositionFromIndex z = null, PositionFromIndex elevation = null)
        {
            CellLayout c = new CellLayout();
            c.CellHeightPx = cell_height_px;
            c.CellWidthPx = cell_width_px;
            c.VerticalOffsetPx = v_offset_px;
            c.HorizontalOffsetPx = h_offset_px;
            c.X = idx => (base_rows - 1 - (idx / cols + base_start_row) + (idx % cols + base_start_col)) *
                         cell_h_offset_px;
            if (elevation == null)
            {
                c.Y = idx => ((idx / cols + base_start_row) + (idx % cols + base_start_col)) * cell_v_offset_px;
            }
            else
            {
                c.Y = idx =>
                    ((idx / cols + base_start_row) + (idx % cols + base_start_col)) * cell_v_offset_px + elevation(idx);
            }

            c.Z = z;
            if (s != null)
            {
                s.layouts.Add(c);
            }

            return c;
        }

        public static CellLayout Create(Surface s, int cell_height_px, int cell_width_px, int v_offset_px,
            int h_offset_px, PositionFromIndex x, PositionFromIndex y, PositionFromIndex z = null)
        {
            CellLayout c = new CellLayout(); //todo: fix x/y order for entire file?
            c.CellHeightPx = cell_height_px;
            c.CellWidthPx = cell_width_px;
            c.VerticalOffsetPx = v_offset_px;
            c.HorizontalOffsetPx = h_offset_px;
            c.X = x;
            c.Y = y;
            c.Z = z;
            if (s != null)
            {
                s.layouts.Add(c);
            }

            return c;
        }
    }

    public class Shader
    {
        public int ShaderProgramID;
        public int OffsetUniformLocation;
        public int TextureUniformLocation;

        protected class id_and_programs
        {
            public int id;
            public Dictionary<int, Shader> programs = null;
        }

        protected static Dictionary<string, id_and_programs> compiled_vs = new Dictionary<string, id_and_programs>();
        protected static Dictionary<string, int> compiled_fs = new Dictionary<string, int>();

        public static Shader Create(string frag_shader)
        {
            return Create(DefaultVS(), frag_shader);
        }

        public static Shader Create(string vert_shader, string frag_shader)
        {
            Shader s = new Shader();
            int vertex_shader = -1;
            if (compiled_vs.ContainsKey(vert_shader))
            {
                vertex_shader = compiled_vs[vert_shader].id;
            }
            else
            {
                vertex_shader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(vertex_shader, vert_shader);
                GL.CompileShader(vertex_shader);
                int compiled;
                GL.GetShader(vertex_shader, ShaderParameter.CompileStatus, out compiled);
                if (compiled < 1)
                {
                    Console.Error.WriteLine(GL.GetShaderInfoLog(vertex_shader));
                    throw new Exception("vertex shader compilation failed");
                }

                id_and_programs v = new id_and_programs();
                v.id = vertex_shader;
                compiled_vs.Add(vert_shader, v);
            }

            int fragment_shader = -1;
            if (compiled_fs.ContainsKey(frag_shader))
            {
                fragment_shader = compiled_fs[frag_shader];
            }
            else
            {
                fragment_shader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(fragment_shader, frag_shader);
                GL.CompileShader(fragment_shader);
                int compiled;
                GL.GetShader(fragment_shader, ShaderParameter.CompileStatus, out compiled);
                if (compiled < 1)
                {
                    Console.Error.WriteLine(GL.GetShaderInfoLog(fragment_shader));
                    throw new Exception("fragment shader compilation failed");
                }

                compiled_fs.Add(frag_shader, fragment_shader);
            }

            if (compiled_vs[vert_shader].programs != null &&
                compiled_vs[vert_shader].programs.ContainsKey(fragment_shader))
            {
                s.ShaderProgramID = compiled_vs[vert_shader].programs[fragment_shader].ShaderProgramID;
                s.OffsetUniformLocation = compiled_vs[vert_shader].programs[fragment_shader].OffsetUniformLocation;
                s.TextureUniformLocation = compiled_vs[vert_shader].programs[fragment_shader].TextureUniformLocation;
            }
            else
            {
                int shader_program = GL.CreateProgram();
                GL.AttachShader(shader_program, vertex_shader);
                GL.AttachShader(shader_program, fragment_shader);
                int attrib_index = 0;
                foreach (string attr in new string[] {"position", "texcoord", "color", "bgcolor"})
                {
                    GL.BindAttribLocation(shader_program, attrib_index++, attr);
                }

                GL.LinkProgram(shader_program);
                s.ShaderProgramID = shader_program;
                s.OffsetUniformLocation = GL.GetUniformLocation(shader_program, "offset");
                s.TextureUniformLocation = GL.GetUniformLocation(shader_program, "texture");
                if (compiled_vs[vert_shader].programs == null)
                {
                    compiled_vs[vert_shader].programs = new Dictionary<int, Shader>();
                }

                Shader p = new Shader();
                p.ShaderProgramID = shader_program;
                p.OffsetUniformLocation = s.OffsetUniformLocation;
                p.TextureUniformLocation = s.TextureUniformLocation;
                compiled_vs[vert_shader].programs.Add(fragment_shader, p);
            }

            return s;
        }

        public static string DefaultVS()
        {
            return
                @"#version 120
uniform vec2 offset;

attribute vec4 position;
attribute vec2 texcoord;
attribute vec4 color;
attribute vec4 bgcolor;

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;

void main(){
 texcoord_fs = texcoord;
 color_fs = color;
 bgcolor_fs = bgcolor;
 gl_Position = vec4(position.x + offset.x,-position.y - offset.y,position.z,1);
}
";
        }

        public static string DefaultFS()
        {
            //todo: I could make a builder for these, kinda. It could make things like alpha testing optional.
            return
                @"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 if(v.a < 0.1){
  discard;
 }
 //gl_FragColor = texture2D(texture,texcoord_fs);
 gl_FragColor = v;
}
";
        }

        public static string FontFS()
        {
            return
                @"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 if(v.r == 1.0 && v.g == 1.0 && v.b == 1.0){
  gl_FragColor = color_fs;
 }
 else{
  gl_FragColor = bgcolor_fs;
 }
}
";
        }

        public static string AAFontFS()
        {
            return
                @"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 gl_FragColor = vec4(bgcolor_fs.r * (1.0 - v.a) + color_fs.r * v.a,bgcolor_fs.g * (1.0 - v.a) + color_fs.g * v.a,bgcolor_fs.b * (1.0 - v.a) + color_fs.b * v.a,bgcolor_fs.a * (1.0 - v.a) + color_fs.a * v.a);
}
";
        }

        public static string TintFS()
        {
            return
                @"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;
varying vec4 color_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 if(v.a < 0.1){
  discard;
 }
 gl_FragColor = vec4(v.r * color_fs.r,v.g * color_fs.g,v.b * color_fs.b,v.a * color_fs.a);
}
";
        }

        public static string NewTintFS()
        {
            return
                @"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 if(v.a < 0.1){
  discard;
 }
 gl_FragColor = vec4(v.r * color_fs.r + bgcolor_fs.r,v.g * color_fs.g + bgcolor_fs.g,v.b * color_fs.b + bgcolor_fs.b,v.a * color_fs.a + bgcolor_fs.a);
}
";
        }

        public static string GrayscaleFS()
        {
            return
                @"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 if(v.a < 0.1){
  discard;
 }
 float f = 0.1 * v.r + 0.2 * v.b + 0.7 * v.g;
 gl_FragColor = vec4(f,f,f,v.a);
}
";
        }

        public static string GrayscaleWithColorsFS()
        {
            return
                @"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 if(v.a < 0.1){
  discard;
 }
 float f = 0.1 * v.r + 0.7 * v.g + 0.2 * v.b;
 gl_FragColor = vec4(f,f,f,v.a);
 if((v.r > 0.6 && v.g < 0.4 && v.b < 0.4) || (v.g > 0.6 && v.r < 0.4 && v.b < 0.4) || (v.b > 0.6 && v.r < 0.4 && v.g < 0.4)){
  gl_FragColor = v;
 }
}
";
        }
    }
}