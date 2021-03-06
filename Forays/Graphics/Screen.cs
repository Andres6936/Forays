/*Copyright (c) 2011-2015  Derrick Creamer
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
using System.Threading;
using Forays.Renderer;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Utilities;
using PosArrays;
using GLDrawing;

namespace Forays
{
    public static class Screen
    {
        // Properties

        private static ColorChar[,] memory;
        private static bool terminal_bold; //for linux terminals

        /// <summary>
        /// If is true, UpdateGLBuffer won't be called - only the memory will be
        /// updated. This is useful if you wish to update all at once, instead
        /// of one at a time.
        /// </summary>
        public static bool NoGLUpdate = false;

        public static int screen_center_col = -1;

        private static bool
            cursor_visible =
                true; //these 3 values are only used in GL mode - in console mode, the Console values are used directly.

        private static int cursor_top;
        private static int cursor_left ;
        public static OpenTk gl ;
        public static Surface textSurface ;
        public static Surface cursorSurface ;
        public static int cellHeight = 16;
        public static int cellWidth = 8;
        public static string currentFont = Global.ForaysImageResources + "font8x16.png";

        public static bool NoClose
        {
            get
            {
                if (gl != null)
                {
                    return gl.NoClose;
                }

                return false;
            }
            set
            {
                if (gl != null)
                {
                    gl.NoClose = value;
                }
            }
        }

        public static bool CursorVisible
        {
            get { return cursor_visible; }
            set
            {
                if (cursor_visible != value)
                {
                    cursor_visible = value;
                    UpdateCursor(value);
                }
            }
        }

        public static int CursorTop
        {
            get { return cursor_top; }
            set
            {
                if (cursor_top != value)
                {
                    cursor_top = value;
                    UpdateCursor(cursor_visible);
                }
            }
        }

        public static int CursorLeft
        {
            get { return cursor_left; }
            set
            {
                if (cursor_left != value)
                {
                    cursor_left = value;
                    UpdateCursor(cursor_visible);
                }
            }
        }

        // Static Constructor

        /// <summary>
        /// For set the mode console is needed added the macro CONSOLE.
        /// </summary>
        static Screen()
        {
            memory = new ColorChar[Global.SCREEN_H, Global.SCREEN_W];
            for (int i = 0; i < Global.SCREEN_H; ++i)
            {
                for (int j = 0; j < Global.SCREEN_W; ++j)
                {
                    memory[i, j].c = ' ';
                    memory[i, j].color = Color.Black;
                    memory[i, j].bgcolor = Color.Black;
                }
            }

            int height_px = Global.SCREEN_H * 16;
            int width_px = Global.SCREEN_W * 8;
            gl = new OpenTk(width_px, height_px, "Forays into Norrendrin");

            textSurface = Surface.Create(gl, "font8x16.png", true,
                Shader.AAFontFS(), false, 2, 4, 4);
            textSurface.Texture.Sprite.Add(new SpriteType(8, 1,
                textSurface.Texture.TextureWidthPx));
            textSurface.Layouts.Add(new CellLayout(Global.SCREEN_W, 16, 8));
            textSurface.SetEasyLayoutCounts(Global.SCREEN_H * Global.SCREEN_W);
            textSurface.DefaultUpdatePositions();
            textSurface.SetDefaultSpriteType(0);
            textSurface.SetDefaultSprite(32); //space
            textSurface.SetDefaultOtherData(
                new List<float>(Color.Gray.GetFloatValues()),
                new List<float>(Color.Black.GetFloatValues()));
            textSurface.DefaultUpdateOtherData();

            cursorSurface = Surface.Create(gl, "font8x16.png", true,
                Shader.AAFontFS(), false, 2, 4, 4);
            cursorSurface.Texture = textSurface.Texture;
            cursorSurface.Layouts.Add(new CellLayout(1, 2, 8));
            cursorSurface.SetEasyLayoutCounts(1);
            cursorSurface.DefaultUpdatePositions();
            cursorSurface.SetDefaultSpriteType(0);
            cursorSurface.SetDefaultSprite(32);
            cursorSurface.SetDefaultOtherData(
                new List<float>(Color.Black.GetFloatValues()),
                new List<float>(Color.Gray.GetFloatValues()));
            cursorSurface.DefaultUpdateOtherData();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            gl.Visible = true;
            Global.Timer = new Stopwatch();
            Global.Timer.Start();
            CursorVisible = false;

            Nym.Verbs.Register("feel",
                "looks"); //Useful for generating messages like "You feel stronger" / "The foo looks stronger".
            InputKey.LoadKeyRebindings();
        }

        // Methods

        public static bool GLUpdate()
        {
            gl.ProcessInput();
            gl.Draw();
            return gl.IsRunning();
        }

        public static void SetCursorPosition(int left, int top)
        {
            if (cursor_left != left || cursor_top != top)
            {
                cursor_left = left;
                cursor_top = top;
                UpdateCursor(cursor_visible);
            }
        }

        public static void UpdateScreenCenterColumn(int col)
        {
            //this is the alternative to "always centered" behavior.
            if (col < screen_center_col - 3)
            {
                //todo
                screen_center_col = col - 3;
            }

            if (col > screen_center_col + 3)
            {
                screen_center_col = col + 3;
            }
        }

        public static ColorChar Char(int r, int c)
        {
            return memory[r, c];
        }

        public static ColorChar MapChar(int r, int c)
        {
            return memory[r + Global.MAP_OFFSET_ROWS, c + Global.MAP_OFFSET_COLS];
        }

        public static ColorChar BlankChar()
        {
            return new ColorChar(Color.Black, ' ');
        }

        public static ColorChar[,] GetCurrentScreen()
        {
            ColorChar[,] result = new ColorChar[Global.SCREEN_H, Global.SCREEN_W];
            for (int i = 0; i < Global.SCREEN_H; ++i)
            {
                for (int j = 0; j < Global.SCREEN_W; ++j)
                {
                    result[i, j] = Char(i, j);
                }
            }

            return result;
        }

        public static ColorChar[,] GetCurrentMap()
        {
            ColorChar[,] result = new ColorChar[Global.ROWS, Global.COLS];
            for (int i = 0; i < Global.ROWS; ++i)
            {
                for (int j = 0; j < Global.COLS; ++j)
                {
                    result[i, j] = MapChar(i, j);
                }
            }

            return result;
        }

        public static ColorChar[,] GetCurrentRect(int row, int col, int height, int width)
        {
            ColorChar[,] result = new ColorChar[height, width];
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    result[i, j] = Char(row + i, col + j);
                }
            }

            return result;
        }

        public static bool MapBoundsCheck(int r, int c)
        {
            if (r >= 0 && r < Global.ROWS && c >= 0 && c < Global.COLS)
            {
                return true;
            }

            return false;
        }

        public static void Blank()
        {
            CursorVisible = false;
            for (int i = 0; i < Global.SCREEN_H; ++i)
            {
                WriteString(i, 0, "".PadRight(Global.SCREEN_W));
                for (int j = 0; j < Global.SCREEN_W; ++j)
                {
                    memory[i, j].c = ' ';
                    memory[i, j].color = Color.Black;
                    memory[i, j].bgcolor = Color.Black;
                }
            }
        }

        /// <summary>
        /// Update the OpenGL Buffer.
        /// </summary>
        /// <param name="startRow">Start coordinate in the axis X.</param>
        /// <param name="startCol">Start coordinate in the axis Y.</param>
        /// <param name="endRow">End coordinate in the axis X.</param>
        /// <param name="endCol">End coordinate in the axus Y.</param>
        public static void UpdateGlBuffer(int startRow, int startCol, int endRow, int endCol)
        {
            int numPositions = ((endCol + endRow * Global.SCREEN_W) -
                                (startCol + startRow * Global.SCREEN_W)) + 1;
            int row = startRow;
            int col = startCol;
            int[] spriteRows = new int[numPositions];
            int[] spriteCols = new int[numPositions];
            float[][] colorInfo = new float[2][];
            colorInfo[0] = new float[4 * numPositions];
            colorInfo[1] = new float[4 * numPositions];
            for (int i = 0; i < numPositions; ++i)
            {
                ColorChar cch = memory[row, col];
                Color4 foregroundColor = Colors.ConvertColor(cch.color);
                Color4 backgroundColor = Colors.ConvertColor(cch.bgcolor);
                spriteRows[i] = 0;
                spriteCols[i] = (int) cch.c;
                int idx4 = i * 4;
                colorInfo[0][idx4] = foregroundColor.R;
                colorInfo[0][idx4 + 1] = foregroundColor.G;
                colorInfo[0][idx4 + 2] = foregroundColor.B;
                colorInfo[0][idx4 + 3] = foregroundColor.A;
                colorInfo[1][idx4] = backgroundColor.R;
                colorInfo[1][idx4 + 1] = backgroundColor.G;
                colorInfo[1][idx4 + 2] = backgroundColor.B;
                colorInfo[1][idx4 + 3] = backgroundColor.A;
                col++;
                if (col == Global.SCREEN_W)
                {
                    row++;
                    col = 0;
                }
            }

            //int idx = (start_col + start_row*Global.SCREEN_W) * 48;
            //GL.BufferSubData(BufferTarget.ArrayBuffer,new IntPtr(sizeof(float)*idx),new IntPtr(sizeof(float)*48*num_positions),values.ToArray());
            gl.UpdateOtherVertexArray(textSurface,
                U.Get1DIndex(startRow, startCol, Global.SCREEN_W), spriteCols,
                null, colorInfo);
            //Game.gl.UpdateVertexArray(start_row,start_col,GLGame.text_surface,sprite_rows,sprite_cols,color_info);
        }

        private static void UpdateGLBuffer(int row, int col)
        {
            ColorChar cch = memory[row, col];
            gl.UpdateOtherSingleVertex(textSurface, U.Get1DIndex(row, col, Global.SCREEN_W),
                (int) cch.c, 0,
                cch.color.GetFloatValues(), cch.bgcolor.GetFloatValues());
        }

        public static void UpdateCursor(bool make_visible)
        {
            if (make_visible && (!Global.GRAPHICAL || MouseUI.Mode != MouseMode.Map))
            {
                //todo: this line probably breaks in graphical mode sometimes.
                cursorSurface.Disabled = false;
                cursorSurface.SetOffsetInPixels(cursor_left * cellWidth,
                    cursor_top * cellHeight + cellHeight * 7 / 8);
            }
            else
            {
                cursorSurface.Disabled = true;
            }
        }

        public static void UpdateGLBuffer(int start_row, int start_col, ColorChar[,] array)
        {
            int array_h = array.GetLength(0);
            int array_w = array.GetLength(1);
            int start_idx = start_col + start_row * Global.SCREEN_W;
            int end_idx = (start_col + array_w - 1) + (start_row + array_h - 1) * Global.SCREEN_W;
            int count = (end_idx - start_idx) + 1;
            int end_row = start_row + array_h - 1;
            int end_col = start_col + array_w - 1;
            //int[] sprite_rows = new int[count];
            int[] sprite_cols = new int[count];
            float[][] color_info = new float[2][];
            color_info[0] = new float[4 * count];
            color_info[1] = new float[4 * count];
            for (int n = 0; n < count; ++n)
            {
                int row = (n + start_col) / Global.SCREEN_W + start_row; //screen coords
                int col = (n + start_col) % Global.SCREEN_W;
                ColorChar cch =
                    (row >= start_row && row <= end_row && col >= start_col && col <= end_col)
                        ? array[row - start_row, col - start_col]
                        : memory[row, col];
                Color4 color = Colors.ConvertColor(cch.color);
                Color4 bgcolor = Colors.ConvertColor(cch.bgcolor);
                //sprite_rows[n] = 0;
                sprite_cols[n] = (int) cch.c;
                int idx4 = n * 4;
                color_info[0][idx4] = color.R;
                color_info[0][idx4 + 1] = color.G;
                color_info[0][idx4 + 2] = color.B;
                color_info[0][idx4 + 3] = color.A;
                color_info[1][idx4] = bgcolor.R;
                color_info[1][idx4 + 1] = bgcolor.G;
                color_info[1][idx4 + 2] = bgcolor.B;
                color_info[1][idx4 + 3] = bgcolor.A;
            }

            gl.UpdateOtherVertexArray(textSurface, start_idx, sprite_cols, null, color_info);
            //Game.gl.UpdateVertexArray(start_row,start_col,GLGame.text_surface,sprite_rows,sprite_cols,color_info);
        }
        
        public static void WriteChar(int r, int c, char ch)
        {
            WriteChar(r, c, new ColorChar(Color.Gray, ch));
        }

        public static void WriteChar(int r, int c, char ch, Color color)
        {
            WriteChar(r, c, new ColorChar(ch, color));
        }

        public static void WriteChar(int r, int c, char ch, Color color, Color bgcolor)
        {
            WriteChar(r, c, new ColorChar(ch, color, bgcolor));
        }

        public static void WriteChar(int r, int c, ColorChar ch)
        {
            // If the char to write is same one that exists in that same
            // position, then there will be no need to update.
            if (memory[r, c].Equals(ch)) return;

            ch.color = Colors.ResolveColor(ch.color);
            ch.bgcolor = Colors.ResolveColor(ch.bgcolor);
            memory[r, c] = ch;
            if (!NoGLUpdate)
            {
                UpdateGLBuffer(r, c);
            }
        }

        public static void WriteArray(int r, int c, ColorChar[,] array)
        {
            int h = array.GetLength(0);
            int w = array.GetLength(1);
            for (int i = 0; i < h; ++i)
            {
                for (int j = 0; j < w; ++j)
                {
                    //WriteChar(i+r,j+c,array[i,j]);
                    ColorChar ch = array[i, j];
                    if (!memory[r + i, c + j].Equals(ch))
                    {
                        ch.color = Colors.ResolveColor(ch.color);
                        ch.bgcolor = Colors.ResolveColor(ch.bgcolor);
                        //memory[r+i,c+j] = ch;
                        array[i, j] = ch;
                        memory[r + i, c + j] = ch;
                    }
                }
            }

            if (!NoGLUpdate)
            {
                UpdateGLBuffer(r, c, array);
            }
        }

        public static void WriteList(int r, int c, List<ColorBufferString> ls)
        {
            int line = r;
            foreach (ColorBufferString cs in ls)
            {
                WriteString(line, c, cs);
                ++line;
            }
        }

        public static void WriteString(int r, int c, string s)
        {
            WriteString(r, c, new ColorString(s, Color.Gray));
        }

        public static void WriteString(int r, int c, string s, Color color)
        {
            WriteString(r, c, new ColorString(s, color));
        }

        public static void WriteString(int r, int c, string s, Color color, Color bgcolor)
        {
            WriteString(r, c, new ColorString(s, color, bgcolor));
        }

        public static void WriteString(int r, int c, ColorString text)
        {
            if (Global.SCREEN_W - c > text.Length)
            {
                //s.s = s.s.Substring(0,; //don't move down to the next line
            }
            else
            {
                text.Text = text.Text.Substring(0, Global.SCREEN_W - c);
            }

            if (text.Length <= 0) return;

            text.Foreground = Colors.ResolveColor(text.Foreground);
            text.Background = Colors.ResolveColor(text.Background);
            ColorChar cch;
            cch.color = text.Foreground;
            cch.bgcolor = text.Background;

            int start_col = -1;
            int end_col = -1;
            int i = 0;
            bool changed = false;
            foreach (char ch in text.Text)
            {
                cch.c = ch;
                if (!memory[r, c + i].Equals(cch))
                {
                    memory[r, c + i] = cch;
                    if (start_col == -1)
                    {
                        start_col = c + i;
                    }

                    end_col = c + i;
                    changed = true;
                }

                ++i;
            }

            if (changed)
            {
                if (!NoGLUpdate)
                {
                    UpdateGlBuffer(r, start_col, r, end_col);
                }
            }

            if (MouseUI.AutomaticButtonsFromStrings)
            {
                int idx = 0;
                int brace = -1;
                int start = -1;
                int end = -1;
                bool last_char_was_separator = false;
                while (true)
                {
                    if (brace == -1)
                    {
                        if (text.Text[idx] == '[')
                        {
                            brace = 0;
                            start = idx;
                        }
                    }
                    else
                    {
                        if (brace == 0)
                        {
                            if (text[idx] == ']')
                            {
                                brace = 1;
                                end = idx;
                            }
                        }
                        else
                        {
                            if (text[idx] == ' ' || text[idx] == '-' || text[idx] == ',')
                            {
                                if (last_char_was_separator)
                                {
                                    ConsoleKey key = ConsoleKey.A;
                                    bool shifted = false;
                                    switch (text[start + 1])
                                    {
                                        case 'E':
                                            key = ConsoleKey.Enter;
                                            break;
                                        case 'T':
                                            key = ConsoleKey.Tab;
                                            break;
                                        case 'P': //"Press any key"
                                            break;
                                        case '?':
                                            key = ConsoleKey.Oem2;
                                            shifted = true;
                                            break;
                                        case '=':
                                            key = ConsoleKey.OemPlus;
                                            break;
                                        default: //all others should be lowercase letters
                                            key = (ConsoleKey.A +
                                                   (text[start + 1] - 'a'));
                                            break;
                                    }

                                    MouseUI.CreateButton(key, shifted, r, c + start, 1,
                                        end - start + 1);
                                    brace = -1;
                                    start = -1;
                                    end = -1;
                                }

                                last_char_was_separator = !last_char_was_separator;
                            }
                            else
                            {
                                last_char_was_separator = false;
                                end = idx;
                            }
                        }
                    }

                    ++idx;
                    if (idx == text.Length)
                    {
                        if (brace == 1)
                        {
                            ConsoleKey key = ConsoleKey.A;
                            bool shifted = false;
                            switch (text[start + 1])
                            {
                                case 'E':
                                    key = ConsoleKey.Enter;
                                    break;
                                case 'T':
                                    key = ConsoleKey.Tab;
                                    break;
                                case 'P': //"Press any key"
                                    break;
                                case '?':
                                    key = ConsoleKey.Oem2;
                                    shifted = true;
                                    break;
                                case '=':
                                    key = ConsoleKey.OemPlus;
                                    break;
                                default: //all others should be lowercase letters
                                    key = (ConsoleKey.A + (text[start + 1] - 'a'));
                                    break;
                            }

                            MouseUI.CreateButton(key, shifted, r, c + start, 1, end - start + 1);
                        }

                        break;
                    }
                }
            }
        }

        public static void WriteString(int r, int c, ColorBufferString cs)
        {
            if (cs.Length() > 0)
            {
                int pos = c;
                int start_col = -1;
                int end_col = -1;
                foreach (ColorString s1 in cs.strings)
                {
                    ColorString s = new ColorString(s1.Text, s1.Foreground, s1.Background);
                    if (s.Text.Length + pos > Global.SCREEN_W)
                    {
                        s.Text = s.Text.Substring(0, Global.SCREEN_W - pos);
                    }

                    s.Foreground = Colors.ResolveColor(s.Foreground);
                    s.Background = Colors.ResolveColor(s.Background);
                    ColorChar cch;
                    cch.color = s.Foreground;
                    cch.bgcolor = s.Background;

                    int i = 0;
                    
                    foreach (char ch in s.Text)
                    {
                        cch.c = ch;
                        if (!memory[r, pos + i].Equals(cch))
                        {
                            memory[r, pos + i] = cch;
                            if (start_col == -1)
                            {
                                start_col = pos + i;
                            }

                            end_col = pos + i;
                            
                        }

                        ++i;
                    }

                    pos += s.Text.Length;
                }

                if ( !NoGLUpdate && start_col != -1)
                {
                    UpdateGlBuffer(r, start_col, r, end_col);
                }

                if (MouseUI.AutomaticButtonsFromStrings)
                {
                    int idx = 0;
                    int brace = -1;
                    int start = -1;
                    int end = -1;
                    bool last_char_was_separator = false;
                    while (true)
                    {
                        char ch = cs[idx].c;
                        if (brace == -1)
                        {
                            if (ch == '[')
                            {
                                brace = 0;
                                start = idx;
                            }
                        }
                        else
                        {
                            if (brace == 0)
                            {
                                if (ch == ']')
                                {
                                    brace = 1;
                                    end = idx;
                                }
                            }
                            else
                            {
                                if (ch == ' ' || ch == '-' || ch == ',')
                                {
                                    if (last_char_was_separator)
                                    {
                                        ConsoleKey key = ConsoleKey.A;
                                        bool shifted = false;
                                        switch (cs[start + 1].c)
                                        {
                                            case 'E':
                                                key = ConsoleKey.Enter;
                                                break;
                                            case 'T':
                                                key = ConsoleKey.Tab;
                                                break;
                                            case 'P': //"Press any key"
                                                break;
                                            case '?':
                                                key = ConsoleKey.Oem2;
                                                shifted = true;
                                                break;
                                            case '=':
                                                key = ConsoleKey.OemPlus;
                                                break;
                                            default: //all others should be lowercase letters
                                                key = (ConsoleKey) (ConsoleKey.A +
                                                                    ((int) cs[start + 1].c -
                                                                     (int) 'a'));
                                                break;
                                        }

                                        MouseUI.CreateButton(key, shifted, r, c + start, 1,
                                            end - start + 1);
                                        brace = -1;
                                        start = -1;
                                        end = -1;
                                    }

                                    last_char_was_separator = !last_char_was_separator;
                                }
                                else
                                {
                                    last_char_was_separator = false;
                                    end = idx;
                                }
                            }
                        }

                        ++idx;
                        if (idx == cs.Length())
                        {
                            if (brace == 1)
                            {
                                ConsoleKey key = ConsoleKey.A;
                                bool shifted = false;
                                switch (cs[start + 1].c)
                                {
                                    case 'E':
                                        key = ConsoleKey.Enter;
                                        break;
                                    case 'T':
                                        key = ConsoleKey.Tab;
                                        break;
                                    case 'P': //"Press any key"
                                        break;
                                    case '?':
                                        key = ConsoleKey.Oem2;
                                        shifted = true;
                                        break;
                                    case '=':
                                        key = ConsoleKey.OemPlus;
                                        break;
                                    default: //all others should be lowercase letters
                                        key = (ConsoleKey) (ConsoleKey.A +
                                                            ((int) cs[start + 1].c - (int) 'a'));
                                        break;
                                }

                                MouseUI.CreateButton(key, shifted, r, c + start, 1,
                                    end - start + 1);
                            }

                            break;
                        }
                    }
                }
            }
        }

        public static void WriteMapChar(int r, int c, char ch)
        {
            WriteMapChar(r, c, new ColorChar(Color.Gray, ch));
        }

        public static void WriteMapChar(int r, int c, char ch, Color color)
        {
            WriteMapChar(r, c, new ColorChar(ch, color));
        }

        public static void WriteMapChar(int r, int c, char ch, Color color, Color bgcolor)
        {
            WriteMapChar(r, c, new ColorChar(ch, color, bgcolor));
        }

        public static void WriteMapChar(int r, int c, ColorChar ch)
        {
            WriteChar(r + Global.MAP_OFFSET_ROWS, c + Global.MAP_OFFSET_COLS, ch);
        }

        public static void WriteMapString(int r, int c, string s)
        {
            ColorString cs;
            cs.Foreground = Color.Gray;
            cs.Background = Color.Black;
            cs.Text = s;
            WriteMapString(r, c, cs);
        }

        public static void WriteMapString(int r, int c, string s, Color color)
        {
            ColorString cs;
            cs.Foreground = color;
            cs.Background = Color.Black;
            cs.Text = s;
            WriteMapString(r, c, cs);
        }

        public static void WriteMapString(int r, int c, ColorString s)
        {
            if (Global.COLS - c > s.Text.Length)
            {
                //s.s = s.s.Substring(0); //don't move down to the next line
            }
            else
            {
                s.Text = s.Text.Substring(0, Global.COLS - c);
            }

            if (s.Text.Length > 0)
            {
                r += Global.MAP_OFFSET_ROWS;
                c += Global.MAP_OFFSET_COLS;
                s.Foreground = Colors.ResolveColor(s.Foreground);
                s.Background = Colors.ResolveColor(s.Background);
                ColorChar cch;
                cch.color = s.Foreground;
                cch.bgcolor = s.Background;
                

                int start_col = -1;
                int end_col = -1;
                int i = 0;
                bool changed = false;
                foreach (char ch in s.Text)
                {
                    cch.c = ch;
                    if (!memory[r, c + i].Equals(cch))
                    {
                        memory[r, c + i] = cch;
                        if (start_col == -1)
                        {
                            start_col = c + i;
                        }

                        end_col = c + i;
                        changed = true;
                    }

                    ++i;
                }

                if (changed)
                {
                    if (!NoGLUpdate)
                    {
                        UpdateGlBuffer(r, start_col, r, end_col);
                    }
                }

                if (MouseUI.AutomaticButtonsFromStrings)
                {
                    int idx = s.Text
                        .IndexOf('['); //for now I'm only checking for a single brace here.
                    if (idx != -1 && idx + 1 < s.Text.Length)
                    {
                        ConsoleKey key = ConsoleKey.A;
                        bool shifted = false;
                        switch (s.Text[idx + 1])
                        {
                            case 'E':
                                key = ConsoleKey.Enter;
                                break;
                            case 'T':
                                key = ConsoleKey.Tab;
                                break;
                            case 'P': //"Press any key"
                                break;
                            case '?':
                                key = ConsoleKey.Oem2;
                                shifted = true;
                                break;
                            case '=':
                                key = ConsoleKey.OemPlus;
                                break;
                            default: //all others should be lowercase letters
                                key = (ConsoleKey) (ConsoleKey.A +
                                                    ((int) s.Text[idx + 1] - (int) 'a'));
                                break;
                        }

                        MouseUI.CreateMapButton(key, shifted, r - Global.MAP_OFFSET_ROWS, 1);
                    }
                }
            }
        }

        public static void WriteMapString(int r, int c, ColorBufferString cs)
        {
            if (cs.Length() > 0)
            {
                r += Global.MAP_OFFSET_ROWS;
                c += Global.MAP_OFFSET_COLS;
                int start_col = -1;
                int end_col = -1;
                int cpos = c;
                foreach (ColorString s1 in cs.strings)
                {
                    ColorString s = new ColorString(s1.Text, s1.Foreground, s1.Background);
                    if (cpos - Global.MAP_OFFSET_COLS + s.Text.Length > Global.COLS)
                    {
                        s.Text = s.Text.Substring(0, Global.COLS - (cpos - Global.MAP_OFFSET_COLS));
                    }

                    s.Foreground = Colors.ResolveColor(s.Foreground);
                    s.Background = Colors.ResolveColor(s.Background);
                    ColorChar cch;
                    cch.color = s.Foreground;
                    cch.bgcolor = s.Background;
                    

                    int i = 0;
                    foreach (char ch in s.Text)
                    {
                        cch.c = ch;
                        if (!memory[r, cpos + i].Equals(cch))
                        {
                            memory[r, cpos + i] = cch;
                            if (start_col == -1)
                            {
                                start_col = cpos + i;
                            }

                            end_col = cpos + i;
                        }

                        ++i;
                    }

                    cpos += s.Text.Length;
                }

                if (  !NoGLUpdate && start_col != -1)
                {
                    UpdateGlBuffer(r, start_col, r, end_col);
                }

                if (MouseUI.AutomaticButtonsFromStrings)
                {
                    int idx = -1;
                    int len = cs.Length();
                    for (int i = 0; i < len; ++i)
                    {
                        if (cs[i].c == '[')
                        {
                            idx = i;
                            break;
                        }
                    }

                    if (idx != -1 && idx + 1 < cs.Length())
                    {
                        ConsoleKey key = ConsoleKey.A;
                        bool shifted = false;
                        switch (cs[idx + 1].c)
                        {
                            case 'E':
                                key = ConsoleKey.Enter;
                                break;
                            case 'T':
                                key = ConsoleKey.Tab;
                                break;
                            case 'P': //"Press any key"
                                break;
                            case '?':
                                key = ConsoleKey.Oem2;
                                shifted = true;
                                break;
                            case '=':
                                key = ConsoleKey.OemPlus;
                                break;
                            default: //all others should be lowercase letters
                                key = (ConsoleKey)
                                    (ConsoleKey.A + ((int) cs[idx + 1].c - (int) 'a'));
                                break;
                        }

                        MouseUI.CreateMapButton(key, shifted, r - Global.MAP_OFFSET_ROWS, 1);
                    }
                }

                /*if(cpos-Global.MAP_OFFSET_COLS < Global.COLS){
                    WriteString(r,cpos,"".PadRight(Global.COLS-(cpos-Global.MAP_OFFSET_COLS)));
                }*/
            }
        }

        public static void WriteStatsChar(int r, int c, ColorChar ch)
        {
            WriteChar(r, c, ch);
        } //was r+1,c

        public static void WriteStatsString(int r, int c, string s)
        {
            ColorString cs;
            cs.Foreground = Color.Gray;
            cs.Background = Color.Black;
            cs.Text = s;
            WriteStatsString(r, c, cs);
        }

        public static void WriteStatsString(int r, int c, string s, Color color)
        {
            ColorString cs;
            cs.Foreground = color;
            cs.Background = Color.Black;
            cs.Text = s;
            WriteStatsString(r, c, cs);
        }

        public static void WriteStatsString(int r, int c, ColorString s)
        {
            if (Global.STATUS_WIDTH - c > s.Text.Length)
            {
                //s.s = s.s.Substring(0); //don't move down to the next line
            }
            else
            {
                s.Text = s.Text.Substring(0, Global.STATUS_WIDTH - c);
            }

            if (s.Text.Length > 0)
            {
                //++r;
                s.Foreground = Colors.ResolveColor(s.Foreground);
                s.Background = Colors.ResolveColor(s.Background);
                ColorChar cch;
                cch.color = s.Foreground;
                cch.bgcolor = s.Background;

                int start_col = -1;
                int end_col = -1;
                int i = 0;
                bool changed = false;
                foreach (char ch in s.Text)
                {
                    cch.c = ch;
                    if (!memory[r, c + i].Equals(cch))
                    {
                        memory[r, c + i] = cch;
                        if (start_col == -1)
                        {
                            start_col = c + i;
                        }

                        end_col = c + i;
                        changed = true;
                    }

                    ++i;
                }

                if (changed)
                {
                    if (!NoGLUpdate)
                    {
                        UpdateGlBuffer(r, start_col, r, end_col);
                    }
                }

                if (MouseUI.AutomaticButtonsFromStrings)
                {
                    int idx = s.Text
                        .IndexOf('['); //for now I'm only checking for a single brace here.
                    if (idx != -1 && idx + 1 < s.Text.Length)
                    {
                        ConsoleKey key = ConsoleKey.A;
                        bool shifted = false;
                        switch (s.Text[idx + 1])
                        {
                            case 'E':
                                key = ConsoleKey.Enter;
                                break;
                            case 'T':
                                key = ConsoleKey.Tab;
                                break;
                            case 'P': //"Press any key"
                                break;
                            case '?':
                                key = ConsoleKey.Oem2;
                                shifted = true;
                                break;
                            case '=':
                                key = ConsoleKey.OemPlus;
                                break;
                            default: //all others should be lowercase letters
                                key = (ConsoleKey) (ConsoleKey.A +
                                                    ((int) s.Text[idx + 1] - (int) 'a'));
                                break;
                        }

                        MouseUI.CreateStatsButton(key, shifted, r, 1);
                    }
                }
            }
        }

        public static void MapDrawWithStrings(ColorChar[,] array, int row, int col, int height,
            int width)
        {
            ColorString s;
            s.Text = "";
            s.Background = Color.Black;
            s.Foreground = Color.Black;
            int current_c = col;
            for (int i = row; i < row + height; ++i)
            {
                s.Text = "";
                current_c = col;
                for (int j = col; j < col + width; ++j)
                {
                    ColorChar ch = array[i, j];
                    if (Colors.ResolveColor(ch.color) != s.Foreground)
                    {
                        if (s.Text.Length > 0)
                        {
                            WriteMapString(i, current_c, s);
                            s.Text = "";
                            s.Text += ch.c;
                            s.Foreground = ch.color;
                            current_c = j;
                        }
                        else
                        {
                            s.Text += ch.c;
                            s.Foreground = ch.color;
                        }
                    }
                    else
                    {
                        s.Text += ch.c;
                    }
                }

                WriteMapString(i, current_c, s);
            }
        }

        public static void AnimateCell(int r, int c, ColorChar ch, int duration)
        {
            ColorChar prev = memory[r, c];
            WriteChar(r, c, ch);
            GLUpdate();
            Thread.Sleep(duration);
            WriteChar(r, c, prev);
        }

        /*public static void AnimateCellNonBlocking(int r,int c,colorchar ch,int duration){
            colorchar prev = memory[r,c]; //experimental animation for realtime input. seems to work decently so far.
            WriteChar(r,c,ch);
            for(int i=0;i<duration;i+=5){
                Thread.Sleep(5);
                if(Console.KeyAvailable){
                    WriteChar(r,c,prev);
                    return;
                }
            }
            WriteChar(r,c,prev);
        }*/
        public static void AnimateMapCell(int r, int c, ColorChar ch)
        {
            AnimateMapCell(r, c, ch, 50);
        }

        public static void AnimateMapCell(int r, int c, ColorChar ch, int duration)
        {
            AnimateCell(r + Global.MAP_OFFSET_ROWS, c + Global.MAP_OFFSET_COLS, ch, duration);
        }

        public static void AnimateMapCells(List<pos> cells, List<ColorChar> chars)
        {
            AnimateMapCells(cells, chars, 50);
        }

        public static void AnimateMapCells(List<pos> cells, List<ColorChar> chars, int duration)
        {
            List<ColorChar> prev = new List<ColorChar>();
            int idx = 0;
            foreach (pos p in cells)
            {
                prev.Add(MapChar(p.row, p.col));
                WriteMapChar(p.row, p.col, chars[idx]);
                ++idx;
            }

            GLUpdate();
            Thread.Sleep(duration);
            idx = 0;
            foreach (pos p in cells)
            {
                WriteMapChar(p.row, p.col, prev[idx]);
                ++idx;
            }
        }

        public static void AnimateMapCells(List<pos> cells, ColorChar ch)
        {
            AnimateMapCells(cells, ch, 50);
        }

        public static void AnimateMapCells(List<pos> cells, ColorChar ch, int duration)
        {
            List<ColorChar> prev = new List<ColorChar>();
            int idx = 0;
            foreach (pos p in cells)
            {
                prev.Add(MapChar(p.row, p.col));
                WriteMapChar(p.row, p.col, ch);
                ++idx;
            }

            GLUpdate();
            Thread.Sleep(duration);
            idx = 0;
            foreach (pos p in cells)
            {
                WriteMapChar(p.row, p.col, prev[idx]);
                ++idx;
            }
        }

        public static void AnimateProjectile(List<Tile> list, ColorChar ch)
        {
            AnimateProjectile(list, ch, 50);
        }

        public static void AnimateProjectile(List<Tile> list, ColorChar ch, int duration)
        {
            CursorVisible = false;
            list.RemoveAt(0);
            foreach (Tile t in list)
            {
                AnimateMapCell(t.row, t.col, ch, duration);
            }

            CursorVisible = true;
        }

        public static void AnimateBoltProjectile(List<Tile> list, Color color)
        {
            AnimateBoltProjectile(list, color, 50);
        }

        public static void AnimateBoltProjectile(List<Tile> list, Color color, int duration)
        {
            CursorVisible = false;
            ColorChar ch;
            ch.color = color;
            ch.bgcolor = Color.Black;
            ch.c = '!';
            switch (list[0].DirectionOf(list[list.Count - 1]))
            {
                case 7:
                case 3:
                    ch.c = '\\';
                    break;
                case 8:
                case 2:
                    ch.c = '|';
                    break;
                case 9:
                case 1:
                    ch.c = '/';
                    break;
                case 4:
                case 6:
                    ch.c = '-';
                    break;
            }

            list.RemoveAt(0);
            foreach (Tile t in list)
            {
                AnimateMapCell(t.row, t.col, ch, duration);
            }

            CursorVisible = true;
        }

        public static void AnimateExplosion(PhysicalObject obj, int radius, ColorChar ch)
        {
            AnimateExplosion(obj, radius, ch, 50, false);
        }

        public static void AnimateExplosion(PhysicalObject obj, int radius, ColorChar ch,
            int duration,
            bool single_frame)
        {
            CursorVisible = false;
            ColorChar[,] prev = new ColorChar[radius * 2 + 1, radius * 2 + 1];
            for (int i = 0; i <= radius * 2; ++i)
            {
                for (int j = 0; j <= radius * 2; ++j)
                {
                    if (MapBoundsCheck(obj.row - radius + i, obj.col - radius + j))
                    {
                        prev[i, j] = MapChar(obj.row - radius + i, obj.col - radius + j);
                    }
                }
            }

            if (!single_frame)
            {
                for (int i = 0; i <= radius; ++i)
                {
                    foreach (Tile t in obj.TilesAtDistance(i))
                    {
                        WriteMapChar(t.row, t.col, ch);
                    }

                    GLUpdate();
                    Thread.Sleep(duration);
                }
            }
            else
            {
                foreach (Tile t in obj.TilesWithinDistance(radius))
                {
                    WriteMapChar(t.row, t.col, ch);
                }

                GLUpdate();
                Thread.Sleep(duration);
            }

            for (int i = 0; i <= radius * 2; ++i)
            {
                for (int j = 0; j <= radius * 2; ++j)
                {
                    if (MapBoundsCheck(obj.row - radius + i, obj.col - radius + j))
                    {
                        WriteMapChar(obj.row - radius + i, obj.col - radius + j, prev[i, j]);
                    }
                }
            }

            CursorVisible = true;
        }

        public static void AnimateBoltBeam(List<Tile> list, Color color)
        {
            AnimateBoltBeam(list, color, 50);
        }

        public static void AnimateBoltBeam(List<Tile> list, Color color, int duration)
        {
            CursorVisible = false;
            ColorChar ch;
            ch.color = color;
            ch.bgcolor = Color.Black;
            ch.c = '!';
            switch (list[0].DirectionOf(list[list.Count - 1]))
            {
                case 7:
                case 3:
                    ch.c = '\\';
                    break;
                case 8:
                case 2:
                    ch.c = '|';
                    break;
                case 9:
                case 1:
                    ch.c = '/';
                    break;
                case 4:
                case 6:
                    ch.c = '-';
                    break;
            }

            list.RemoveAt(0);
            List<ColorChar> memlist = new List<ColorChar>();
            foreach (Tile t in list)
            {
                memlist.Add(MapChar(t.row, t.col));
                WriteMapChar(t.row, t.col, ch);
                GLUpdate();
                Thread.Sleep(duration);
            }

            int i = 0;
            foreach (Tile t in list)
            {
                WriteMapChar(t.row, t.col, memlist[i++]);
            }

            CursorVisible = true;
        }

        public static void AnimateBeam(List<Tile> list, ColorChar ch)
        {
            AnimateBeam(list, ch, 50);
        }

        public static void AnimateBeam(List<Tile> list, ColorChar ch, int duration)
        {
            CursorVisible = false;
            list.RemoveAt(0);
            List<ColorChar> memlist = new List<ColorChar>();
            foreach (Tile t in list)
            {
                memlist.Add(MapChar(t.row, t.col));
                WriteMapChar(t.row, t.col, ch);
                GLUpdate();
                Thread.Sleep(duration);
            }

            int i = 0;
            foreach (Tile t in list)
            {
                WriteMapChar(t.row, t.col, memlist[i++]);
            }

            CursorVisible = true;
        }

        public static void AnimateStorm(pos origin, int radius, int num_frames, int num_per_frame,
            char c, Color color)
        {
            AnimateStorm(origin, radius, num_frames, num_per_frame, new ColorChar(c, color));
        }

        public static void AnimateStorm(pos origin, int radius, int num_frames, int num_per_frame,
            ColorChar ch)
        {
            for (int i = 0; i < num_frames; ++i)
            {
                List<pos> cells = new List<pos>();
                List<pos> nearby = origin.PositionsWithinDistance(radius);
                for (int j = 0; j < num_per_frame; ++j)
                {
                    pos p = nearby.RemoveRandom();
                    if (MapBoundsCheck(p.row, p.col))
                    {
                        //the ones that are out of bounds still count toward the total, so they don't become more dense as you get near edges.
                        cells.Add(p);
                    }
                }

                AnimateMapCells(cells, ch);
            }
        }

        public static void DrawMapBorder(ColorChar ch)
        {
            for (int i = 0; i < Global.ROWS; i += Global.ROWS - 1)
            {
                for (int j = 0; j < Global.COLS; ++j)
                {
                    WriteMapChar(i, j, ch);
                }
            }

            for (int j = 0; j < Global.COLS; j += Global.COLS - 1)
            {
                for (int i = 0; i < Global.ROWS; ++i)
                {
                    WriteMapChar(i, j, ch);
                }
            }
        }

        public static Input.Key GetKeyPressed()
        {
            return gl.GetKeyPressed();
        }
    }
}