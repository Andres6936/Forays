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

    public delegate void SurfaceUpdateMethod(SurfaceDefaults defaults);


    public class Surface
    {
        public OpenTk window;
        public VertexBufferObject VertexBufferObject;
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
                texture = Texture.Create(Global.ForaysImageResources + textureFilename,
                    null, loadTextureFromEmbeddedResource),
                shader = Shader.Create(fragShader)
            };

            window?.Surfaces.Add(s);

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
            return x_offset_px + layout.HorizontalOffset;
        }

        public int TotalXOffsetPx()
        {
            return x_offset_px + layouts[0].HorizontalOffset;
        }

        public int TotalYOffsetPx(CellLayout layout)
        {
            return y_offset_px + layout.VerticalOffset;
        }

        public void SetEasyLayoutCounts(params int[] counts_per_layout)
        {
            if (counts_per_layout.GetLength(0) != layouts.Count)
            {
                throw new ArgumentException("SetEasyLayoutCounts: Number of arguments (" +
                                            counts_per_layout.GetLength(0) +
                                            ") must match number of layouts (" +
                                            layouts.Count + ").");
            }

            defaults.positions =
                new List<int>(); //this method creates the default lists used by Update()
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