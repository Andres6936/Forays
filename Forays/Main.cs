/*Copyright (c) 2011-2016  Derrick Creamer
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Forays.Renderer;
using Forays.Scenes;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GLDrawing;

namespace Forays
{
    public class Game
    {
        static void Main(string[] args)
        {
#if CONSOLE
            Screen.GLMode = false;
#endif
            // Identifies the operating system, or platform, supported by an
            // assembly.
            PlatformID operatingSystem = Environment.OSVersion.Platform;

            // If the operating system is Unix.
            // If the operating system is Macintosh.
            // Note: This value was returned by Silverlight. On .NET Core, its
            // replacement is Unix.
            if (operatingSystem == PlatformID.Unix || operatingSystem == PlatformID.MacOSX)
            {
                Global.LINUX = true;
            }

            if (args != null && args.Length > 0)
            {
                if (args[0] == "-c" || args[0] == "--console")
                {
                    Screen.GLMode = false;
                }

                if (args[0] == "-g" || args[0] == "--gl")
                {
                    Screen.GLMode = true;
                }
            }

            if (!Screen.GLMode)
            {
                if (Global.LINUX)
                {
                    Screen.CursorVisible = false;
                    Screen.SetCursorPosition(0,
                        0); //todo: this should still work fine but it's worth a verification.
                    if (Console.BufferWidth < Global.SCREEN_W ||
                        Console.BufferHeight < Global.SCREEN_H)
                    {
                        Console.Write("Please resize your terminal to {0}x{1}, then press any key.",
                            Global.SCREEN_W,
                            Global.SCREEN_H);
                        Screen.SetCursorPosition(0, 1);
                        Console.Write("         Current dimensions are {0}x{1}.".PadRight(57),
                            Console.BufferWidth,
                            Console.BufferHeight);
                        InputKey.ReadKey(false);
                        Screen.SetCursorPosition(0, 0);
                        if (Console.BufferWidth < Global.SCREEN_W ||
                            Console.BufferHeight < Global.SCREEN_H)
                        {
                            Screen.CursorVisible = true;
                            Environment.Exit(0);
                        }
                    }

                    Screen.Blank();
                    Console.TreatControlCAsInput = true;
                }
                else
                {
                    if (Type.GetType("Mono.Runtime") != null)
                    {
                        // If you try to resize the Windows Command Prompt using Mono, it crashes, so just switch
                        Screen.GLMode =
                            true; // back to GL mode in that case. (Fortunately, nobody uses Mono on Windows unless they're compiling a project in MD/XS.)
                    }
                    else
                    {
                        Screen.CursorVisible = false;
                        Console.Title = "Forays into Norrendrin";
                        Console.BufferHeight = Global.SCREEN_H;
                        Console.SetWindowSize(Global.SCREEN_W, Global.SCREEN_H);
                        Console.TreatControlCAsInput = true;
                    }
                }
            }

            if (Screen.GLMode)
            {
                ToolkitOptions.Default.EnableHighResolution = false;
                int height_px = Global.SCREEN_H * 16;
                int width_px = Global.SCREEN_W * 8;
                Screen.gl = new OpenTk(width_px, height_px, "Forays into Norrendrin");

                Screen.textSurface = Surface.Create(Screen.gl, "font8x16.png", true,
                    Shader.AAFontFS(), false, 2, 4, 4);
                SpriteType.DefineSingleRowSprite(Screen.textSurface, 8, 1);
                Screen.textSurface.Layouts.Add(new CellLayout(Global.SCREEN_W, 16, 8));
                Screen.textSurface.SetEasyLayoutCounts(Global.SCREEN_H * Global.SCREEN_W);
                Screen.textSurface.DefaultUpdatePositions();
                Screen.textSurface.SetDefaultSpriteType(0);
                Screen.textSurface.SetDefaultSprite(32); //space
                Screen.textSurface.SetDefaultOtherData(
                    new List<float>(Color.Gray.GetFloatValues()),
                    new List<float>(Color.Black.GetFloatValues()));
                Screen.textSurface.DefaultUpdateOtherData();

                Screen.cursorSurface = Surface.Create(Screen.gl, "font8x16.png", true,
                    Shader.AAFontFS(), false, 2, 4, 4);
                Screen.cursorSurface.Texture = Screen.textSurface.Texture;
                Screen.cursorSurface.Layouts.Add(new CellLayout(1, 2, 8));
                Screen.cursorSurface.SetEasyLayoutCounts(1);
                Screen.cursorSurface.DefaultUpdatePositions();
                Screen.cursorSurface.SetDefaultSpriteType(0);
                Screen.cursorSurface.SetDefaultSprite(32);
                Screen.cursorSurface.SetDefaultOtherData(
                    new List<float>(Color.Black.GetFloatValues()),
                    new List<float>(Color.Gray.GetFloatValues()));
                Screen.cursorSurface.DefaultUpdateOtherData();

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                Screen.gl.Visible = true;
                Global.Timer = new Stopwatch();
                Global.Timer.Start();
                Screen.CursorVisible = false;
            }

            Nym.Verbs.Register("feel",
                "looks"); //Useful for generating messages like "You feel stronger" / "The foo looks stronger".
            InputKey.LoadKeyRebindings();

            var manager = new SceneManager();

            while (manager.IsRunning())
            {
                manager.Clear();
                manager.Draw();
                manager.ProcessInput();
            }
        }
    }
}