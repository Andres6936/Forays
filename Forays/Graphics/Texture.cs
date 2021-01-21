using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using OpenTK.Graphics.OpenGL;

namespace Forays
{
    public class Texture
    {
        public int TextureIndex;
        public int TextureWidthPx;
        public List<SpriteType> Sprite;

        private int textureHeightPx;
        private static int _nextTexture;

        /// <summary>
        /// Currently, max_textures serves only to crash in a better way.
        /// Eventually I'll figure out how to swap texture units around!.
        /// </summary>
        private static int _maxTextures = -1;

        /// <summary>
        /// The Textures contained herein are used only to store index/height/width.
        /// </summary>
        private static readonly Dictionary<string, Texture> TextureInfo =
            new Dictionary<string, Texture>();

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

        private Texture()
        {
        }

        private void LoadTexture(string filename, bool loadFromEmbeddedResource = false)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException(filename);
            }

            if (TextureInfo.ContainsKey(filename))
            {
                Texture t = TextureInfo[filename];
                TextureIndex = t.TextureIndex;
                textureHeightPx = t.textureHeightPx;
                TextureWidthPx = t.TextureWidthPx;
            }
            else
            {
                if (_maxTextures == -1)
                {
                    GL.GetInteger(GetPName.MaxTextureImageUnits, out _maxTextures);
                }

                int num = _nextTexture++;
                if (num == _maxTextures)
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
                    bmp = new Bitmap(Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream(filename));
                }
                else
                {
                    bmp = new Bitmap(filename);
                }

                BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width,
                    bmp_data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte,
                    bmp_data.Scan0);
                bmp.UnlockBits(bmp_data);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int) TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int) TextureMagFilter.Nearest);
                TextureIndex = num;
                textureHeightPx = bmp.Height;
                TextureWidthPx = bmp.Width;
                Texture
                    t = new Texture(); //this one goes into the dictionary as an easy way to store the index/height/width of this filename.
                t.TextureIndex = num;
                t.textureHeightPx = bmp.Height;
                t.TextureWidthPx = bmp.Width;
                TextureInfo.Add(filename, t);
            }
        }

        private void ReplaceTexture(string filename, string replaced,
            bool loadFromEmbeddedResource = false)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException(filename);
            }

            if (TextureInfo.ContainsKey(filename))
            {
                Texture t = TextureInfo[filename];
                TextureIndex = t.TextureIndex;
                textureHeightPx = t.textureHeightPx;
                TextureWidthPx = t.TextureWidthPx;
            }
            else
            {
                int num;
                if (TextureInfo.ContainsKey(replaced))
                {
                    num = TextureInfo[replaced].TextureIndex;
                    TextureInfo.Remove(replaced);
                }
                else
                {
                    if (_maxTextures == -1)
                    {
                        GL.GetInteger(GetPName.MaxTextureImageUnits, out _maxTextures);
                    }

                    num = _nextTexture++;
                    if (num == _maxTextures)
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
                    bmp = new Bitmap(Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream(filename));
                }
                else
                {
                    bmp = new Bitmap(filename);
                }

                BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width,
                    bmp_data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte,
                    bmp_data.Scan0);
                bmp.UnlockBits(bmp_data);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int) TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int) TextureMagFilter.Nearest);
                TextureIndex = num;
                textureHeightPx = bmp.Height;
                TextureWidthPx = bmp.Width;
                Texture
                    t = new Texture(); //this one goes into the dictionary as an easy way to store the index/height/width of this filename.
                t.TextureIndex = num;
                t.textureHeightPx = bmp.Height;
                t.TextureWidthPx = bmp.Width;
                TextureInfo.Add(filename, t);
            }
        }
    }
}