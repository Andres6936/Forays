using System.IO;
using System.Collections.Generic;
using Forays.Enums;
using PosArrays;

namespace Forays.Loader
{
    public sealed class BinaryLoader : Loader
    {
        // Member

        Dictionary<int, PhysicalObject> identificator = new Dictionary<int, PhysicalObject>();

        // Construct

        public BinaryLoader(string filename)
        {
            using (var reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                NamePlayer = reader.ReadString();
                CurrentLevelId = reader.ReadInt32();

                var numberLevelTypes = reader.ReadInt32();
                for (var index = 0; index < numberLevelTypes; index += 1)
                {
                    LevelTypes.Add((LevelType) reader.ReadInt32());
                }

                WizLite = reader.ReadBoolean();
                WizDark = reader.ReadBoolean();

                for (var row = 0; row < Global.ROWS; row += 1)
                {
                    for (var col = 0; col < Global.COLS; col += 1)
                    {
                        LastCharacterSeen[row, col] = reader.ReadChar();
                        LastColorCharacterSeen[row, col] = (Color) reader.ReadInt32();
                        LastBackgroundColorCharacterSeen[row, col] = (Color) reader.ReadInt32();
                    }
                }

                if (LevelTypes[CurrentLevelId] == LevelType.Final)
                {
                    for (var index = 0; index < 5; index += 1)
                    {
                        FinalLevelCultistCount[index] = reader.ReadInt32();
                    }

                    FinalLevelDemonCount = reader.ReadInt32();
                    FinalLevelClock = reader.ReadInt32();
                }

                var numberFeatList = reader.ReadInt32();
                for (var index = 0; index < numberFeatList; index += 1)
                {
                    FeatTypes.Add((FeatType) reader.ReadInt32());
                }

                // 13. Number Spell List (Int32)
                var numberSpellList = reader.ReadInt32();
                for (var index = 0; index < numberSpellList; index += 1)
                {
                    // 13.1 Spell Type (Int32) -> Depend of 13
                    SpellTypes.Add((SpellType) reader.ReadInt32());
                }

                // 14. Num Actor TieBreakers (Int32)
                var numberTiebreakers = reader.ReadInt32();
                for (var index = 0; index < numberTiebreakers; index += 1)
                {
                    // 14.1 Identification (Int32)
                    var identification = reader.ReadInt32();

                    if (identification == 0)
                    {
                        continue;
                    }

                    var actor = new Actor
                    {
                        // 14.2 Row Position (Int32)
                        row = reader.ReadInt32(),
                        // 14.3 Col Position (Int32)
                        col = reader.ReadInt32(),
                        symbol = reader.ReadChar(),
                        color = (Color) reader.ReadInt32(),
                        type = (ActorType) reader.ReadInt32(),
                        maxhp = reader.ReadInt32(),
                        curhp = reader.ReadInt32(),
                        maxmp = reader.ReadInt32(),
                        curmp = reader.ReadInt32(),
                        speed = reader.ReadInt32(),
                        // 14.12 Light Radius (Int32)
                        light_radius = reader.ReadInt32()
                    };

                    // 14.13 Target ID (Int32)
                    // Discard for the moment
                    var targetId = reader.ReadInt32();

                    // 14.14 Number Items (Int32)
                    var numberItems = reader.ReadInt32();
                    for (var indexItem = 0; indexItem < numberItems; indexItem += 1)
                    {
                        // 14.14.1 Identification Item (Int32)
                        var itemIdentification = reader.ReadInt32();

                        if (itemIdentification == 0)
                        {
                            continue;
                        }

                        var item = new Item
                        {
                            row = reader.ReadInt32(),
                            col = reader.ReadInt32(),
                            symbol = reader.ReadChar(),
                            color = (Color) reader.ReadInt32(),
                            light_radius = reader.ReadInt32(),
                            type = (ConsumableType) reader.ReadInt32(),
                            quantity = reader.ReadInt32(),
                            charges = reader.ReadInt32(),
                            other_data = reader.ReadInt32(),
                            ignored = reader.ReadBoolean(),
                            do_not_stack = reader.ReadBoolean(),
                            // 14.14.13 Revealed By Light (Boolean)
                            revealed_by_light = reader.ReadBoolean()
                        };

                        actor.inv.Add(item);
                    }

                    // 14.15 Number Attributes (Int32)
                    var numberAttributes = reader.ReadInt32();
                    for (var attribute = 0; attribute < numberAttributes; attribute += 1)
                    {
                        var attributeType = (AttrType) reader.ReadInt32();
                        actor.attrs[attributeType] = reader.ReadInt32();
                    }

                    var numberSkills = reader.ReadInt32();
                    for (var indexSkill = 0; indexSkill < numberSkills; indexSkill += 1)
                    {
                        var skillType = (SkillType) reader.ReadInt32();
                        actor.skills[skillType] = reader.ReadInt32();
                    }

                    var numberFeats = reader.ReadInt32();
                    for (var indexFeat = 0; indexFeat < numberFeats; indexFeat += 1)
                    {
                        var featType = (FeatType) reader.ReadInt32();
                        actor.feats[featType] = reader.ReadBoolean();
                    }

                    var numberSpells = reader.ReadInt32();
                    for (var indexSpell = 0; indexSpell < numberSpells; indexSpell += 1)
                    {
                        var spellType = (SpellType) reader.ReadInt32();
                        actor.spells[spellType] = reader.ReadBoolean();
                    }

                    // 14.19 Exhaustion (Int32)
                    actor.exhaustion = reader.ReadInt32();
                    // 14.20 Time Last Action (Int32)
                    actor.time_of_last_action = reader.ReadInt32();
                    // 14.21 Recover Time (Int32)
                    actor.recover_time = reader.ReadInt32();

                    // 14.22 Path Count (Int32)
                    var pathCount = reader.ReadInt32();
                    for (var indexPath = 0; indexPath < pathCount; indexPath += 1)
                    {
                        var pathRow = reader.ReadInt32();
                        var pathCol = reader.ReadInt32();
                        actor.path.Add(new pos(pathRow, pathCol));
                    }

                    // 14.23 Location ID (Int32)
                    // Ignored for the moment
                    var locationId = reader.ReadInt32();

                    // 14.24 Player Visibility Duration (Int32)
                    actor.player_visibility_duration = reader.ReadInt32();

                    // 14.25 Number Weapons (Int32)
                    var numberWeapons = reader.ReadInt32();
                    for (var indexWeapon = 0; indexWeapon < numberWeapons; indexWeapon += 1)
                    {
                        var weapon = new Weapon(WeaponType.NO_WEAPON)
                        {
                            // 14.25.1 Weapon Type
                            type = (WeaponType) reader.ReadInt32(),
                            // 14.25.2 Enchantment Type (Int32)
                            enchantment = (EnchantmentType) reader.ReadInt32()
                        };

                        // 14.25.3 Number Statues (Int32)
                        var numberStatuses = reader.ReadInt32();
                        for (var indexStatuses = 0; indexStatuses < numberStatuses; indexStatuses += 1)
                        {
                            // 14.25.3.1 Equipment Status (Int32)
                            var equipmentStatus = (EquipmentStatus) reader.ReadInt32();
                            // 14.25.3.2 Has ST (Boolean)
                            weapon.status[equipmentStatus] = reader.ReadBoolean();
                        }

                        actor.weapons.AddLast(weapon);
                    }

                    // 14.26 Number Armors (Int32)
                    var numberArmors = reader.ReadInt32();
                    for (var indexArmor = 0; indexArmor < numberArmors; indexArmor += 1)
                    {
                        var armor = new Armor(ArmorType.NO_ARMOR)
                        {
                            // 14.26.1 Armor Type (Int32)
                            type = (ArmorType) reader.ReadInt32(),
                            // 14.26.2 Enchantment Type (Int32)
                            enchantment = (EnchantmentType) reader.ReadInt32()
                        };

                        // 14.26.3 Number Statues (Int32)
                        var numberStatuses = reader.ReadInt32();
                        for (var indexStatuses = 0; indexStatuses < numberStatuses; indexStatuses += 1)
                        {
                            // 14.26.3.1 Equipment Status (Int32)
                            var equipmentStatus = (EquipmentStatus) reader.ReadInt32();
                            // 14.26.3.2 Has ST (Boolean)
                            armor.status[equipmentStatus] = reader.ReadBoolean();
                        }

                        actor.armors.AddLast(armor);
                    }

                    Tiebreakers.Add(actor);
                    identificator.Add(identification, actor);
                }
            }
        }
    }
}