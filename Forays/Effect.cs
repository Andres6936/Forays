/*Copyright (c) 2014-2016  Derrick Creamer
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/

using System;
using System.Collections.Generic;
using System.Threading;
using PosArrays;
using Utilities;
using Nym;
using Forays.Enums;
using static Nym.NameElement;

namespace Forays
{
    public static class SharedEffect
    {
        private static MessageBuffer B
        {
            get { return Actor.B; }
        } //todo: add collapse (and/or the falling stones effect) and passage here eventually

        private static Map M
        {
            get { return Actor.M; }
        }

        private static Queue Q
        {
            get { return Actor.Q; }
        }

        private static Actor player
        {
            get { return Actor.player; }
        }

        private static int ROWS
        {
            get { return Global.ROWS; }
        }

        private static int COLS
        {
            get { return Global.COLS; }
        }

        public static void ShowKnownItems(Hash<ConsumableType> IDed)
        {
            MouseUI.PushButtonMap();
            UI.draw_bottom_commands = false;
            UI.darken_status_bar = true;
            const int width = 25;
            List<ConsumableType> potion_order = new List<ConsumableType>
            {
                ConsumableType.STONEFORM, ConsumableType.CLOAKING, ConsumableType.VAMPIRISM,
                ConsumableType.HEALING,
                ConsumableType.MYSTIC_MIND, ConsumableType.SILENCE, ConsumableType.REGENERATION,
                ConsumableType.ROOTS,
                ConsumableType.BRUTISH_STRENGTH, ConsumableType.HASTE
            };
            List<ConsumableType> scroll_order = new List<ConsumableType>
            {
                ConsumableType.SUNLIGHT, ConsumableType.DARKNESS, ConsumableType.BLINKING,
                ConsumableType.RENEWAL,
                ConsumableType.FIRE_RING, ConsumableType.CALLING, ConsumableType.KNOWLEDGE,
                ConsumableType.PASSAGE,
                ConsumableType.THUNDERCLAP, ConsumableType.RAGE, ConsumableType.ENCHANTMENT,
                ConsumableType.TIME,
                ConsumableType.TRAP_CLEARING
            };
            List<ConsumableType> orb_order = new List<ConsumableType>
            {
                ConsumableType.BREACHING, ConsumableType.FREEZING, ConsumableType.SHIELDING,
                ConsumableType.BLADES,
                ConsumableType.CONFUSION, ConsumableType.FLAMES, ConsumableType.DETONATION,
                ConsumableType.PAIN,
                ConsumableType.TELEPORTAL, ConsumableType.FOG
            };
            List<ConsumableType> wand_order = new List<ConsumableType>
            {
                ConsumableType.DUST_STORM, ConsumableType.SLUMBER, ConsumableType.TELEKINESIS,
                ConsumableType.REACH,
                ConsumableType.INVISIBILITY, ConsumableType.WEBS, ConsumableType.FLESH_TO_FIRE
            };
            List<ColorBufferString> potions = new List<ColorBufferString>();
            List<ColorBufferString> scrolls = new List<ColorBufferString>();
            List<ColorBufferString> orbs = new List<ColorBufferString>();
            List<ColorBufferString> wands = new List<ColorBufferString>();
            List<List<ColorBufferString>> string_lists = new List<List<ColorBufferString>>
                {potions, scrolls, orbs, wands};
            int list_idx = 0;
            foreach (List<ConsumableType> item_list in new List<List<ConsumableType>>
                {potion_order, scroll_order, orb_order, wand_order})
            {
                int item_idx = 0;
                while (item_idx + 1 < item_list.Count)
                {
                    ConsumableType[] ct = new ConsumableType[2];
                    string[] names = new string[2];
                    Color[] ided_color = new Color[2];
                    for (int i = 0; i < 2; ++i)
                    {
                        ct[i] = item_list[item_idx + i];
                        names[i] = ct[i].ToString()[0] + ct[i].ToString().Substring(1).ToLower();
                        names[i] = names[i].Replace('_', ' ');
                        if (IDed[ct[i]])
                        {
                            ided_color[i] = Color.Cyan;
                        }
                        else
                        {
                            ided_color[i] = Color.DarkGray;
                        }
                    }

                    int num_spaces = width - (names[0].Length + names[1].Length);
                    string_lists[list_idx].Add(new ColorBufferString(names[0], ided_color[0],
                        "".PadRight(num_spaces),
                        Color.Black, names[1], ided_color[1]));
                    item_idx += 2;
                }

                if (item_list.Count % 2 == 1)
                {
                    ConsumableType ct = item_list.Last();
                    string n =
                        (ct.ToString()[0] + ct.ToString().Substring(1).ToLower()).Replace('_', ' ');
                    //name = name[i].Replace('_',' ');
                    Color ided_color = Color.DarkGray;
                    if (IDed[ct])
                    {
                        ided_color = Color.Cyan;
                    }

                    int num_spaces = width - n.Length;
                    string_lists[list_idx]
                        .Add(new ColorBufferString(n, ided_color, "".PadRight(num_spaces),
                            Color.Black));
                }

                ++list_idx;
            }

            Screen.WriteMapString(0, 0, "".PadRight(COLS, '-'));
            for (int i = 1; i < ROWS + 2; ++i)
            {
                Screen.WriteMapString(i, 0, "".PadToMapSize());
            }

            Screen.WriteMapString(ROWS + 2, 0, "".PadRight(COLS, '-'));
            const Color label_color = Color.Yellow;
            const int first_column_offset = 2;
            const int second_column_offset = first_column_offset + 35;
            const int first_item_row = 4;
            Screen.WriteMapString(first_item_row - 1, 8 + first_column_offset, "- Potions -",
                label_color);
            Screen.WriteMapString(first_item_row - 1, 7 + second_column_offset, "- Scrolls -",
                label_color);
            int line = first_item_row;
            foreach (ColorBufferString s in potions)
            {
                Screen.WriteMapString(line, first_column_offset, s);
                ++line;
            }

            line = first_item_row;
            foreach (ColorBufferString s in scrolls)
            {
                Screen.WriteMapString(line, second_column_offset, s);
                ++line;
            }

            const int second_item_row = first_item_row + 11;
            Screen.WriteMapString(second_item_row - 1, 9 + first_column_offset, "- Orbs -",
                label_color);
            Screen.WriteMapString(second_item_row - 1, 8 + second_column_offset, "- Wands -",
                label_color);
            line = second_item_row;
            foreach (ColorBufferString s in orbs)
            {
                Screen.WriteMapString(line, first_column_offset, s);
                ++line;
            }

            line = second_item_row;
            foreach (ColorBufferString s in wands)
            {
                Screen.WriteMapString(line, second_column_offset, s);
                ++line;
            }

            UI.Display("Discovered item types: ");
            InputKey.ReadKey();
            MouseUI.PopButtonMap();
            UI.draw_bottom_commands = true;
            UI.darken_status_bar = false;
        }

        public static void ShowPreviousMessages(bool show_footsteps)
        {
            const int text_height = Global.ROWS + 1;
            List<string> messages = B.GetMessageLog();
            MouseUI.PushButtonMap(MouseMode.ScrollableMenu);
            UI.draw_bottom_commands = false;
            UI.darken_status_bar = true;
            MouseUI.CreateMapButton(ConsoleKey.OemMinus, false, 0, 1);
            MouseUI.CreateMapButton(ConsoleKey.OemPlus, false, text_height + 1, 1);
            Screen.WriteMapString(0, 0, "".PadRight(COLS, '-'));
            Screen.WriteMapString(text_height + 1, 0, "".PadRight(COLS, '-'));
            ConsoleKeyInfo command;
            char ch;
            int startline = Math.Max(0, messages.Count - text_height);
            for (bool done = false; !done;)
            {
                if (startline > 0)
                {
                    Screen.WriteMapString(0, COLS - 3,
                        new ColorBufferString("[", Color.Yellow, "-", Color.Cyan, "]",
                            Color.Yellow));
                }
                else
                {
                    Screen.WriteMapString(0, COLS - 3, "---");
                }

                bool more = false;
                if (startline + text_height < messages.Count)
                {
                    more = true;
                }

                if (more)
                {
                    Screen.WriteMapString(text_height + 1, COLS - 3,
                        new ColorBufferString("[", Color.Yellow, "+", Color.Cyan, "]",
                            Color.Yellow));
                }
                else
                {
                    Screen.WriteMapString(text_height + 1, COLS - 3, "---");
                }

                for (int i = 1; i <= text_height; ++i)
                {
                    if (messages.Count - startline < i)
                    {
                        Screen.WriteMapString(i, 0, "".PadToMapSize());
                    }
                    else
                    {
                        Screen.WriteMapString(i, 0, messages[i + startline - 1].PadToMapSize());
                    }
                }

                UI.Display("Previous messages: ");
                command = InputKey.ReadKey();
                ConsoleKey ck = command.Key;
                switch (ck)
                {
                    case ConsoleKey.Backspace:
                    case ConsoleKey.PageUp:
                    case ConsoleKey.NumPad9:
                        ch = (char) 8;
                        break;
                    case ConsoleKey.Enter:
                        ch = ' '; //hackery ahoy - enter becomes space and pagedown becomes enter.
                        break;
                    case ConsoleKey.PageDown:
                    case ConsoleKey.NumPad3:
                        ch = (char) 13;
                        break;
                    case ConsoleKey.Home:
                    case ConsoleKey.NumPad7:
                        ch = '[';
                        break;
                    case ConsoleKey.End:
                    case ConsoleKey.NumPad1:
                        ch = ']';
                        break;
                    default:
                        ch = command.GetCommandChar();
                        break;
                }

                switch (ch)
                {
                    case ' ':
                    case (char) 27:
                        done = true;
                        break;
                    case '8':
                    case '-':
                    case '_':
                        if (startline > 0)
                        {
                            --startline;
                        }

                        break;
                    case '2':
                    case '+':
                    case '=':
                        if (more)
                        {
                            ++startline;
                        }

                        break;
                    case (char) 8:
                        if (startline > 0)
                        {
                            startline -= text_height;
                            if (startline < 0)
                            {
                                startline = 0;
                            }
                        }

                        break;
                    case (char) 13:
                        if (messages.Count > text_height)
                        {
                            startline += text_height;
                            if (startline + text_height > messages.Count)
                            {
                                startline = messages.Count - text_height;
                            }
                        }

                        break;
                    case '[':
                        startline = 0;
                        break;
                    case ']':
                        startline = Math.Max(0, messages.Count - text_height);
                        break;
                    default:
                        break;
                }
            }

            if (show_footsteps && player.HasAttr(AttrType.DETECTING_MOVEMENT) &&
                Actor.previous_footsteps.Count > 0)
            {
                M.Draw();
                Screen.AnimateMapCells(Actor.previous_footsteps, new ColorChar('!', Color.Red),
                    150);
            }

            MouseUI.PopButtonMap();
            UI.draw_bottom_commands = true;
            UI.darken_status_bar = false;
        }

        public static bool Telekinesis(bool cast, Actor user, Tile t)
        {
            bool wand = !cast;
            if (t == null)
            {
                return false;
            }

            if (t != null)
            {
                if (wand && user == player && !Item.identified[ConsumableType.TELEKINESIS])
                {
                    Item.identified[ConsumableType.TELEKINESIS] = true;
                    B.Add("(It was a wand of telekinesis!) ");
                    B.Print(true);
                }

                List<Tile> ai_line = null;
                if (user != player && t == player.tile())
                {
                    var fires = user.TilesWithinDistance(12).Where(x =>
                        x.passable && x.actor() == null && x.Is(FeatureType.FIRE) &&
                        user.CanSee(x) &&
                        player.HasBresenhamLineWithCondition(x, false,
                            y => y.passable && y.actor() == null));
                    if (fires.Count > 0)
                    {
                        ai_line = player.GetBestExtendedLineOfEffect(fires.Random());
                    }
                    else
                    {
                        if (wand)
                        {
                            ai_line = player.GetBestExtendedLineOfEffect(user);
                        }
                        else
                        {
                            ai_line = player.GetBestExtendedLineOfEffect(
                                player.TileInDirection(Global.RandomDirection()));
                        }
                    }
                }

                Actor a = t.actor();
                if (a == null && t.inv == null && !t.Is(FeatureType.GRENADE) &&
                    t.Is(FeatureType.TROLL_CORPSE, FeatureType.TROLL_BLOODWITCH_CORPSE))
                {
                    ActorType troll_type =
                        t.Is(FeatureType.TROLL_CORPSE)
                            ? ActorType.TROLL
                            : ActorType.TROLL_BLOODWITCH;
                    FeatureType troll_corpse = t.Is(FeatureType.TROLL_CORPSE)
                        ? FeatureType.TROLL_CORPSE
                        : FeatureType.TROLL_BLOODWITCH_CORPSE;
                    Event troll_event = Q.FindTargetedEvent(t, EventType.REGENERATING_FROM_DEATH);
                    troll_event.dead = true;
                    Actor troll = Actor.Create(troll_type, t.row, t.col);
                    foreach (Event e in Q.list)
                    {
                        if (e.target == troll && e.type == EventType.MOVE)
                        {
                            e.tiebreaker = troll_event.tiebreaker;
                            e.dead = true;
                            break;
                        }
                    }

                    Actor.tiebreakers[troll_event.tiebreaker] = troll;
                    troll.symbol = '%';
                    troll.attrs[AttrType.CORPSE] = 1;
                    troll.Name = new Name(troll.GetName(Possessive) + " corpse");
                    troll.curhp = troll_event.value;
                    troll.attrs[AttrType.PERMANENT_DAMAGE] = troll_event.secondary_value;
                    troll.attrs[AttrType.NO_ITEM]++;
                    t.features.Remove(troll_corpse);
                    a = troll;
                }

                if (a != null)
                {
                    string msg = "Throw " + a.GetName(true, The) + " in which direction? ";
                    if (a == player)
                    {
                        msg = "Throw yourself in which direction? ";
                    }

                    List<Tile> line = null;
                    if (user == player)
                    {
                        TargetInfo info = a.GetTarget(false, 12, 0, false, true, false, msg);
                        if (info != null)
                        {
                            line = info.extended_line;
                        }
                    }
                    else
                    {
                        line = ai_line;
                    }

                    if (line != null)
                    {
                        if (line.Count == 1 && line[0].actor() == user)
                        {
                            if (wand)
                            {
                                return true;
                            }

                            return false;
                        }

                        if (cast)
                        {
                            B.Add(user.GetName(false, The, Verb("cast")) + " telekinesis. ", user);
                            user.MakeNoise(6); //should match spellVolume, hack
                            if (a.type == ActorType.ALASI_BATTLEMAGE &&
                                !a.HasSpell(SpellType.TELEKINESIS))
                            {
                                a.curmp += Spell.Tier(SpellType.TELEKINESIS);
                                if (a.curmp > a.maxmp)
                                {
                                    a.curmp = a.maxmp;
                                }

                                a.GainSpell(SpellType.TELEKINESIS);
                                B.Add(
                                    "Runes on " + a.GetName(false, The, Possessive) +
                                    " armor align themselves with the spell. ", a);
                            }
                        }

                        if (a == user && a == player)
                        {
                            B.Add("You throw yourself forward. ");
                        }
                        else
                        {
                            if (line.Count == 1)
                            {
                                B.Add(
                                    user.GetName(true, The, Verb("throw")) + " " +
                                    a.GetName(true, The) +
                                    " into the ceiling. ", user, a);
                            }
                            else
                            {
                                B.Add(
                                    user.GetName(true, The, Verb("throw")) + " " +
                                    a.GetName(true, The) + ". ", user,
                                    a);
                            }
                        }

                        B.DisplayContents();
                        user.attrs[AttrType.SELF_TK_NO_DAMAGE] = 1;
                        a.attrs[AttrType.TELEKINETICALLY_THROWN] = 1;
                        a.attrs[AttrType.TURN_INTO_CORPSE]++;
                        if (line.Count == 1)
                        {
                            a.TakeDamage(DamageType.NORMAL, DamageClass.PHYSICAL, R.Roll(3, 6),
                                user,
                                "colliding with the ceiling");
                            a.CollideWith(a.tile());
                        }
                        else
                        {
                            a.tile().KnockObjectBack(a, line, 12, user);
                        }

                        a.attrs[AttrType.TELEKINETICALLY_THROWN] = 0;
                        user.attrs[AttrType.SELF_TK_NO_DAMAGE] = 0;
                        if (a.curhp <= 0 && a.HasAttr(AttrType.REGENERATES_FROM_DEATH))
                        {
                            a.attrs[AttrType.TURN_INTO_CORPSE] = 0;
                            a.attrs[AttrType.CORPSE] = 0;
                            a.attrs[AttrType.FROZEN] = 0;
                            a.attrs[AttrType.INVULNERABLE] = 0;
                            a.attrs[AttrType.SHIELDED] = 0;
                            a.attrs[AttrType.BLOCKING] = 0;
                            a.curhp =
                                1; //this is all pretty hacky - perhaps I should relocate the regenerating corpse through other means.
                            a.TakeDamage(DamageType.NORMAL, DamageClass.NO_TYPE, 500, null);
                        }
                        else
                        {
                            a.CorpseCleanup();
                        }
                    }
                    else
                    {
                        if (wand)
                        {
                            return true;
                        }

                        return false;
                    }
                }
                else
                {
                    bool blast_fungus = false;
                    if (t.type == TileType.BLAST_FUNGUS && !t.Is(FeatureType.GRENADE,
                            FeatureType.WEB,
                            FeatureType.FORASECT_EGG, FeatureType.BONES))
                    {
                        blast_fungus = true;
                    }

                    if (t.inv != null || blast_fungus)
                    {
                        Item i = t.inv;
                        string itemname = "";
                        if (blast_fungus)
                        {
                            itemname = "the blast fungus";
                        }
                        else
                        {
                            itemname = i.GetName(true, The, Extra);
                        }

                        string msg = "Throw " + itemname + " in which direction? ";
                        List<Tile> line = null;
                        if (user == player)
                        {
                            TargetInfo info = t.GetTarget(false, 12, 0, false, true, false, msg);
                            if (info != null)
                            {
                                line = info.extended_line;
                            }
                        }
                        else
                        {
                            line = ai_line;
                        }

                        if (line != null)
                        {
                            if (line.Count > 13)
                            {
                                line = line.ToCount(13); //for range 12
                            }

                            if (cast)
                            {
                                B.Add(user.GetName(false, The, Verb("cast")) + " telekinesis. ",
                                    user);
                                user.MakeNoise(6); //should match spellVolume
                            }

                            if (blast_fungus)
                            {
                                B.Add("The blast fungus is pulled from the floor. ", t);
                                B.Add("Its fuse ignites! ", t);
                                t.Toggle(null);
                                i = Item.Create(ConsumableType.BLAST_FUNGUS, t.row, t.col);
                                if (i != null)
                                {
                                    i.other_data = 3;
                                    i.revealed_by_light = true;
                                    Q.Add(new Event(i, 100, EventType.BLAST_FUNGUS));
                                    Screen.AnimateMapCell(t.row, t.col,
                                        new ColorChar('3', Color.Red), 100);
                                }
                            }

                            if (line.Count == 1)
                            {
                                B.Add(
                                    user.GetName(true, The, Verb("throw")) + " " + itemname +
                                    " into the ceiling. ",
                                    user, t);
                            }
                            else
                            {
                                B.Add(
                                    user.GetName(true, The, Verb("throw")) + " " + itemname + ". ",
                                    user, t);
                            }

                            B.DisplayContents();
                            if (i.quantity > 1)
                            {
                                i.quantity--;
                                bool revealed = i.revealed_by_light;
                                i = new Item(i, -1, -1);
                                i.revealed_by_light = revealed;
                            }
                            else
                            {
                                t.inv = null;
                                Screen.WriteMapChar(t.row, t.col, M.VisibleColorChar(t.row, t.col));
                            }

                            bool trigger_traps = false;
                            Tile t2 = line.LastBeforeSolidTile();
                            Actor first = user.FirstActorInLine(line);
                            if (t2 == line.LastOrDefault() && first == null)
                            {
                                trigger_traps = true;
                            }

                            if (first != null)
                            {
                                t2 = first.tile();
                            }

                            line = line.ToFirstSolidTileOrActor();
                            //if(line.Count > 0){
                            //	line.RemoveAt(line.Count - 1);
                            //}
                            if (line.Count > 0)
                            {
                                line.RemoveAt(line.Count - 1);
                            }

                            {
                                Tile first_unseen = null;
                                foreach (Tile tile2 in line)
                                {
                                    if (!tile2.seen)
                                    {
                                        first_unseen = tile2;
                                        break;
                                    }
                                }

                                if (first_unseen != null)
                                {
                                    line = line.To(first_unseen);
                                    if (line.Count > 0)
                                    {
                                        line.RemoveAt(line.Count - 1);
                                    }
                                }
                            }
                            if (line.Count > 0)
                            {
                                //or > 1 ?
                                user.AnimateProjectile(line, i.symbol, i.color);
                            }

                            if (first == user)
                            {
                                B.Add(user.GetName(false, The, Verb("catch")) + " it! ", user);
                                if (user.inv.Count < Global.MAX_INVENTORY_SIZE)
                                {
                                    user.GetItem(i);
                                }
                                else
                                {
                                    B.Add("Your pack is too full to fit anything else. ");
                                    i.ignored = true;
                                    user.tile().GetItem(i);
                                }
                            }
                            else
                            {
                                if (first != null)
                                {
                                    B.Add("It hits " + first.GetName(The) + ". ", first);
                                }

                                if (i.IsBreakable())
                                {
                                    B.Add($"{i.GetName(true, The, Extra, Verb("break"))}! ", t2);
                                    if (i.ItemClass == ConsumableClass.ORB)
                                    {
                                        i.Use(null, new List<Tile> {t2});
                                    }
                                    else
                                    {
                                        i.CheckForMimic();
                                    }

                                    t2.MakeNoise(4);
                                }
                                else
                                {
                                    t2.GetItem(i);
                                }

                                t2.MakeNoise(2);
                            }

                            if (first != null && first != user && first != player)
                            {
                                first.player_visibility_duration = -1;
                                first.attrs[AttrType.PLAYER_NOTICED]++;
                            }
                            else
                            {
                                if (trigger_traps && t2.IsTrap())
                                {
                                    t2.TriggerTrap();
                                }
                            }
                        }
                        else
                        {
                            if (wand)
                            {
                                return true;
                            }

                            return false;
                        }
                    }
                    else
                    {
                        if (!t.Is(FeatureType.GRENADE) &&
                            (t.Is(TileType.DOOR_C, TileType.DOOR_O, TileType.POISON_BULB) || t.Is(
                                 FeatureType.WEB,
                                 FeatureType.FORASECT_EGG, FeatureType.BONES)))
                        {
                            if (cast)
                            {
                                B.Add(user.GetName(false, The, Verb("cast")) + " telekinesis. ",
                                    user);
                                user.MakeNoise(6); //should match spellVolume
                            }

                            if (t.Is(TileType.DOOR_C))
                            {
                                B.Add("The door slams open. ", t);
                                t.Toggle(null);
                            }
                            else
                            {
                                if (t.Is(TileType.DOOR_O))
                                {
                                    B.Add("The door slams open. ", t);
                                    t.Toggle(null);
                                }
                            }

                            if (t.Is(TileType.POISON_BULB))
                            {
                                t.Bump(0);
                            }

                            if (t.Is(FeatureType.WEB))
                            {
                                B.Add("The web is destroyed. ", t);
                                t.RemoveFeature(FeatureType.WEB);
                            }

                            if (t.Is(FeatureType.FORASECT_EGG))
                            {
                                B.Add("The egg is destroyed. ", t);
                                t.RemoveFeature(FeatureType.FORASECT_EGG); //todo: forasect pathing?
                            }

                            if (t.Is(FeatureType.BONES))
                            {
                                B.Add("The bones are scattered. ", t);
                                t.RemoveFeature(FeatureType.BONES);
                            }
                        }
                        else
                        {
                            bool grenade = t.Is(FeatureType.GRENADE);
                            bool barrel = t.Is(TileType.BARREL);
                            bool flaming_barrel = barrel && t.IsBurning();
                            bool torch = t.Is(TileType.STANDING_TORCH);
                            string feature_name = "";
                            int impact_damage_dice = 3;
                            ColorChar vis = new ColorChar(t.symbol, t.color);
                            switch (t.type)
                            {
                                case TileType.CRACKED_WALL:
                                    feature_name = "cracked wall";
                                    break;
                                case TileType.RUBBLE:
                                    feature_name = "pile of rubble";
                                    break;
                                case TileType.STATUE:
                                    feature_name = "statue";
                                    break;
                            }

                            if (grenade)
                            {
                                impact_damage_dice = 0;
                                feature_name = "grenade";
                                vis.c = ',';
                                vis.color = Color.Red;
                            }

                            if (flaming_barrel)
                            {
                                feature_name = "flaming barrel of oil";
                            }

                            if (barrel)
                            {
                                feature_name = "barrel";
                            }

                            if (torch)
                            {
                                feature_name = "torch";
                            }

                            if (feature_name == "")
                            {
                                if (wand)
                                {
                                    if (user == player)
                                    {
                                        B.Add("The wand grabs at empty space. ", t);
                                    }

                                    return true;
                                }

                                return false;
                            }

                            string msg = "Throw the " + feature_name + " in which direction? ";
                            bool passable_hack = !t.passable;
                            if (passable_hack)
                            {
                                t.passable = true;
                            }

                            List<Tile> line = null;
                            if (user == player)
                            {
                                TargetInfo info = t.GetTarget(false, 12, 0, false, true, false,
                                    msg);
                                if (info != null)
                                {
                                    line = info.extended_line;
                                }
                            }
                            else
                            {
                                line = ai_line;
                            }

                            if (passable_hack)
                            {
                                t.passable = false;
                            }

                            if (line != null)
                            {
                                if (cast)
                                {
                                    B.Add(user.GetName(false, The, Verb("cast")) + " telekinesis. ",
                                        user);
                                    user.MakeNoise(6); //should match spellVolume
                                }

                                if (line.Count == 1)
                                {
                                    B.Add(
                                        user.GetName(true, The, Verb("throw")) + " the " +
                                        feature_name +
                                        " into the ceiling. ", user, t);
                                }
                                else
                                {
                                    B.Add(
                                        user.GetName(true, The, Verb("throw")) + " the " +
                                        feature_name + ". ", user,
                                        t);
                                }

                                B.DisplayContents();
                                user.attrs[AttrType.SELF_TK_NO_DAMAGE] = 1;
                                switch (t.type)
                                {
                                    case TileType.CRACKED_WALL:
                                    case TileType.RUBBLE:
                                    case TileType.STATUE:
                                    case TileType.BARREL:
                                    case TileType.STANDING_TORCH:
                                        if (flaming_barrel)
                                        {
                                            t.RemoveFeature(FeatureType.FIRE);
                                        }

                                        t.Toggle(null, TileType.FLOOR);
                                        foreach (Tile neighbor in t.TilesAtDistance(1))
                                        {
                                            neighbor.solid_rock = false;
                                        }

                                        break;
                                }

                                if (grenade)
                                {
                                    t.RemoveFeature(FeatureType.GRENADE);
                                    Event e = Q.FindTargetedEvent(t, EventType.GRENADE);
                                    if (e != null)
                                    {
                                        e.dead = true;
                                    }
                                }

                                Screen.WriteMapChar(t.row, t.col, M.VisibleColorChar(t.row, t.col));
                                ColorChar[,] mem = Screen.GetCurrentMap();
                                int current_row = t.row;
                                int current_col = t.col;
                                //
                                int knockback_strength = 12;
                                if (line.Count == 1)
                                {
                                    knockback_strength = 0;
                                }

                                int i = 0;
                                while (true)
                                {
                                    Tile t2 = line[i];
                                    if (t2 == t)
                                    {
                                        break;
                                    }

                                    ++i;
                                }

                                line.RemoveRange(0, i + 1);
                                while (knockback_strength > 0)
                                {
                                    //if the knockback strength is greater than 1, you're passing *over* at least one tile.
                                    Tile t2 = line[0];
                                    line.RemoveAt(0);
                                    if (!t2.passable)
                                    {
                                        if (t2.Is(TileType.CRACKED_WALL, TileType.DOOR_C,
                                                TileType.HIDDEN_DOOR) &&
                                            impact_damage_dice > 0)
                                        {
                                            string tilename = t2.GetName(true, The);
                                            if (t2.type == TileType.HIDDEN_DOOR)
                                            {
                                                tilename = "a hidden door";
                                                t2.Toggle(null);
                                            }

                                            B.Add(
                                                "The " + feature_name + " flies into " + tilename +
                                                ". ", t2);
                                            t2.Toggle(null);
                                            Screen.WriteMapChar(current_row, current_col,
                                                mem[current_row, current_col]);
                                        }
                                        else
                                        {
                                            B.Add(
                                                "The " + feature_name + " flies into " +
                                                t2.GetName(true, The) + ". ",
                                                t2);
                                            if (impact_damage_dice > 0)
                                            {
                                                t2.Bump(M.tile[current_row, current_col]
                                                    .DirectionOf(t2));
                                            }

                                            Screen.WriteMapChar(current_row, current_col,
                                                mem[current_row, current_col]);
                                        }

                                        knockback_strength = 0;
                                        break;
                                    }
                                    else
                                    {
                                        if (t2.actor() != null)
                                        {
                                            B.Add(
                                                "The " + feature_name + " flies into " +
                                                t2.actor().GetName(true, The) +
                                                ". ", t2);
                                            if (t2.actor().type != ActorType.SPORE_POD &&
                                                !t2.actor().HasAttr(AttrType.SELF_TK_NO_DAMAGE))
                                            {
                                                t2.actor().TakeDamage(DamageType.NORMAL,
                                                    DamageClass.PHYSICAL,
                                                    R.Roll(impact_damage_dice, 6), user,
                                                    "colliding with a " + feature_name);
                                            }

                                            knockback_strength = 0;
                                            Screen.WriteMapChar(t2.row, t2.col, vis);
                                            Screen.WriteMapChar(current_row, current_col,
                                                mem[current_row, current_col]);
                                            current_row = t2.row;
                                            current_col = t2.col;
                                            break;
                                        }
                                        else
                                        {
                                            if (t2.Is(FeatureType.WEB))
                                            {
                                                //unless perhaps grenades should get stuck and explode in the web?
                                                t2.RemoveFeature(FeatureType.WEB);
                                            }

                                            Screen.WriteMapChar(t2.row, t2.col, vis);
                                            Screen.WriteMapChar(current_row, current_col,
                                                mem[current_row, current_col]);
                                            current_row = t2.row;
                                            current_col = t2.col;
                                            Screen.GLUpdate();
                                            Thread.Sleep(20);
                                        }
                                    }

                                    //M.Draw();
                                    knockback_strength--;
                                }

                                Tile current = M.tile[current_row, current_col];
                                if (grenade)
                                {
                                    B.Add("The grenade explodes! ", current);
                                    current.ApplyExplosion(1, user, "an exploding grenade");
                                }

                                if (barrel)
                                {
                                    B.Add("The barrel smashes! ", current);
                                    List<Tile> cone = current.TilesWithinDistance(1)
                                        .Where(x => x.passable);
                                    List<Tile> added = new List<Tile>();
                                    foreach (Tile t3 in cone)
                                    {
                                        foreach (int dir in U.FourDirections)
                                        {
                                            if (R.CoinFlip() && t3.TileInDirection(dir).passable)
                                            {
                                                added.AddUnique(t3.TileInDirection(dir));
                                            }
                                        }
                                    }

                                    cone.AddRange(added);
                                    foreach (Tile t3 in cone)
                                    {
                                        t3.AddFeature(FeatureType.OIL);
                                        if (t3.actor() != null &&
                                            !t3.actor().HasAttr(AttrType.OIL_COVERED,
                                                AttrType.SLIMED))
                                        {
                                            if (t3.actor().IsBurning())
                                            {
                                                t3.actor().ApplyBurning();
                                            }
                                            else
                                            {
                                                t3.actor().attrs[AttrType.OIL_COVERED] = 1;
                                                B.Add(
                                                    t3.actor().GetName(false, The, Are) +
                                                    " covered in oil. ",
                                                    t3.actor());
                                                if (t3.actor() == player)
                                                {
                                                    Help.TutorialTip(TutorialTopic.Oiled);
                                                }
                                            }
                                        }
                                    }

                                    if (flaming_barrel)
                                    {
                                        current.ApplyEffect(DamageType.FIRE);
                                    }
                                }

                                if (torch)
                                {
                                    current.AddFeature(FeatureType.FIRE);
                                }

                                user.attrs[AttrType.SELF_TK_NO_DAMAGE] = 0;
                            }
                            else
                            {
                                if (wand)
                                {
                                    return true;
                                }

                                return false;
                            }
                        }
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public static void DebugDisplayPositions(IEnumerable<pos> list)
        {
            var temp = Screen.GetCurrentScreen();
            foreach (pos p in list)
            {
                Screen.WriteMapChar(p.row, p.col, '!', Color.Red);
            }

            InputKey.ReadKey();
            Screen.WriteArray(0, 0, temp);
        }

        public static void DebugDisplayDijkstra(PosArray<int> d)
        {
            DebugDisplayDijkstra(d, 1);
        }

        public static void DebugDisplayDijkstra(PosArray<int> d, int default_cost)
        {
            int h = d.objs.GetLength(0);
            int w = d.objs.GetLength(1);
            for (int i = 0; i < h; ++i)
            {
                for (int j = 0; j < w; ++j)
                {
                    if (d[i, j] == U.DijkstraMax)
                    {
                        Screen.WriteMapChar(i, j, '!', Color.DarkGray);
                    }
                    else
                    {
                        if (d[i, j] == U.DijkstraMin)
                        {
                            Screen.WriteMapChar(i, j, '#', Color.Gray);
                        }
                        else
                        {
                            if (d[i, j] == 0)
                            {
                                Screen.WriteMapChar(i, j, '0', Color.White);
                            }
                            else
                            {
                                int cost = d[i, j] / default_cost;
                                if (cost < 10)
                                {
                                    if (cost < 0)
                                    {
                                        if (cost < -26)
                                        {
                                            Screen.WriteMapChar(i, j, '-', Color.DarkRed);
                                        }
                                        else
                                        {
                                            char ch = (char) (Math.Abs(cost) + (int) ('A') - 1);
                                            Screen.WriteMapChar(i, j, ch, Color.Red);
                                        }
                                    }
                                    else
                                    {
                                        Screen.WriteMapChar(i, j, cost.ToString()[0], Color.Cyan);
                                    }
                                }
                                else
                                {
                                    Screen.WriteMapChar(i, j, '+', Color.Blue);
                                }
                            }
                        }
                    }
                }
            }

            InputKey.ReadKey();
        }
    }
}