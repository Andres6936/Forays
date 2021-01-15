using System;
using System.Collections.Generic;
using System.IO;
using Forays.Entity;
using Forays.Enums;
using Forays.Loader;
using PosArrays;
using Utilities;

namespace Forays.Scenes
{
    public class PlayScene : IScene
    {
        // Member Variable

        private int recentdepth = -1;
        private char recentwin = '-';
        private bool saved_game;
        private bool on_highscore_list = false;
        private string recentcause = "";
        private string recentname = "".PadRight(30);
        private Queue Queue;
        private ConsoleKeyInfo command;

        // Construct

        public PlayScene()
        {
            MouseUI.PushButtonMap();
            saved_game = File.Exists("forays.sav");
            Global.GAME_OVER = false;
            Global.BOSS_KILLED = false;
            Global.SAVING = false;
            Global.LoadOptions();
        }

        // Methods Public

        public void Draw()
        {
            int row = 8;
            int col = (Global.SCREEN_W - 28) /
                      2; //centering "Forays into Norrendrin x.y.z", which is 28 chars.
            Screen.WriteString(row++, col,
                new ColorString("Forays into Norrendrin " + Global.VERSION, Color.Yellow));
            Screen.WriteString(row++, col, new ColorString("".PadRight(28, '-'), Color.Green));
            col += 4; //recenter for menu options
            row++;

            if (!saved_game)
            {
                Screen.WriteString(row++, col, "[a] Start a new game");
            }
            else
            {
                Screen.WriteString(row++, col, "[a] Resume saved game");
            }

            Screen.WriteString(row++, col, "[b] How to play");
            Screen.WriteString(row++, col, "[c] High scores");
            Screen.WriteString(row++, col, "[d] Quit");
            for (int i = 0; i < 4; ++i)
            {
                Screen.WriteChar(i + row - 4, col + 1,
                    new ColorChar(Color.Cyan, (char) (i + 'a')));
                MouseUI.CreateButton((ConsoleKey) (i + ConsoleKey.A), false, i + row - 4, 0, 1,
                    Global.SCREEN_W);
            }

            Screen.ResetColors();
            Screen.SetCursorPosition(Global.MAP_OFFSET_COLS, Global.MAP_OFFSET_ROWS + 8);
        }

        public void Clear()
        {
            Screen.Blank();
        }

        public NextScene ProcessInput()
        {
            command = Input.ReadKey(false);

            switch (command.KeyChar)
            {
                case 'a':
                {
                    Actor.attack[ActorType.PLAYER] = new List<AttackInfo>
                    {
                        new AttackInfo(100, 2, AttackEffect.NO_CRIT, "& hit *", "& miss *", "")
                    };
                    if (!saved_game)
                    {
                        Actor.feats_in_order = new List<FeatType>();
                        Actor.spells_in_order = new List<SpellType>();
                    }

                    MapView.Map = new Map(PlayerView.Player, Queue,
                        MessageBufferView.MessageBuffer);
                    MessageBufferView.MessageBuffer = new MessageBuffer(PlayerView.Player);
                    Queue = new Queue(MessageBufferView.MessageBuffer);
                    Map.Q = Queue;
                    Map.B = MessageBufferView.MessageBuffer;
                    PhysicalObject.M = MapView.Map;
                    PhysicalObject.B = MessageBufferView.MessageBuffer;
                    PhysicalObject.Q = Queue;
                    PhysicalObject.player = PlayerView.Player;
                    Event.Q = Queue;
                    Event.B = MessageBufferView.MessageBuffer;
                    Event.M = MapView.Map;
                    Event.player = PlayerView.Player;
                    Fire.fire_event = null;
                    Fire.burning_objects = new List<PhysicalObject>();
                    if (!saved_game)
                    {
                        Actor.player_name = "";
                        if (File.Exists("name.txt"))
                        {
                            StreamReader file = new StreamReader("name.txt");
                            string base_name = file.ReadLine();
                            if (base_name == "%random%")
                            {
                                Actor.player_name = Global.GenerateCharacterName();
                            }
                            else
                            {
                                Actor.player_name = base_name;
                            }

                            int num = 0;
                            if (base_name != "%random%" && file.Peek() != -1)
                            {
                                num = Convert.ToInt32(file.ReadLine());
                                if (num > 1)
                                {
                                    Actor.player_name =
                                        Actor.player_name + " " + Global.RomanNumeral(num);
                                }
                            }

                            file.Close();
                            if (num > 0)
                            {
                                StreamWriter fileout = new StreamWriter("name.txt", false);
                                fileout.WriteLine(base_name);
                                fileout.WriteLine(num + 1);
                                fileout.Close();
                            }
                        }

                        if (Actor.player_name == "")
                        {
                            MouseUI.PushButtonMap(MouseMode.NameEntry);
                            Screen.Blank();
                            /*for(int i=4;i<=7;++i){
                                Screen.WriteMapString(i,0,"".PadToMapSize());
                            }*/
                            string s = "";
                            int name_option = 0;
                            int c = 3;
                            while (true)
                            {
                                Screen.WriteMapString(4, c, "Enter name: ");
                                if (s == "")
                                {
                                    Screen.WriteMapString(6, c,
                                        "(Press [Enter] for a random name)".GetColorString());
                                }
                                else
                                {
                                    Screen.WriteMapString(6, c,
                                        "(Press [Enter] when finished)    ".GetColorString());
                                }

                                List<string> name_options = new List<string>
                                {
                                    "Default: Choose a new name for each character",
                                    "Static:  Use this name for every character",
                                    "Legacy:  Name all future characters after this one",
                                    "Random:  Name all future characters randomly"
                                };
                                for (int i = 0; i < 4; ++i)
                                {
                                    Color option_color = Color.DarkGray;
                                    if (i == name_option)
                                    {
                                        option_color = Color.White;
                                    }

                                    Screen.WriteMapString(15 + i, c, name_options[i],
                                        option_color);
                                }

                                Screen.WriteMapString(20, c,
                                    "(Press [Tab] to change naming preference)"
                                        .GetColorString());
                                if (name_option != 0)
                                {
                                    Screen.WriteMapString(22, c - 5,
                                        "(To stop naming characters automatically, delete name.txt)",
                                        Color.Green);
                                }
                                else
                                {
                                    Screen.WriteMapString(22, c - 5, "".PadToMapSize());
                                }

                                Screen.WriteMapString(4, c + 12, s.PadRight(26));
                                Screen.SetCursorPosition(
                                    c + Global.MAP_OFFSET_COLS + 12 + s.Length,
                                    Global.MAP_OFFSET_ROWS + 4);
                                MouseUI.CreateButton(ConsoleKey.Enter, false,
                                    6 + Global.MAP_OFFSET_ROWS, 0, 1,
                                    Global.SCREEN_W);
                                MouseUI.CreateButton(ConsoleKey.Tab, false,
                                    20 + Global.MAP_OFFSET_ROWS, 0, 1,
                                    Global.SCREEN_W);
                                MouseUI.CreateButton(ConsoleKey.F21, false,
                                    15 + Global.MAP_OFFSET_ROWS, 0, 1,
                                    Global.SCREEN_W);
                                MouseUI.CreateButton(ConsoleKey.F22, false,
                                    16 + Global.MAP_OFFSET_ROWS, 0, 1,
                                    Global.SCREEN_W);
                                MouseUI.CreateButton(ConsoleKey.F23, false,
                                    17 + Global.MAP_OFFSET_ROWS, 0, 1,
                                    Global.SCREEN_W);
                                MouseUI.CreateButton(ConsoleKey.F24, false,
                                    18 + Global.MAP_OFFSET_ROWS, 0, 1,
                                    Global.SCREEN_W);
                                command = Input.ReadKey();
                                if ((command.KeyChar >= '!' && command.KeyChar <= '~') ||
                                    command.KeyChar == ' ')
                                {
                                    if (s.Length < 26)
                                    {
                                        s = s + command.KeyChar;
                                    }
                                }
                                else
                                {
                                    if (command.Key == ConsoleKey.Backspace && s.Length > 0)
                                    {
                                        s = s.Substring(0, s.Length - 1);
                                    }
                                    else
                                    {
                                        if (command.Key == ConsoleKey.Escape)
                                        {
                                            s = "";
                                        }
                                        else
                                        {
                                            if (command.Key == ConsoleKey.Tab)
                                            {
                                                name_option = (name_option + 1) % 4;
                                            }
                                            else
                                            {
                                                if (command.Key == ConsoleKey.Enter)
                                                {
                                                    if (s.Length == 0)
                                                    {
                                                        s = Global.GenerateCharacterName();
                                                    }
                                                    else
                                                    {
                                                        Actor.player_name = s;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    switch (command.Key)
                                                    {
                                                        case ConsoleKey.F21:
                                                            name_option = 0;
                                                            break;
                                                        case ConsoleKey.F22:
                                                            name_option = 1;
                                                            break;
                                                        case ConsoleKey.F23:
                                                            name_option = 2;
                                                            break;
                                                        case ConsoleKey.F24:
                                                            name_option = 3;
                                                            break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            MouseUI.PopButtonMap();
                            switch (name_option)
                            {
                                case 1: //static
                                {
                                    StreamWriter fileout = new StreamWriter("name.txt", false);
                                    fileout.WriteLine(s);
                                    fileout.WriteLine(0);
                                    fileout.Close();
                                    break;
                                }
                                case 2: //legacy
                                {
                                    StreamWriter fileout = new StreamWriter("name.txt", false);
                                    fileout.WriteLine(s);
                                    fileout.WriteLine(2);
                                    fileout.Close();
                                    break;
                                }
                                case 3: //random
                                {
                                    StreamWriter fileout = new StreamWriter("name.txt", false);
                                    fileout.WriteLine("%random%");
                                    fileout.WriteLine(0);
                                    fileout.Close();
                                    break;
                                }
                            }
                        }

                        {
                            Event e = new Event(PlayerView.Player, 0, EventType.MOVE);
                            e.tiebreaker = 0;
                            Queue.Add(e);
                        }
                        Item.GenerateUnIDedNames();
                        MapView.Map.GenerateLevelTypes();
                        MapView.Map.GenerateLevel();
                        PlayerView.Player.UpdateRadius(0, 6, true);
                        Item.Create(ConsumableType.BANDAGES, PlayerView.Player).other_data = 5;
                        Item.Create(ConsumableType.FLINT_AND_STEEL, PlayerView.Player).other_data =
                            3;
                        PlayerView.Player.inv[0].revealed_by_light = true;
                        PlayerView.Player.inv[1].revealed_by_light = true;
                    }
                    else
                    {
                        var binaryLoader = new BinaryLoader("forays.sav");

                        //loading
                        FileStream file = new FileStream("forays.sav", FileMode.Open);
                        BinaryReader b = new BinaryReader(file);

                        var id = new Dictionary<int, PhysicalObject>();
                        id.Add(0, null);
                        var missing_target_id = new Dict<PhysicalObject, int>();
                        var need_targets = new List<Actor>();
                        var missing_location_id = new Dict<PhysicalObject, int>();
                        var need_location = new List<Actor>();

                        // 1. Player Name (String)
                        Actor.player_name = binaryLoader.NamePlayer;
                        // 2. Current Level (Int32)
                        MapView.Map.currentLevelIdx = binaryLoader.CurrentLevelId;
                        MapView.Map.level_types = binaryLoader.LevelTypes;

                        // 4. Wiz Lite (Boolean)
                        MapView.Map.wiz_lite = binaryLoader.WizLite;
                        // 5. Wiz Dark (Boolean)
                        MapView.Map.wiz_dark = binaryLoader.WizDark;
                        MapView.Map.last_seen = binaryLoader.LastSeen;
                        // 9. Final Level Cultist Count (Int32)
                        MapView.Map.final_level_cultist_count =
                            binaryLoader.FinalLevelCultistCount;
                        // 10. Final Level Demon Count (Int32)
                        MapView.Map.final_level_demon_count = binaryLoader.FinalLevelDemonCount;
                        // 11. Final Level Clock (Int32)
                        MapView.Map.final_level_clock = binaryLoader.FinalLevelClock;

                        Actor.feats_in_order = binaryLoader.FeatTypes;
                        Actor.spells_in_order = binaryLoader.SpellTypes;
                        Actor.tiebreakers = binaryLoader.Tiebreakers;

                        foreach (var tiebreaker in Actor.tiebreakers)
                        {
                            if (tiebreaker.row >= 0 && tiebreaker.row < Global.ROWS &&
                                tiebreaker.col >= 0 && tiebreaker.col < Global.COLS)
                            {
                                MapView.Map.actor[tiebreaker.row, tiebreaker.col] = tiebreaker;
                            }

                            if (tiebreaker.type == ActorType.PLAYER)
                            {
                                PlayerView.Player = tiebreaker;
                                Actor.player = tiebreaker;
                                Item.player = tiebreaker;
                                Map.player = tiebreaker;
                                Event.player = tiebreaker;
                                Tile.player = tiebreaker;
                            }

                            if (tiebreaker.target == null)
                            {
                                need_targets.Add(tiebreaker);
                                // missing_target_id[a] = target_ID;
                            }

                            if (tiebreaker.target_location == null)
                            {
                                need_location.Add(tiebreaker);
                                // missing_location_id[a] = location_ID;
                            }
                        }

                        // 15. Number Groups (Int32)
                        int num_groups = b.ReadInt32();
                        for (int i = 0; i < num_groups; ++i)
                        {
                            List<Actor> group = new List<Actor>();
                            // 15.1 Group Size (Int32)
                            int group_size = b.ReadInt32();
                            for (int j = 0; j < group_size; ++j)
                            {
                                // 15.1.1 Actor Value (Int32)
                                group.Add((Actor) id[b.ReadInt32()]);
                            }

                            foreach (Actor a in group)
                            {
                                a.group = group;
                            }
                        }

                        // 16. Number Tiles (Int32)
                        int num_tiles = b.ReadInt32();
                        for (int i = 0; i < num_tiles; ++i)
                        {
                            Tile t = new Tile();
                            // 16.1 Identification Tile (Int32)
                            int ID = b.ReadInt32();
                            id.Add(ID, t);
                            // 16.2 Row Tile (Int32)
                            t.row = b.ReadInt32();
                            // 16.3 Column Tile (Int32)
                            t.col = b.ReadInt32();
                            MapView.Map.tile[t.row, t.col] = t;
                            //todo name
                            // 16.4 Symbol Tile (Char)
                            t.symbol = b.ReadChar();
                            // 16.5 Color Tile (Int32)
                            t.color = (Color) b.ReadInt32();
                            // 16.6 Light Radius Tile (Int32)
                            t.light_radius = b.ReadInt32();
                            // 16.7 Type of Tile (Int32)
                            t.type = (TileType) b.ReadInt32();
                            // 16.8 Is a passable Tile (Boolean) 
                            t.passable = b.ReadBoolean();
                            // 16.9 Has internal opacity (Boolean)
                            t.SetInternalOpacity(b.ReadBoolean());
                            // 16.10 Is internal seen (Boolean)
                            t.SetInternalSeen(b.ReadBoolean());
                            // 16.11 Revealed by Ligth (Boolean)
                            t.revealed_by_light = b.ReadBoolean();
                            // 16.12 Is Solid Rock (Boolean)
                            t.solid_rock = b.ReadBoolean();
                            // 16.13 Light value (Int32)
                            t.light_value = b.ReadInt32();
                            // 16.14 Direction Exited (Int32)
                            t.direction_exited = b.ReadInt32();
                            // 16.15 Has toggle value
                            if (b.ReadBoolean())
                            {
                                //indicates a toggles_into value
                                // 16.15.1 Value of Toggle (Int32)
                                t.toggles_into = (TileType) b.ReadInt32();
                            }
                            else
                            {
                                t.toggles_into = null;
                            }

                            // 16.16 Item ID (Int32)
                            int item_id = b.ReadInt32();
                            if (item_id != 0)
                            {
                                t.inv = new Item();
                                id.Add(item_id, t.inv);
                                // 16.16.1 Item row (Int32)
                                t.inv.row = b.ReadInt32();
                                // 16.16.2 Item column (Int32)
                                t.inv.col = b.ReadInt32();
                                //todo name
                                // 16.16.3 Symbol Item (Char)
                                t.inv.symbol = b.ReadChar();
                                // 16.16.4 Color Item (Int32)
                                t.inv.color = (Color) b.ReadInt32();
                                t.inv.light_radius = b.ReadInt32();
                                t.inv.type = (ConsumableType) b.ReadInt32();
                                t.inv.quantity = b.ReadInt32();
                                t.inv.charges = b.ReadInt32();
                                t.inv.other_data = b.ReadInt32();
                                t.inv.ignored = b.ReadBoolean();
                                t.inv.do_not_stack = b.ReadBoolean();
                                t.inv.revealed_by_light = b.ReadBoolean();
                            }
                            else
                            {
                                t.inv = null;
                            }

                            int num_features = b.ReadInt32();
                            for (int j = 0; j < num_features; ++j)
                            {
                                t.features.Add((FeatureType) b.ReadInt32());
                            }
                        }

                        foreach (Actor a in need_targets)
                        {
                            if (id.ContainsKey(missing_target_id[a]))
                            {
                                a.target = (Actor) id[missing_target_id[a]];
                            }
                            else
                            {
                                throw new Exception("Error: some actors weren't loaded(1). ");
                            }
                        }

                        foreach (Actor a in need_location)
                        {
                            if (id.ContainsKey(missing_location_id[a]))
                            {
                                a.target_location = (Tile) id[missing_location_id[a]];
                            }
                            else
                            {
                                throw new Exception("Error: some tiles weren't loaded(2). ");
                            }
                        }

                        int game_turn = b.ReadInt32();
                        Queue.turn =
                            -1; //this keeps events from being added incorrectly to the front of the queue while loading. turn is set correctly after events are all loaded.
                        int num_events = b.ReadInt32();
                        for (int i = 0; i < num_events; ++i)
                        {
                            Event e = new Event();
                            if (b.ReadBoolean())
                            {
                                //if true, this is an item that doesn't exist elsewhere, so grab all its info.
                                int item_id = b.ReadInt32();
                                if (item_id != 0)
                                {
                                    Item item = new Item();
                                    id.Add(item_id, item);
                                    item.row = b.ReadInt32();
                                    item.col = b.ReadInt32();
                                    //todo name
                                    item.symbol = b.ReadChar();
                                    item.color = (Color) b.ReadInt32();
                                    item.light_radius = b.ReadInt32();
                                    item.type = (ConsumableType) b.ReadInt32();
                                    item.quantity = b.ReadInt32();
                                    item.charges = b.ReadInt32();
                                    item.other_data = b.ReadInt32();
                                    item.ignored = b.ReadBoolean();
                                    item.do_not_stack = b.ReadBoolean();
                                    item.revealed_by_light = b.ReadBoolean();
                                    e.target = item;
                                }
                            }
                            else
                            {
                                int target_ID = b.ReadInt32();
                                if (id.ContainsKey(target_ID))
                                {
                                    e.target = id[target_ID];
                                }
                                else
                                {
                                    throw new Exception(
                                        "Error: some tiles/actors weren't loaded(4). ");
                                }
                            }

                            int area_count = b.ReadInt32();
                            for (int j = 0; j < area_count; ++j)
                            {
                                if (e.area == null)
                                {
                                    e.area = new List<Tile>();
                                }

                                int tile_ID = b.ReadInt32();
                                if (id.ContainsKey(tile_ID))
                                {
                                    e.area.Add((Tile) id[tile_ID]);
                                }
                                else
                                {
                                    throw new Exception(
                                        "Error: some tiles weren't loaded(5). ");
                                }
                            }

                            e.delay = b.ReadInt32();
                            e.type = (EventType) b.ReadInt32();
                            e.attr = (AttrType) b.ReadInt32();
                            e.feature = (FeatureType) b.ReadInt32();
                            e.value = b.ReadInt32();
                            e.secondary_value = b.ReadInt32();
                            e.msg = b.ReadString();
                            int objs_count = b.ReadInt32();
                            for (int j = 0; j < objs_count; ++j)
                            {
                                if (e.msg_objs == null)
                                {
                                    e.msg_objs = new List<PhysicalObject>();
                                }

                                int obj_ID = b.ReadInt32();
                                if (id.ContainsKey(obj_ID))
                                {
                                    e.msg_objs.Add(id[obj_ID]);
                                }
                                else
                                {
                                    throw new Exception(
                                        "Error: some actors/tiles weren't loaded(6). ");
                                }
                            }

                            e.time_created = b.ReadInt32();
                            e.dead = b.ReadBoolean();
                            e.tiebreaker = b.ReadInt32();
                            Queue.Add(e);
                            if (e.type == EventType.FIRE && !e.dead)
                            {
                                Fire.fire_event = e;
                            }
                        }

                        Queue.turn = game_turn;
                        foreach (Event e in Queue.list)
                        {
                            if (e.type == EventType.MOVE && e.target == PlayerView.Player)
                            {
                                Queue.current_event = e;
                                break;
                            }
                        }

                        int num_footsteps = b.ReadInt32();
                        for (int i = 0; i < num_footsteps; ++i)
                        {
                            int step_row = b.ReadInt32();
                            int step_col = b.ReadInt32();
                            Actor.footsteps.Add(new pos(step_row, step_col));
                        }

                        int num_prev_footsteps = b.ReadInt32();
                        for (int i = 0; i < num_prev_footsteps; ++i)
                        {
                            int step_row = b.ReadInt32();
                            int step_col = b.ReadInt32();
                            Actor.previous_footsteps.Add(new pos(step_row, step_col));
                        }

                        Actor.interrupted_path.row = b.ReadInt32();
                        Actor.interrupted_path.col = b.ReadInt32();
                        UI.viewing_commands_idx = b.ReadInt32();
                        MapView.Map.feat_gained_this_level = b.ReadBoolean();
                        MapView.Map.extra_danger = b.ReadInt32();
                        int num_unIDed = b.ReadInt32();
                        for (int i = 0; i < num_unIDed; ++i)
                        {
                            ConsumableType ct = (ConsumableType) b.ReadInt32();
                            string s = b.ReadString();
                            //Item.unIDed_name[ct] = s; //todo broke loading here
                        }

                        int num_IDed = b.ReadInt32();
                        for (int i = 0; i < num_IDed; ++i)
                        {
                            ConsumableType ct = (ConsumableType) b.ReadInt32();
                            bool IDed = b.ReadBoolean();
                            Item.identified[ct] = IDed;
                        }

                        int num_item_colors = b.ReadInt32();
                        for (int i = 0; i < num_item_colors; ++i)
                        {
                            ConsumableType ct = (ConsumableType) b.ReadInt32();
                            Item.proto[ct].color = (Color) b.ReadInt32();
                        }

                        int num_burning = b.ReadInt32();
                        for (int i = 0; i < num_burning; ++i)
                        {
                            int obj_ID = b.ReadInt32();
                            if (id.ContainsKey(obj_ID))
                            {
                                Fire.burning_objects.Add(id[obj_ID]);
                            }
                            else
                            {
                                throw new Exception(
                                    "Error: some actors/tiles weren't loaded(7). ");
                            }
                        }

                        MapView.Map.aesthetics =
                            new PosArray<AestheticFeature>(Global.ROWS, Global.COLS);
                        for (int i = 0; i < Global.ROWS; ++i)
                        {
                            for (int j = 0; j < Global.COLS; ++j)
                            {
                                MapView.Map.aesthetics[i, j] = (AestheticFeature) b.ReadInt32();
                            }
                        }

                        MapView.Map.dungeonDescription = b.ReadString();
                        if (b.ReadBoolean())
                        {
                            int numShrines = b.ReadInt32();
                            MapView.Map.nextLevelShrines =
                                new List<SchismDungeonGenerator.CellType>();
                            for (int i = 0; i < numShrines; ++i)
                            {
                                MapView.Map.nextLevelShrines.Add(
                                    (SchismDungeonGenerator.CellType) b.ReadInt32());
                            }
                        }

                        MapView.Map.shrinesFound = new int[5];
                        for (int i = 0; i < 5; ++i)
                        {
                            MapView.Map.shrinesFound[i] = b.ReadInt32();
                        }

                        Tile.spellbooks_generated = b.ReadInt32();
                        UI.viewing_commands_idx = b.ReadInt32();
                        string[] messages = new string[Global.MESSAGE_LOG_LENGTH];
                        int num_messages = b.ReadInt32();
                        for (int i = 0; i < num_messages; ++i)
                        {
                            messages[i] = b.ReadString();
                        }

                        for (int i = num_messages; i < Global.MESSAGE_LOG_LENGTH; ++i)
                        {
                            messages[i] = "";
                        }

                        int message_pos = b.ReadInt32();
                        MessageBufferView.MessageBuffer.LoadMessagesAndPosition(messages,
                            message_pos,
                            num_messages);
                        b.Close();
                        file.Close();
                        File.Delete("forays.sav");
                        Tile.Feature(FeatureType.TELEPORTAL).color =
                            Item.Prototype(ConsumableType.TELEPORTAL).color;
                        MapView.Map.CalculatePoppyDistanceMap();
                        MapView.Map.UpdateDangerValues();
                    }

                    Screen.NoClose = true;
                    MouseUI.PushButtonMap(MouseMode.Map);
                    MouseUI.CreateStatsButtons();
                    try
                    {
                        while (!Global.GAME_OVER)
                        {
                            Queue.Pop();
                        }
                    }
                    catch (Exception e)
                    {
                        StreamWriter fileout = new StreamWriter("error.txt", false);
                        fileout.WriteLine(e.Message);
                        fileout.WriteLine(e.StackTrace);
                        fileout.Close();
                        MouseUI.IgnoreMouseMovement = true;
                        MouseUI.IgnoreMouseClicks = true;
                        Screen.Blank();
                        Screen.WriteString(12, 0,
                            "  An error has occured. See error.txt for more details. Press any key to quit."
                                .PadOuter(Global.SCREEN_W));
                        Input.ReadKey(false);
                        Global.Quit();
                    }

                    MouseUI.PopButtonMap();
                    MouseUI.IgnoreMouseMovement = false;
                    Screen.NoClose = false;
                    Global.SaveOptions();
                    recentdepth = MapView.Map.Depth;
                    recentname = Actor.player_name;
                    recentwin = Global.BOSS_KILLED ? 'W' : '-';
                    recentcause = Global.KILLED_BY;
                    on_highscore_list = false;
                    if (!Global.SAVING)
                    {
                        List<string> newhighscores = new List<string>();
                        int num_scores = 0;
                        bool added = false;
                        if (File.Exists("highscore.txt"))
                        {
                            StreamReader file = new StreamReader("highscore.txt");
                            string s = "";
                            while (s.Length < 2 || s.Substring(0, 2) != "--")
                            {
                                s = file.ReadLine();
                                newhighscores.Add(s);
                            }

                            s = "!!";
                            while (s.Substring(0, 2) != "--")
                            {
                                s = file.ReadLine();
                                if (s.Substring(0, 2) == "--")
                                {
                                    if (!added && num_scores < Global.HIGH_SCORES)
                                    {
                                        char symbol = Global.BOSS_KILLED ? 'W' : '-';
                                        newhighscores.Add(
                                            $"{MapView.Map.Depth} {symbol} {Actor.player_name} -- {Global.KILLED_BY}");
                                        //newhighscores.Add(game.M.current_level.ToString() + " " + symbol + " " + Actor.player_name + " -- " + Global.KILLED_BY);
                                        on_highscore_list = true;
                                    }

                                    newhighscores.Add(s);
                                    break;
                                }

                                if (num_scores < Global.HIGH_SCORES)
                                {
                                    string[] tokens = s.Split(' ');
                                    int dlev = Convert.ToInt32(tokens[0]);
                                    if (dlev < MapView.Map.Depth ||
                                        (dlev == MapView.Map.Depth && Global.BOSS_KILLED))
                                    {
                                        if (!added)
                                        {
                                            char symbol = Global.BOSS_KILLED ? 'W' : '-';
                                            newhighscores.Add(
                                                $"{MapView.Map.Depth} {symbol} {Actor.player_name} -- {Global.KILLED_BY}");
                                            //newhighscores.Add(game.M.current_level.ToString() + " " + symbol + " " + Actor.player_name + " -- " + Global.KILLED_BY);
                                            ++num_scores;
                                            added = true;
                                            on_highscore_list = true;
                                        }

                                        if (num_scores < Global.HIGH_SCORES)
                                        {
                                            newhighscores.Add(s);
                                            ++num_scores;
                                        }
                                    }
                                    else
                                    {
                                        newhighscores.Add(s);
                                        ++num_scores;
                                    }
                                }
                            }

                            file.Close();
                        }
                        else
                        {
                            newhighscores.Add("High scores:");
                            newhighscores.Add("--");
                            char symbol = Global.BOSS_KILLED ? 'W' : '-';
                            newhighscores.Add(
                                $"{MapView.Map.Depth} {symbol} {Actor.player_name} -- {Global.KILLED_BY}");
                            //newhighscores.Add(game.M.current_level.ToString() + " " + symbol + " " + Actor.player_name + " -- " + Global.KILLED_BY);
                            newhighscores.Add("--");
                            on_highscore_list = true;
                        }

                        StreamWriter fileout = new StreamWriter("highscore.txt", false);
                        foreach (string str in newhighscores)
                        {
                            fileout.WriteLine(str);
                        }

                        fileout.Close();
                    }

                    if (!Global.QUITTING && !Global.SAVING)
                    {
                        return NextScene.GameOver;
                    }

                    break;
                }
                case 'b':
                {
                    Help.DisplayHelp();
                    break;
                }
                case 'c':
                {
                    MouseUI.PushButtonMap();
                    Screen.Blank();
                    List<string> scores = new List<string>();
                    {
                        if (!File.Exists("highscore.txt"))
                        {
                            List<string> newhighscores = new List<string>();
                            newhighscores.Add("High scores:");
                            newhighscores.Add("--");
                            newhighscores.Add("--");
                            StreamWriter fileout = new StreamWriter("highscore.txt", false);
                            foreach (string str in newhighscores)
                            {
                                fileout.WriteLine(str);
                            }

                            fileout.Close();
                        }

                        StreamReader file = new StreamReader("highscore.txt");
                        string s = "";
                        while (s.Length < 2 || s.Substring(0, 2) != "--")
                        {
                            s = file.ReadLine();
                        }

                        s = "!!";
                        while (s.Substring(0, 2) != "--")
                        {
                            s = file.ReadLine();
                            if (s.Substring(0, 2) == "--" || scores.Count == Global.HIGH_SCORES)
                            {
                                break;
                            }
                            else
                            {
                                scores.Add(s);
                            }
                        }

                        file.Close();
                    }
                    if (scores.Count == Global.HIGH_SCORES && !on_highscore_list &&
                        recentdepth != -1)
                    {
                        scores.RemoveLast();
                        scores.Add(recentdepth.ToString() + " " + recentwin + " " + recentname +
                                   " -- " +
                                   recentcause);
                    }

                    int longest_name = 0;
                    int longest_cause = 0;
                    foreach (string s in scores)
                    {
                        string[] tokens = s.Split(' ');
                        string name_and_cause_of_death = s.Substring(tokens[0].Length + 3);
                        int idx = name_and_cause_of_death.LastIndexOf(" -- ");
                        string name = name_and_cause_of_death.Substring(0, idx);
                        string cause_of_death = name_and_cause_of_death.Substring(idx + 4);
                        if (name.Length > longest_name)
                        {
                            longest_name = name.Length;
                        }

                        if (cause_of_death.Length > longest_cause)
                        {
                            longest_cause = cause_of_death.Length;
                        }
                    }

                    int total_spaces =
                        Global.SCREEN_W -
                        (longest_name + 4 +
                         longest_cause); //max name length is 26 and max cause length is 42. Depth is the '4'.
                    int half_spaces = total_spaces / 2;
                    int half_spaces_offset = (total_spaces + 1) / 2;
                    int spaces1 = half_spaces / 4;
                    int spaces2 = half_spaces - (half_spaces / 4);
                    int spaces3 = half_spaces_offset - (half_spaces_offset / 4);
                    int name_middle = spaces1 + longest_name / 2;
                    int depth_middle = spaces1 + spaces2 + longest_name + 1;
                    int cause_middle = spaces1 + spaces2 + spaces3 + longest_name + 4 +
                                       (longest_cause - 1) / 2;
                    Color primary = Color.Green;
                    Color recent = Color.Cyan;
                    Screen.WriteString(0, (Global.SCREEN_W - 11) / 2,
                        new ColorString("HIGH SCORES",
                            Color.Yellow)); //"HIGH SCORES" has width 11
                    Screen.WriteString(1, (Global.SCREEN_W - 11) / 2,
                        new ColorString("-----------", Color.Cyan));
                    Screen.WriteString(2, name_middle - 4,
                        new ColorString("Character", primary));
                    Screen.WriteString(2, depth_middle - 2, new ColorString("Depth", primary));
                    Screen.WriteString(2, cause_middle - 6,
                        new ColorString("Cause of death", primary));
                    bool written_recent = false;
                    int line = 3;
                    foreach (string s in scores)
                    {
                        if (line >= Global.SCREEN_H)
                        {
                            break;
                        }

                        string[] tokens = s.Split(' ');
                        int dlev = Convert.ToInt32(tokens[0]);
                        char winning = tokens[1][0];
                        string name_and_cause_of_death = s.Substring(tokens[0].Length + 3);
                        int idx = name_and_cause_of_death.LastIndexOf(" -- ");
                        string name = name_and_cause_of_death.Substring(0, idx);
                        string cause_of_death = name_and_cause_of_death.Substring(idx + 4);
                        string cause_capitalized =
                            cause_of_death.Substring(0, 1).ToUpper() +
                            cause_of_death.Substring(1);
                        Color current_color = Color.White;
                        if (!written_recent && name == recentname && dlev == recentdepth &&
                            winning == recentwin &&
                            cause_of_death == recentcause)
                        {
                            current_color = recent;
                            written_recent = true;
                        }
                        else
                        {
                            current_color = Color.White;
                        }

                        Screen.WriteString(line, spaces1, new ColorString(name, current_color));
                        Screen.WriteString(line, spaces1 + spaces2 + longest_name,
                            new ColorString(dlev.ToString().PadLeft(2), current_color));
                        Screen.WriteString(line, spaces1 + spaces2 + spaces3 + longest_name + 4,
                            new ColorString(cause_capitalized, current_color));
                        if (winning == 'W')
                        {
                            Screen.WriteString(line, spaces1 + spaces2 + longest_name + 3,
                                new ColorString("W", Color.Yellow));
                        }

                        ++line;
                    }

                    Input.ReadKey(false);
                    MouseUI.PopButtonMap();
                    break;
                }
                case 'd':
                    Global.Quit();
                    break;
                default:
                    break;
            }

            return NextScene.None;
        }
    }
}