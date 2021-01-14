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
using Forays.Renderer;
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


    public class Surface
    {
        public OpenTk window;
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

        public static Surface Create(OpenTk window_, string texture_filename, params int[] vertex_attrib_counts)
        {
            return Create(window_, texture_filename, false, Shader.DefaultFS(), false, vertex_attrib_counts);
        }

        public static Surface Create(OpenTk window_, string texture_filename,
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