/*Copyright (c) 2014-2015  Derrick Creamer
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/

using System;
using System.Collections.Generic;
using Forays;
using Forays.Renderer;
using Forays.Graphics;

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


    public class Surface
    {
        private OpenTk window;
        private SurfaceDefaults defaults = new SurfaceDefaults();
        public VertexBufferObject VertexBufferObject;
        public Texture Texture;
        public Shader Shader;
        public List<CellLayout> Layouts = new List<CellLayout>();
        public float RawXOffset;
        public float RawYOffset;
        public bool UseDepthBuffer;
        public bool Disabled = false;


        public static Surface Create(OpenTk window, string textureFilename,
            bool loadTextureFromEmbeddedResource,
            string fragShader, bool hasDepth, params int[] vertexAttributeCounts)
        {
            var s = new Surface
            {
                window = window,
                UseDepthBuffer = hasDepth,
                // hasDepth ? 3 : 2 - This section of code determine the dimensions
                // of vertex buffer object.
                VertexBufferObject = VertexBufferObject.Create(hasDepth ? 3 : 2,
                    VertexAttributes.Create(vertexAttributeCounts)),
                Texture = Texture.Create(Global.ForaysImageResources + textureFilename,
                    null, loadTextureFromEmbeddedResource),
                Shader = Shader.Create(fragShader)
            };

            window?.Surfaces.Add(s);

            return s;
        }

        public void SetOffsetInPixels(int xOffsetPx, int yOffsetPx)
        {
            RawXOffset = (float) (xOffsetPx * 2) / (float) window.Viewport.Width;
            RawYOffset = (float) (yOffsetPx * 2) / (float) window.Viewport.Height;
        }

        public void SetEasyLayoutCounts(params int[] countsPerLayout)
        {
            if (countsPerLayout.GetLength(0) != Layouts.Count)
            {
                throw new ArgumentException("SetEasyLayoutCounts: Number of arguments (" +
                                            countsPerLayout.GetLength(0) +
                                            ") must match number of layouts (" +
                                            Layouts.Count + ").");
            }

            defaults.positions =
                new List<int>(); //this method creates the default lists used by Update()
            defaults.layouts = new List<int>();
            int idx = 0;
            foreach (int count in countsPerLayout)
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

        public void SetDefaultSprite(int spriteIndex)
        {
            defaults.single_sprite = spriteIndex;
        }

        public void SetDefaultSpriteType(int spriteTypeIndex)
        {
            defaults.single_sprite_type = spriteTypeIndex;
        }

        public void SetDefaultOtherData(params List<float>[] otherData)
        {
            defaults.single_other_data = otherData;
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
    }
}