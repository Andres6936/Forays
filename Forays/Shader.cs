using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Forays
{
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