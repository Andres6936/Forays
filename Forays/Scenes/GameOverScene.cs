using System;
using System.Collections.Generic;
using System.IO;
using Forays.Enums;
using Utilities;

namespace Forays.Scenes
{
    public class GameOverScene : IScene
    {
        public void Draw()
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

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public NextScene ProcessInput()
        {
            throw new System.NotImplementedException();
        }
    }
}