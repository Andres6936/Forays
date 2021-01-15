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
using System.IO;
using System.Reflection;
using Forays.Entity;
using Forays.Enums;
using Forays.Loader;
using Forays.Renderer;
using Forays.Scenes;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Utilities;
using static Nym.NameElement;
using PosArrays;
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
            {
                int os = (int) Environment.OSVersion.Platform;
                if (os == 4 || os == 6 || os == 128)
                {
                    Global.LINUX = true;
                }
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
                        Input.ReadKey(false);
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
                Screen.gl.Icon = new System.Drawing.Icon(Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(Global.ForaysImageResources + "forays.ico"));
                Screen.gl.ResizingPreference = ResizeOption.SnapWindow;
                Screen.gl.ResizingFullScreenPreference = ResizeOption.AddBorder;
                Screen.gl.KeyDown += Input.KeyDownHandler;
                Screen.gl.Mouse.Move += Input.MouseMoveHandler;
                Screen.gl.Mouse.ButtonUp += Input.MouseClickHandler;
                Screen.gl.Mouse.WheelChanged += Input.MouseWheelHandler;
                Screen.gl.MouseLeave += Input.MouseLeaveHandler;
                Screen.gl.Closing += Input.OnClosing;
                Screen.gl.FinalResize += Input.HandleResize;
                Screen.textSurface = Surface.Create(Screen.gl,
                    Global.ForaysImageResources + "font8x16.png", true,
                    Shader.AAFontFS(), false, 2, 4, 4);
                SpriteType.DefineSingleRowSprite(Screen.textSurface, 8, 1);
                CellLayout.CreateGrid(Screen.textSurface, Global.SCREEN_H, Global.SCREEN_W, 16, 8,
                    0, 0);
                Screen.textSurface.SetEasyLayoutCounts(Global.SCREEN_H * Global.SCREEN_W);
                Screen.textSurface.DefaultUpdatePositions();
                Screen.textSurface.SetDefaultSpriteType(0);
                Screen.textSurface.SetDefaultSprite(32); //space
                Screen.textSurface.SetDefaultOtherData(new List<float>(Color.Gray.GetFloatValues()),
                    new List<float>(Color.Black.GetFloatValues()));
                Screen.textSurface.DefaultUpdateOtherData();
                Screen.gl.Surfaces.Add(Screen.textSurface);
                Screen.cursorSurface = Surface.Create(Screen.gl,
                    Global.ForaysImageResources + "font8x16.png", true,
                    Shader.AAFontFS(), false, 2, 4, 4);
                Screen.cursorSurface.texture = Screen.textSurface.texture;
                CellLayout.CreateGrid(Screen.cursorSurface, 1, 1, 2, 8, 0, 0);
                Screen.cursorSurface.SetEasyLayoutCounts(1);
                Screen.cursorSurface.DefaultUpdatePositions();
                Screen.cursorSurface.SetDefaultSpriteType(0);
                Screen.cursorSurface.SetDefaultSprite(32);
                Screen.cursorSurface.SetDefaultOtherData(
                    new List<float>(Color.Black.GetFloatValues()),
                    new List<float>(Color.Gray.GetFloatValues()));
                Screen.cursorSurface.DefaultUpdateOtherData();
                Screen.gl.Surfaces.Add(Screen.cursorSurface);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                Screen.gl.Visible = true;
                Global.Timer = new Stopwatch();
                Global.Timer.Start();
                Screen.CursorVisible = false;
            }

            Nym.Verbs.Register("feel",
                "looks"); //Useful for generating messages like "You feel stronger" / "The foo looks stronger".
            Input.LoadKeyRebindings();

            var manager = new SceneManager();

            while (manager.IsRunning())
            {
                manager.Clear();
                manager.Draw();
                manager.ProcessInput();
            }

            MainMenu();
        }

        static void MainMenu()
        {
            while (true)
            {
                if (Global.QUITTING)
                {
                    Global.Quit();
                }
            }
        }

        static void GameOverScreen(Game game)
        {
            MouseUI.PushButtonMap();
            game.Player.attrs[AttrType.BLIND] = 0; //make sure the player can actually view the map
            game.Player.attrs[AttrType.BURNING] = 0;
            game.Player.attrs[AttrType.FROZEN] = 0; //...without borders
            //game.M.Draw();
            ColorChar[,] mem = null;
            UI.DisplayStats();
            bool showed_IDed_tip = false;
            if (Global.KILLED_BY != "gave up" &&
                !Help.displayed[TutorialTopic.IdentifiedConsumables])
            {
                if (game.Player.inv.Where(item =>
                        Item.identified[item.type] &&
                        item.Is(ConsumableType.HEALING, ConsumableType.TIME)).Count > 0)
                {
                    Help.TutorialTip(TutorialTopic.IdentifiedConsumables);
                    Global.SaveOptions();
                    showed_IDed_tip = true;
                }
            }

            if (!showed_IDed_tip && Global.KILLED_BY != "gave up" &&
                !Help.displayed[TutorialTopic.UnidentifiedConsumables])
            {
                int known_count = 0;
                foreach (ConsumableType ct in Item.identified)
                {
                    if (Item.identified[ct] && Item.GetItemClass(ct) != ConsumableClass.OTHER)
                    {
                        ++known_count;
                    }
                }

                if (known_count < 2 &&
                    game.Player.inv.Where(item => !Item.identified[item.type]).Count > 2)
                {
                    Help.TutorialTip(TutorialTopic.UnidentifiedConsumables);
                    Global.SaveOptions();
                }
            }

            Hash<ConsumableType> knownAtTimeOfDeath = new Hash<ConsumableType>(Item.identified);
            foreach (ConsumableType ct in Enum.GetValues(typeof(ConsumableType)))
            {
                Item.identified[ct] = true;
            }

            List<string> postMortemInventoryList = new List<string>();
            foreach (Item i in game.Player.inv)
            {
                if (i.ItemClass == ConsumableClass.WAND) i.other_data = -1;
                if (knownAtTimeOfDeath[i.type])
                {
                    postMortemInventoryList.Add(i.GetName(false, An, Extra));
                }
                else
                {
                    if (Item.tried[i.type])
                    {
                        postMortemInventoryList.Add(i.GetName(false, An, Extra) + " {tried}");
                    }
                    else
                    {
                        postMortemInventoryList.Add(i.GetName(false, An, Extra) + " {untried}");
                    }
                }
            }

            List<string> ls = new List<string>();
            ls.Add("See the map");
            ls.Add("See last messages");
            ls.Add("Examine your equipment");
            ls.Add("Examine your inventory");
            ls.Add("View known item types");
            ls.Add("See character info");
            ls.Add("Write this information to a file");
            ls.Add("Done");
            for (bool done = false; !done;)
            {
                if (mem != null)
                {
                    Screen.MapDrawWithStrings(mem, 0, 0, Global.ROWS, Global.COLS);
                }

                game.Player.Select("Would you like to examine your character! ",
                    "".PadRight(Global.COLS),
                    "".PadRight(Global.COLS), ls, true, false, false);
                int sel = game.Player.GetSelection("Would you like to examine your character? ",
                    ls.Count, true, false,
                    false);
                mem = Screen.GetCurrentMap();
                switch (sel)
                {
                    case 0:
                        MouseUI.PushButtonMap();
                        Dictionary<Actor, ColorChar> old_ch = new Dictionary<Actor, ColorChar>();
                        List<Actor> drawn = new List<Actor>();
                        foreach (Actor a in game.map.AllActors())
                        {
                            if (game.Player.CanSee(a))
                            {
                                old_ch.Add(a, game.map.last_seen[a.row, a.col]);
                                game.map.last_seen[a.row, a.col] = new ColorChar(a.symbol, a.color);
                                drawn.Add(a);
                            }
                        }

                        Screen.MapDrawWithStrings(game.map.last_seen, 0, 0, Global.ROWS,
                            Global.COLS);
                        game.Player.GetTarget(true, -1, -1, true, false, false, "");
                        //game.UI.Display("Press any key to continue. ");
                        //Input.ReadKey();
                        MouseUI.PopButtonMap();
                        foreach (Actor a in drawn)
                        {
                            game.map.last_seen[a.row, a.col] = old_ch[a];
                        }

                        game.map.Redraw();
                        /*foreach(Tile t in game.M.AllTiles()){
                            if(t.type != TileType.FLOOR && !t.IsTrap()){
                                bool good = false;
                                foreach(Tile neighbor in t.TilesAtDistance(1)){
                                    if(neighbor.type != TileType.WALL){
                                        good = true;
                                    }
                                }
                                if(good){
                                    t.seen = true;
                                }
                            }
                        }
                        game.UI.Display("Press any key to continue. ");
                        Screen.WriteMapChar(0,0,'-');
                        game.M.Draw();
                        Input.ReadKey();*/
                        break;
                    case 1:
                    {
                        SharedEffect.ShowPreviousMessages(false);
                        break;
                    }
                    case 2:
                        UI.DisplayEquipment();
                        break;
                    case 3:
                        MouseUI.PushButtonMap();
                        MouseUI.AutomaticButtonsFromStrings = true;
                        for (int i = 1; i < 9; ++i)
                        {
                            Screen.WriteMapString(i, 0, "".PadRight(Global.COLS));
                        }

                        MouseUI.AutomaticButtonsFromStrings = false;
                        game.Player.Select("In your pack: ", postMortemInventoryList, true, false,
                            false);
                        Input.ReadKey();
                        MouseUI.PopButtonMap();
                        break;
                    case 4:
                    {
                        SharedEffect.ShowKnownItems(knownAtTimeOfDeath);
                        break;
                    }
                    case 5:
                        UI.DisplayCharacterInfo();
                        break;
                    case 6:
                    {
                        UI.Display("Enter file name: ");
                        MouseUI.PushButtonMap();
                        string filename = Input.EnterString(40);
                        MouseUI.PopButtonMap();
                        if (filename == "")
                        {
                            break;
                        }

                        if (!filename.Contains("."))
                        {
                            filename = filename + ".txt";
                        }

                        StreamWriter file = new StreamWriter(filename, true);
                        UI.DisplayCharacterInfo(false);
                        ColorChar[,] screen = Screen.GetCurrentScreen();
                        for (int i = 2; i < Global.SCREEN_H; ++i)
                        {
                            for (int j = 0; j < Global.SCREEN_W; ++j)
                            {
                                file.Write(screen[i, j].c);
                            }

                            file.WriteLine();
                        }

                        file.WriteLine();
                        file.WriteLine("Inventory: ");
                        foreach (string s in postMortemInventoryList)
                        {
                            file.WriteLine(s);
                        }

                        if (postMortemInventoryList.Count == 0)
                        {
                            file.WriteLine("(nothing)");
                        }

                        file.WriteLine();
                        file.WriteLine("Known items: ");
                        bool known_items_found = false;
                        foreach (ConsumableType ct in knownAtTimeOfDeath)
                        {
                            if (Item.GetItemClass(ct) != ConsumableClass.OTHER)
                            {
                                known_items_found = true;
                                file.WriteLine(Item.Prototype(ct).Name.Singular);
                            }
                        }

                        if (!known_items_found)
                        {
                            file.WriteLine("(none)");
                        }
                        else
                        {
                            file.WriteLine();
                        }

                        file.WriteLine();
                        foreach (Tile t in game.map.AllTiles())
                        {
                            if (t.type != TileType.FLOOR && !t.IsTrap())
                            {
                                bool good = false;
                                foreach (Tile neighbor in t.TilesAtDistance(1))
                                {
                                    if (neighbor.type != TileType.WALL)
                                    {
                                        good = true;
                                    }
                                }

                                if (good)
                                {
                                    t.seen = true;
                                }
                            }
                        }

                        Screen.WriteMapChar(0, 0,
                            '-'); //todo: this was a hack. can now be replaced with the proper Redraw method, I think.
                        game.map.Draw();
                        int col = 0;
                        foreach (ColorChar cch in Screen.GetCurrentMap())
                        {
                            file.Write(cch.c);
                            ++col;
                            if (col == Global.COLS)
                            {
                                file.WriteLine();
                                col = 0;
                            }
                        }

                        file.WriteLine();
                        file.WriteLine("Last messages: ");
                        foreach (string s in game.MessageBuffer.GetMessageLog())
                        {
                            //todo, limit message log size?
                            if (s != "")
                            {
                                file.WriteLine(s);
                            }
                        }

                        /*Screen.WriteMapString(0,0,"".PadRight(Global.COLS,'-'));
                        int line = 1;
                        foreach(string s in game.B.GetMessages()){
                            if(line < 21){
                                Screen.WriteMapString(line,0,s.PadRight(Global.COLS));
                            }
                            ++line;
                        }
                        Screen.WriteMapString(21,0,"".PadRight(Global.COLS,'-'));
                        file.WriteLine("Last messages: ");
                        col = 0;
                        foreach(colorchar cch in Screen.GetCurrentMap()){
                            file.Write(cch.c);
                            ++col;
                            if(col == Global.COLS){
                                file.WriteLine();
                                col = 0;
                            }
                        }*/
                        file.WriteLine();
                        file.Close();
                        break;
                    }
                    case 7:
                        done = true;
                        break;
                    default:
                        break;
                }
            }

            MouseUI.PopButtonMap();
        }
    }
}