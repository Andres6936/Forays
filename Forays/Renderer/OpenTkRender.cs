using System;
using System.Drawing;
using System.Collections.Generic;
using Forays.Renderer;
using GLDrawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Forays.Renderer
{
    public class OpenTkRender : GameWindow, IRenderer
    {
        public List<Surface> Surfaces = new List<Surface>();

        public ResizeOption ResizingPreference = ResizeOption.StretchToFit;
        public ResizeOption ResizingFullScreenPreference = ResizeOption.StretchToFit;
        protected bool Resizing = false;

        public int
            SnapWidth = 1; //if EnforceRatio is true, the AddBorder and SnapWindow options will require that the new multiples of SnapHeight and SnapWidth be equal.

        public int
            SnapHeight =
                1; //Assuming SnapW is 100 and SnapH is 50:  If EnforceRatio is true, the valid sizes are 100x50, 200x100, 300x150, and so on.

        public bool
            EnforceRatio = false; //If EnforceRatio is false, 100x750 is a valid size, as are 900x50 and 200x200.

        public bool NoClose = false;
        public bool FullScreen = false;

        protected FrameEventArgs render_args = new FrameEventArgs();
        protected Dictionary<Key, bool> key_down = new Dictionary<Key, bool>();
        protected bool DepthTestEnabled = false;
        protected int LastShaderID = -1;

        public Action FinalResize = null;
        protected Rectangle internalViewport;

        public Rectangle Viewport
        {
            get { return internalViewport; }
            set
            {
                internalViewport = value;
                GL.Viewport(value);
            }
        }

        public void SetViewport(int x, int y, int width, int height)
        {
            internalViewport = new Rectangle(x, y, width, height);
            GL.Viewport(x, y, width, height);
        }

        public OpenTkRender(int w, int h, string title) : base(w, h, GraphicsMode.Default, title)
        {
            VSync = VSyncMode.On;
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.EnableVertexAttribArray(0); //these 2 attrib arrays are always on, for position and texcoords.
            GL.EnableVertexAttribArray(1);
            KeyDown += KeyDownHandler;
            KeyUp += KeyUpHandler;
            Keyboard.KeyRepeat = true;
            internalViewport = new Rectangle(0, 0, w, h);
        }

        protected virtual void KeyDownHandler(object sender, KeyboardKeyEventArgs args)
        {
            key_down[args.Key] = true;
        }

        protected virtual void KeyUpHandler(object sender, KeyboardKeyEventArgs args)
        {
            key_down[args.Key] = false;
        }

        public bool KeyIsDown(Key key)
        {
            bool value;
            key_down.TryGetValue(key, out value);
            return value;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = NoClose;
            base.OnClosing(e);
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);

            if (!Focused) return;

            key_down[Key.AltLeft] = false; //i could simply reset the whole dictionary, too...
            key_down[Key.AltRight] = false;
            key_down[Key.ShiftLeft] = false;
            key_down[Key.ShiftRight] = false;
            key_down[Key.ControlLeft] = false;
            key_down[Key.ControlRight] = false;
        }

        protected override void OnResize(EventArgs e)
        {
            Resizing = true;
        }

        public void DefaultHandleResize()
        {
            ResizeOption pref = ResizingPreference;
            if (FullScreen)
            {
                pref = ResizingFullScreenPreference;
            }

            switch (pref)
            {
                case ResizeOption.StretchToFit:
                    SetViewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
                    break;
                case ResizeOption.AddBorder
                    : //should AddBorder allow the screen to be resized below SnapWidth x SnapHeight? Maybe an option?
                {
                    int height_multiple = ClientRectangle.Height / SnapHeight;
                    int width_multiple = ClientRectangle.Width / SnapWidth;
                    if (EnforceRatio)
                    {
                        int min = Math.Min(height_multiple, width_multiple);
                        height_multiple = min;
                        width_multiple = min;
                    }

                    if (height_multiple < 1)
                    {
                        height_multiple = 1;
                    }

                    if (width_multiple < 1)
                    {
                        width_multiple = 1;
                    }

                    int view_height = SnapHeight * height_multiple;
                    int view_width = SnapWidth * width_multiple;
                    SetViewport((ClientRectangle.Width - view_width) / 2, (ClientRectangle.Height - view_height) / 2,
                        view_width, view_height);
                    break;
                }
                case ResizeOption.SnapWindow: //you probably don't want to use this option for fullscreen.
                {
                    int height_multiple = ClientRectangle.Height / SnapHeight;
                    int width_multiple = ClientRectangle.Width / SnapWidth;
                    if (EnforceRatio)
                    {
                        int min = Math.Min(height_multiple, width_multiple);
                        height_multiple = min;
                        width_multiple = min;
                    }

                    if (height_multiple < 1)
                    {
                        height_multiple = 1;
                    }

                    if (width_multiple < 1)
                    {
                        width_multiple = 1;
                    }

                    Height = SnapHeight * height_multiple;
                    Width = SnapWidth * width_multiple;
                    SetViewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
                    break;
                }
                case ResizeOption.NoResize:
                    Height = SnapHeight;
                    Width = SnapWidth;
                    break;
            }
        }

        public void ToggleFullScreen()
        {
            ToggleFullScreen(!FullScreen);
        }

        public void ToggleFullScreen(bool fullscreen_on)
        {
            FullScreen = fullscreen_on;
            if (fullscreen_on)
            {
                WindowState = WindowState.Fullscreen;
            }
            else
            {
                WindowState = WindowState.Normal;
            }

            Resizing = true; //todo: changed this to simply set Resizing instead of actually calling resizing methods.
        }

        public bool WindowUpdate()
        {
            ProcessEvents();
            if (IsExiting)
            {
                return false;
            }

            if (Resizing)
            {
                //HandleResize();
                FinalResize?.Invoke();
                Resizing = false;
            }

            DrawSurfaces();
            return true;
        }

        public void DrawSurfaces()
        {
            base.OnRenderFrame(render_args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (Surface s in Surfaces)
            {
                if (!s.Disabled)
                {
                    if (DepthTestEnabled != s.UseDepthBuffer)
                    {
                        if (s.UseDepthBuffer)
                        {
                            GL.Enable(EnableCap.DepthTest);
                        }
                        else
                        {
                            GL.Disable(EnableCap.DepthTest);
                        }

                        DepthTestEnabled = s.UseDepthBuffer;
                    }

                    if (LastShaderID != s.shader.ShaderProgramID)
                    {
                        GL.UseProgram(s.shader.ShaderProgramID);
                        LastShaderID = s.shader.ShaderProgramID;
                    }

                    GL.Uniform2(s.shader.OffsetUniformLocation, s.raw_x_offset, s.raw_y_offset);
                    GL.Uniform1(s.shader.TextureUniformLocation, s.texture.TextureIndex);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, s.vbo.ElementArrayBufferID);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, s.vbo.PositionArrayBufferID);
                    GL.VertexAttribPointer(0, s.vbo.PositionDimensions, VertexAttribPointerType.Float, false,
                        sizeof(float) * s.vbo.PositionDimensions, new IntPtr(0)); //position
                    GL.BindBuffer(BufferTarget.ArrayBuffer, s.vbo.OtherArrayBufferID);
                    int stride = sizeof(float) * s.vbo.VertexAttribs.TotalSize;
                    GL.VertexAttribPointer(1, s.vbo.VertexAttribs.Size[0], VertexAttribPointerType.Float, false, stride,
                        new IntPtr(0)); //texcoords
                    int total_of_previous_attribs = s.vbo.VertexAttribs.Size[0];
                    for (int i = 1; i < s.vbo.VertexAttribs.Size.Length; ++i)
                    {
                        GL.EnableVertexAttribArray(
                            i + 1); //i+1 because 0 and 1 are always on (for position & texcoords)
                        GL.VertexAttribPointer(i + 1, s.vbo.VertexAttribs.Size[i], VertexAttribPointerType.Float, false,
                            stride, new IntPtr(sizeof(float) * total_of_previous_attribs));
                        total_of_previous_attribs += s.vbo.VertexAttribs.Size[i];
                    }

                    GL.DrawElements(PrimitiveType.Triangles, s.vbo.NumElements, DrawElementsType.UnsignedInt,
                        IntPtr.Zero);
                    for (int i = 1; i < s.vbo.VertexAttribs.Size.Length; ++i)
                    {
                        GL.DisableVertexAttribArray(i + 1);
                    }
                }
            }

            SwapBuffers();
        }

        /*public static void ReplaceTexture(int texture_unit,string filename){ //binds a texture to the given texture unit, replacing the texture that's already there
            if(String.IsNullOrEmpty(filename)){
                throw new ArgumentException(filename);
            }
            GL.ActiveTexture(TextureUnit.Texture0 + texture_unit);
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D,id);
            Bitmap bmp = new Bitmap(filename);
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0,0,bmp.Width,bmp.Height),ImageLockMode.ReadOnly,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Rgba,bmp_data.Width,bmp_data.Height,0,OpenTK.Graphics.OpenGL.PixelFormat.Bgra,PixelType.UnsignedByte,bmp_data.Scan0);
            bmp.UnlockBits(bmp_data);
            GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMinFilter,(int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMagFilter,(int)TextureMagFilter.Nearest);
        }*/
        public void UpdatePositionVertexArray(Surface s, IList<int> index_list, IList<int> layout_list = null)
        {
            UpdatePositionVertexArray(s, -1, index_list, layout_list);
        }

        public void UpdatePositionVertexArray(Surface s, int start_index, IList<int> index_list,
            IList<int> layout_list = null)
        {
            int count = index_list.Count;
            if (layout_list == null)
            {
                layout_list = new int[count]; //if not supplied, assume layout 0.
            }

            float[] values =
                new float[count * 4 * s.vbo.PositionDimensions]; //2 or 3 dimensions for 4 vertices for each tile
            int[] indices = null;
            if (start_index < 0 && s.vbo.NumElements != count * 6)
            {
                indices = new int[count * 6];
                s.vbo.NumElements = count * 6;
            }
            else
            {
                if (start_index >= 0)
                {
                    if ((start_index + count) * 6 > s.vbo.NumElements && s.vbo.NumElements > 0)
                    {
                        throw new ArgumentException(
                            "Error: start_index + count is bigger than VBO size. To always replace the previous data, set start_index to -1.");
                    } //todo: I could also just ignore the start_index if there's too much data.

                    if ((start_index + count) * 4 * s.vbo.PositionDimensions > s.vbo.PositionDataSize &&
                        s.vbo.PositionDataSize > 0)
                    {
                        throw new ArgumentException(
                            "Error: (start_index + count) * total_attrib_size is bigger than VBO size. To always replace the previous data, set start_index to -1.");
                    }
                }
            }

            float width_ratio = 2.0f / (float) Viewport.Width;
            float height_ratio = 2.0f / (float) Viewport.Height;
            int current_total = 0;
            foreach (int i in index_list)
            {
                float x_offset = (float) s.layouts[layout_list[current_total]].HorizontalOffsetPx;
                float y_offset = (float) s.layouts[layout_list[current_total]].VerticalOffsetPx;
                float x_w = (float) s.layouts[layout_list[current_total]].CellWidthPx;
                float y_h = (float) s.layouts[layout_list[current_total]].CellHeightPx;
                float cellx = s.layouts[layout_list[current_total]].X(i) + x_offset;
                float celly = s.layouts[layout_list[current_total]].Y(i) + y_offset;
                float x = cellx * width_ratio - 1.0f;
                float y = celly * height_ratio - 1.0f;
                float x_plus1 = (cellx + x_w) * width_ratio - 1.0f;
                float y_plus1 = (celly + y_h) * height_ratio - 1.0f;

                int N = s.vbo.PositionDimensions;
                int idxN = current_total * 4 * N;

                values[idxN] = x; //the 4 corners, flipped so it works with the inverted Y axis
                values[idxN + 1] = y_plus1;
                values[idxN + N] = x;
                values[idxN + N + 1] = y;
                values[idxN + N * 2] = x_plus1;
                values[idxN + N * 2 + 1] = y;
                values[idxN + N * 3] = x_plus1;
                values[idxN + N * 3 + 1] = y_plus1;
                if (N == 3)
                {
                    float z = s.layouts[layout_list[current_total]].Z(i);
                    values[idxN + 2] = z;
                    values[idxN + N + 2] = z;
                    values[idxN + N * 2 + 2] = z;
                    values[idxN + N * 3 + 2] = z;
                }

                if (indices != null)
                {
                    int idx4 = current_total * 4;
                    int idx6 = current_total * 6;
                    indices[idx6] = idx4;
                    indices[idx6 + 1] = idx4 + 1;
                    indices[idx6 + 2] = idx4 + 2;
                    indices[idx6 + 3] = idx4;
                    indices[idx6 + 4] = idx4 + 2;
                    indices[idx6 + 5] = idx4 + 3;
                }

                current_total++;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, s.vbo.PositionArrayBufferID);
            if ((start_index < 0 && s.vbo.PositionDataSize != values.Length) || s.vbo.PositionDataSize == 0)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * values.Length), values,
                    BufferUsageHint.StreamDraw);
                s.vbo.PositionDataSize = values.Length;
            }
            else
            {
                int offset = start_index;
                if (offset < 0)
                {
                    offset = 0;
                }

                GL.BufferSubData(BufferTarget.ArrayBuffer,
                    new IntPtr(sizeof(float) * 4 * s.vbo.PositionDimensions * offset),
                    new IntPtr(sizeof(float) * values.Length), values);
                //GL.BufferSubData(BufferTarget.ArrayBuffer,new IntPtr(0),new IntPtr(sizeof(float) * values.Length),values);
            }

            if (indices != null)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, s.vbo.ElementArrayBufferID);
                GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(int) * indices.Length), indices,
                    BufferUsageHint.StaticDraw);
            }
        }

        public void UpdatePositionSingleVertex(Surface s, int index, int layout = 0)
        {
            float[] values = new float[4 * s.vbo.PositionDimensions]; //2 or 3 dimensions for 4 vertices
            float width_ratio = 2.0f / (float) Viewport.Width;
            float height_ratio = 2.0f / (float) Viewport.Height;
            float x_offset = (float) s.layouts[layout].HorizontalOffsetPx;
            float y_offset = (float) s.layouts[layout].VerticalOffsetPx;
            float x_w = (float) s.layouts[layout].CellWidthPx;
            float y_h = (float) s.layouts[layout].CellHeightPx;
            float cellx = s.layouts[layout].X(index) + x_offset;
            float celly = s.layouts[layout].Y(index) + y_offset;
            float x = cellx * width_ratio - 1.0f;
            float y = celly * height_ratio - 1.0f;
            float x_plus1 = (cellx + x_w) * width_ratio - 1.0f;
            float y_plus1 = (celly + y_h) * height_ratio - 1.0f;

            int N = s.vbo.PositionDimensions;

            values[0] = x; //the 4 corners, flipped so it works with the inverted Y axis
            values[1] = y_plus1;
            values[N] = x;
            values[N + 1] = y;
            values[N * 2] = x_plus1;
            values[N * 2 + 1] = y;
            values[N * 3] = x_plus1;
            values[N * 3 + 1] = y_plus1;
            if (N == 3)
            {
                float z = s.layouts[layout].Z(index);
                values[2] = z;
                values[N + 2] = z;
                values[N * 2 + 2] = z;
                values[N * 3 + 2] = z;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, s.vbo.PositionArrayBufferID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * 4 * s.vbo.PositionDimensions * index),
                new IntPtr(sizeof(float) * values.Length), values);
        }

        public void UpdateOtherVertexArray(Surface s, IList<int> sprite_index, params IList<float>[] vertex_attributes)
        {
            UpdateOtherVertexArray(s, -1, sprite_index, new int[sprite_index.Count],
                vertex_attributes); //default to sprite type 0.
        } //should I add more overloads here?

        public void UpdateOtherVertexArray(Surface s, int start_index, IList<int> sprite_index, IList<int> sprite_type,
            params IList<float>[] vertex_attributes)
        {
            int count = sprite_index.Count;
            int a = s.vbo.VertexAttribs.TotalSize;
            int a4 = a * 4;
            if (start_index >= 0 && (start_index + count) * a4 > s.vbo.OtherDataSize && s.vbo.OtherDataSize > 0)
            {
                throw new ArgumentException(
                    "Error: (start_index + count) * total_attrib_size is bigger than VBO size. To always replace the previous data, set start_index to -1.");
            }

            if (sprite_type == null)
            {
                sprite_type = new int[sprite_index.Count];
            }

            float[] all_values = new float[count * a4];
            for (int i = 0; i < count; ++i)
            {
                SpriteType sprite = s.texture.Sprite[sprite_type[i]];
                float tex_start_x = sprite.X(sprite_index[i]);
                float tex_start_y = sprite.Y(sprite_index[i]);
                float tex_end_x = tex_start_x + sprite.SpriteWidth;
                float tex_end_y = tex_start_y + sprite.SpriteHeight;
                float[] values = new float[a4];
                values[0] = tex_start_x; //the 4 corners, texcoords:
                values[1] = tex_end_y;
                values[a] = tex_start_x;
                values[a + 1] = tex_start_y;
                values[a * 2] = tex_end_x;
                values[a * 2 + 1] = tex_start_y;
                values[a * 3] = tex_end_x;
                values[a * 3 + 1] = tex_end_y;
                int prev_total = 2;
                for (int g = 1; g < s.vbo.VertexAttribs.Size.Length; ++g)
                {
                    //starting at 1 because texcoords are already done
                    int attrib_size = s.vbo.VertexAttribs.Size[g];
                    for (int k = 0; k < attrib_size; ++k)
                    {
                        float attrib =
                            vertex_attributes[g - 1][
                                k + i *
                                attrib_size]; // -1 because the vertex_attributes array doesn't contain texcoords here in the update method.
                        values[prev_total + k] = attrib;
                        values[prev_total + k + a] = attrib;
                        values[prev_total + k + a * 2] = attrib;
                        values[prev_total + k + a * 3] = attrib;
                    }

                    prev_total += attrib_size;
                }

                values.CopyTo(all_values, i * a4);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, s.vbo.OtherArrayBufferID);
            if ((start_index < 0 && s.vbo.OtherDataSize != a4 * count) || s.vbo.OtherDataSize == 0)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * a4 * count), all_values,
                    BufferUsageHint.StreamDraw);
                s.vbo.OtherDataSize = a4 * count;
            }
            else
            {
                int offset = start_index;
                if (offset < 0)
                {
                    offset = 0;
                }

                GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * a4 * offset),
                    new IntPtr(sizeof(float) * a4 * count), all_values);
                //GL.BufferSubData(BufferTarget.ArrayBuffer,new IntPtr(0),new IntPtr(sizeof(float) * a4 * count),all_values);
            }
        }

        public void UpdateOtherSingleVertex(Surface s, int index, int sprite_index, int sprite_type,
            params IList<float>[] vertex_attributes)
        {
            int a = s.vbo.VertexAttribs.TotalSize;
            int a4 = a * 4;
            float[] values = new float[a4];
            SpriteType sprite = s.texture.Sprite[sprite_type];
            float tex_start_x = sprite.X(sprite_index);
            float tex_start_y = sprite.Y(sprite_index);
            float tex_end_x = tex_start_x + sprite.SpriteWidth;
            float tex_end_y = tex_start_y + sprite.SpriteHeight;
            values[0] = tex_start_x; //the 4 corners, texcoords:
            values[1] = tex_end_y;
            values[a] = tex_start_x;
            values[a + 1] = tex_start_y;
            values[a * 2] = tex_end_x;
            values[a * 2 + 1] = tex_start_y;
            values[a * 3] = tex_end_x;
            values[a * 3 + 1] = tex_end_y;
            int prev_total = 2;
            for (int g = 1; g < s.vbo.VertexAttribs.Size.Length; ++g)
            {
                //starting at 1 because texcoords are already done
                int attrib_size = s.vbo.VertexAttribs.Size[g];
                for (int k = 0; k < attrib_size; ++k)
                {
                    float
                        attrib = vertex_attributes[g - 1][
                            k]; // -1 because the vertex_attributes array doesn't contain texcoords here in the update method.
                    values[prev_total + k] = attrib;
                    values[prev_total + k + a] = attrib;
                    values[prev_total + k + a * 2] = attrib;
                    values[prev_total + k + a * 3] = attrib;
                }

                prev_total += attrib_size;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, s.vbo.OtherArrayBufferID);
            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * a4 * index),
                new IntPtr(sizeof(float) * a4), values);
        }

        public void WriteString(int x, int y, string text)
        {
            throw new NotImplementedException();
        }

        public void WriteString(int x, int y, string text, Color foreground)
        {
            throw new NotImplementedException();
        }

        public void WriteString(int x, int y, string text, Color foreground, Color background)
        {
            throw new NotImplementedException();
        }
    }
}