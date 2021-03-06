using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Forays.Scenes;
using Forays.Graphics;
using Forays.Input;
using GLDrawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Key = OpenTK.Input.Key;

namespace Forays.Renderer
{
    public class OpenTk : GameWindow, IRenderer
    {
        public bool NoClose = false;
        public bool FullScreen = false;
        public Action FinalResize = null;
        public List<Surface> Surfaces = new List<Surface>();

        private FrameEventArgs render_args = new FrameEventArgs();
        private Dictionary<Key, bool> key_down = new Dictionary<Key, bool>();
        private bool Resizing = false;
        private bool DepthTestEnabled = false;
        private int LastShaderID = -1;
        private Rectangle internalViewport;

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
            // GL.Viewport maps the NDC to the window.
            GL.Viewport(x, y, width, height);
        }

        public OpenTk(int w, int h, string title) : base(w, h, GraphicsMode.Default, title)
        {
            // Set to false for applications that are not DPI-aware (e.g. WinForms.)
            ToolkitOptions.Default.EnableHighResolution = false;
            // This takes four floats, ranging between 0.0f and 1.0f. This
            // decides the color of the window after it gets cleared between
            // frames.
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            // These 2 attributes arrays are always ON, for position and text
            // coordinate.
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            VSync = VSyncMode.On;
            KeyDown += KeyDownHandler;
            KeyUp += KeyUpHandler;
            Keyboard.KeyRepeat = true;
            internalViewport = new Rectangle(0, 0, w, h);
            Icon = new Icon(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(Global.ForaysImageResources + "forays.ico"));
            FinalResize += InputKey.HandleResize;
            Closing += InputKey.OnClosing;
            KeyDown += InputKey.KeyDownHandler;
            MouseLeave += InputKey.MouseLeaveHandler;
            Mouse.Move += InputKey.MouseMoveHandler;
            Mouse.ButtonUp += InputKey.MouseClickHandler;
            Mouse.WheelChanged += InputKey.MouseWheelHandler;
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

            Resizing =
                true; //todo: changed this to simply set Resizing instead of actually calling resizing methods.
        }

        public void UpdatePositionVertexArray(Surface s, IList<int> index_list,
            IList<int> layout_list = null)
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
                new float[count * 4 *
                          s.VertexBufferObject
                              .PositionDimensions]; //2 or 3 dimensions for 4 vertices for each tile
            int[] indices = null;
            if (start_index < 0 && s.VertexBufferObject.NumElements != count * 6)
            {
                indices = new int[count * 6];
                s.VertexBufferObject.NumElements = count * 6;
            }
            else
            {
                if (start_index >= 0)
                {
                    if ((start_index + count) * 6 > s.VertexBufferObject.NumElements &&
                        s.VertexBufferObject.NumElements > 0)
                    {
                        throw new ArgumentException(
                            "Error: start_index + count is bigger than VBO size. To always replace the previous data, set start_index to -1.");
                    } //todo: I could also just ignore the start_index if there's too much data.

                    if ((start_index + count) * 4 * s.VertexBufferObject.PositionDimensions >
                        s.VertexBufferObject.PositionDataSize &&
                        s.VertexBufferObject.PositionDataSize > 0)
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
                float x_offset = (float) s.Layouts[layout_list[current_total]].HorizontalOffset;
                float y_offset = (float) s.Layouts[layout_list[current_total]].VerticalOffset;
                float x_w = (float) s.Layouts[layout_list[current_total]].Width;
                float y_h = (float) s.Layouts[layout_list[current_total]].Height;
                float cellx = s.Layouts[layout_list[current_total]].X(i) + x_offset;
                float celly = s.Layouts[layout_list[current_total]].Y(i) + y_offset;
                float x = cellx * width_ratio - 1.0f;
                float y = celly * height_ratio - 1.0f;
                float x_plus1 = (cellx + x_w) * width_ratio - 1.0f;
                float y_plus1 = (celly + y_h) * height_ratio - 1.0f;

                int N = s.VertexBufferObject.PositionDimensions;
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
                    throw new Exception("3 Dimension not is current supported.");
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

            GL.BindBuffer(BufferTarget.ArrayBuffer, s.VertexBufferObject.PositionArrayBufferId);
            if ((start_index < 0 && s.VertexBufferObject.PositionDataSize != values.Length) ||
                s.VertexBufferObject.PositionDataSize == 0)
            {
                // The fourth parameter is a BufferUsageHint, which specifies
                // how we want the graphics card to manage the given data.
                // For this case: StreamDraw: the data will change every time
                // it is drawn.
                GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * values.Length),
                    values,
                    BufferUsageHint.StreamDraw);
                s.VertexBufferObject.PositionDataSize = values.Length;
            }
            else
            {
                int offset = start_index;
                if (offset < 0)
                {
                    offset = 0;
                }

                GL.BufferSubData(BufferTarget.ArrayBuffer,
                    new IntPtr(sizeof(float) * 4 * s.VertexBufferObject.PositionDimensions *
                               offset),
                    new IntPtr(sizeof(float) * values.Length), values);
                //GL.BufferSubData(BufferTarget.ArrayBuffer,new IntPtr(0),new IntPtr(sizeof(float) * values.Length),values);
            }

            if (indices != null)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer,
                    s.VertexBufferObject.ElementArrayBufferId);
                // The fourth parameter is a BufferUsageHint, which specifies
                // how we want the graphics card to manage the given data.
                // For this case: StaticDraw: the data will most likely not
                // change at all or very rarely.
                GL.BufferData(BufferTarget.ElementArrayBuffer,
                    new IntPtr(sizeof(int) * indices.Length), indices,
                    BufferUsageHint.StaticDraw);
            }
        }

        public void UpdateOtherVertexArray(Surface s, int start_index, IList<int> sprite_index,
            IList<int> sprite_type,
            params IList<float>[] vertex_attributes)
        {
            int count = sprite_index.Count;
            int a = s.VertexBufferObject.VertexAttributes.TotalSize;
            int a4 = a * 4;
            if (start_index >= 0 &&
                (start_index + count) * a4 > s.VertexBufferObject.OtherDataSize &&
                s.VertexBufferObject.OtherDataSize > 0)
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
                SpriteType sprite = s.Texture.Sprite[sprite_type[i]];
                float tex_start_x = sprite.X(sprite_index[i]);
                float tex_start_y = sprite.Y(sprite_index[i]);
                float tex_end_x = tex_start_x + sprite.SpriteWidth;
                float tex_end_y = tex_start_y + SpriteType.SpriteHeight;
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
                for (int g = 1; g < s.VertexBufferObject.VertexAttributes.Size.Length; ++g)
                {
                    //starting at 1 because texcoords are already done
                    int attrib_size = s.VertexBufferObject.VertexAttributes.Size[g];
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

            GL.BindBuffer(BufferTarget.ArrayBuffer, s.VertexBufferObject.OtherArrayBufferId);
            if ((start_index < 0 && s.VertexBufferObject.OtherDataSize != a4 * count) ||
                s.VertexBufferObject.OtherDataSize == 0)
            {
                // The fourth parameter is a BufferUsageHint, which specifies
                // how we want the graphics card to manage the given data.
                // For this case: StreamDraw: the data will change every time
                // it is drawn.
                GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * a4 * count),
                    all_values,
                    BufferUsageHint.StreamDraw);
                s.VertexBufferObject.OtherDataSize = a4 * count;
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

        public void UpdateOtherSingleVertex(Surface surface, int index, int spriteIndex,
            int spriteType, params IList<float>[] vertexAttributes)
        {
            int a = surface.VertexBufferObject.VertexAttributes.TotalSize;
            int a4 = a * 4;
            float[] values = new float[a4];
            SpriteType sprite = surface.Texture.Sprite[spriteType];
            float tex_start_x = sprite.X(spriteIndex);
            float tex_start_y = sprite.Y(spriteIndex);
            float tex_end_x = tex_start_x + sprite.SpriteWidth;
            float tex_end_y = tex_start_y + SpriteType.SpriteHeight;
            values[0] = tex_start_x; //the 4 corners, texcoords:
            values[1] = tex_end_y;
            values[a] = tex_start_x;
            values[a + 1] = tex_start_y;
            values[a * 2] = tex_end_x;
            values[a * 2 + 1] = tex_start_y;
            values[a * 3] = tex_end_x;
            values[a * 3 + 1] = tex_end_y;
            int prev_total = 2;
            for (int g = 1; g < surface.VertexBufferObject.VertexAttributes.Size.Length; ++g)
            {
                //starting at 1 because texcoords are already done
                int attrib_size = surface.VertexBufferObject.VertexAttributes.Size[g];
                for (int k = 0; k < attrib_size; ++k)
                {
                    float
                        attrib = vertexAttributes[g - 1][
                            k]; // -1 because the vertex_attributes array doesn't contain texcoords here in the update method.
                    values[prev_total + k] = attrib;
                    values[prev_total + k + a] = attrib;
                    values[prev_total + k + a * 2] = attrib;
                    values[prev_total + k + a * 3] = attrib;
                }

                prev_total += attrib_size;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, surface.VertexBufferObject.OtherArrayBufferId);
            GL.BufferSubData(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * a4 * index),
                new IntPtr(sizeof(float) * a4), values);
        }

        public bool IsRunning()
        {
            return !IsExiting;
        }

        public void Draw()
        {
            base.OnRenderFrame(render_args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (var surface in Surfaces.Where(surface => !surface.Disabled))
            {
                if (DepthTestEnabled != surface.UseDepthBuffer)
                {
                    if (surface.UseDepthBuffer)
                    {
                        GL.Enable(EnableCap.DepthTest);
                    }
                    else
                    {
                        GL.Disable(EnableCap.DepthTest);
                    }

                    DepthTestEnabled = surface.UseDepthBuffer;
                }

                if (LastShaderID != surface.Shader.ShaderProgramID)
                {
                    GL.UseProgram(surface.Shader.ShaderProgramID);
                    LastShaderID = surface.Shader.ShaderProgramID;
                }

                GL.Uniform2(surface.Shader.OffsetUniformLocation, surface.RawXOffset,
                    surface.RawYOffset);
                GL.Uniform1(surface.Shader.TextureUniformLocation, surface.Texture.TextureIndex);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer,
                    surface.VertexBufferObject.ElementArrayBufferId);
                GL.BindBuffer(BufferTarget.ArrayBuffer,
                    surface.VertexBufferObject.PositionArrayBufferId);
                GL.VertexAttribPointer(0, surface.VertexBufferObject.PositionDimensions,
                    VertexAttribPointerType.Float,
                    false,
                    sizeof(float) * surface.VertexBufferObject.PositionDimensions,
                    new IntPtr(0)); //position
                GL.BindBuffer(BufferTarget.ArrayBuffer,
                    surface.VertexBufferObject.OtherArrayBufferId);
                int stride = sizeof(float) * surface.VertexBufferObject.VertexAttributes.TotalSize;
                GL.VertexAttribPointer(1, surface.VertexBufferObject.VertexAttributes.Size[0],
                    VertexAttribPointerType.Float, false, stride,
                    new IntPtr(0)); //texcoords
                int totalOfPreviousAttribs = surface.VertexBufferObject.VertexAttributes.Size[0];
                for (int i = 1; i < surface.VertexBufferObject.VertexAttributes.Size.Length; ++i)
                {
                    GL.EnableVertexAttribArray(
                        i + 1); //i+1 because 0 and 1 are always on (for position & texcoords)
                    GL.VertexAttribPointer(i + 1,
                        surface.VertexBufferObject.VertexAttributes.Size[i],
                        VertexAttribPointerType.Float, false,
                        stride, new IntPtr(sizeof(float) * totalOfPreviousAttribs));
                    totalOfPreviousAttribs += surface.VertexBufferObject.VertexAttributes.Size[i];
                }

                GL.DrawElements(PrimitiveType.Triangles, surface.VertexBufferObject.NumElements,
                    DrawElementsType.UnsignedInt,
                    IntPtr.Zero);
                for (int i = 1; i < surface.VertexBufferObject.VertexAttributes.Size.Length; ++i)
                {
                    GL.DisableVertexAttribArray(i + 1);
                }
            }

            // Almost any modern OpenGL context is what's known as "double-buffered".
            // Double-buffering means that there are two areas that OpenGL draws
            // to. In essence: One area is displayed, while the other is being
            // rendered to. Then, when you call SwapBuffers, the two are reversed.
            // A single-buffered context could have issues such as screen tearing.
            SwapBuffers();
        }

        /// <summary>
        /// Clears the screen, using the color set in Gl.ClearColor function.
        /// This should always be the first function called when rendering.
        /// </summary>
        public void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
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

        public NextScene ProcessInput()
        {
            ProcessEvents();

            if (Resizing)
            {
                //HandleResize();
                FinalResize?.Invoke();
                Resizing = false;
            }

            return NextScene.None;
        }

        public Input.Key GetKeyPressed()
        {
            Input.Key keyPressed = new Input.Key();
            
            ProcessEvents();

            KeyboardState input = OpenTK.Input.Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                keyPressed.SetKeyCode(KeyCode.Escape);
                
                Console.Write("Escape.");
            }
            else if (input.IsKeyDown(Key.Space))
            {
                keyPressed.SetKeyCode(KeyCode.Space);
                
                Console.Write("Space.");
            }

            return keyPressed;
        }
    }
}