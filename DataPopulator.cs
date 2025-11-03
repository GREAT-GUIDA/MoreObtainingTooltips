using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MoreObtainingTooltips.ObtainingSystem;
using Terraria.ID;
using Terraria.Localization;
using Terraria;
using System.IO;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;

namespace MoreObtainingTooltips
{
    public enum FishingRarity {
        None, Junk, Quest, Plentiful, Common, Uncommon,
        Rare, VeryRare, ExtremelyRare
    }
    public enum FishingSuffix {
        None, Hardmode,
        PostWoS, PostSupreme,
        PostDesertScourge, PostLeviathan, PostProvidence
    }
    class DataPopulator
    {
        public static void InitializeFishingSources(Dictionary<int, List<SourceInfo>> FishingSources) {
            FishingSources.Clear();
            InitializeFishingSources_Vanilla(FishingSources);
            InitializeFishingSources_Mods(FishingSources);
            InitializeFishingSources_Detect(FishingSources);
        }
        public static void InitializeCustomizedSources(Dictionary<int, List<SourceInfo>> CustomizedSources) {
            CustomizedSources.Clear();

            InitializeCustomizedSources_Vanilla(CustomizedSources);
            InitializeCustomizedSources_Mods(CustomizedSources);
            InitializeCustomizedSources_Detect(FishingSources);
        }
        public static void InitializeCustomizedSources_Vanilla(Dictionary<int, List<SourceInfo>> CustomizedSources) {
            AddCustomizedSourceToItems("AnglerQuest",
                // Guaranteed Rewards
                ItemID.FuzzyCarrot, ItemID.AnglerHat, ItemID.AnglerVest, ItemID.AnglerPants,
                ItemID.BottomlessBucket, ItemID.GoldenFishingRod,
                // Random Rewards
                ItemID.HoneyAbsorbantSponge, ItemID.BottomlessHoneyBucket, ItemID.HotlineFishingHook,
                ItemID.FinWings, ItemID.SuperAbsorbantSponge, ItemID.GoldenBugNet, ItemID.FishHook,
                ItemID.FishMinecart, ItemID.SeashellHairpin, ItemID.MermaidTail, ItemID.MermaidAdornment,
                ItemID.FishCostumeMask, ItemID.FishCostumeShirt, ItemID.FishCostumeFinskirt,
                ItemID.HighTestFishingLine, ItemID.AnglerEarring, ItemID.TackleBox,
                ItemID.FishermansGuide, ItemID.WeatherRadio, ItemID.Sextant, ItemID.FishingBobber,
                ItemID.FishingPotion, ItemID.SonarPotion, ItemID.CratePotion,
                // Furniture Rewards
                ItemID.LifePreserver, ItemID.ShipsWheel, ItemID.CompassRose, ItemID.WallAnchor,
                ItemID.PillaginMePixels, ItemID.TreasureMap, ItemID.GoldfishTrophy, ItemID.BunnyfishTrophy,
                ItemID.SwordfishTrophy, ItemID.SharkteethTrophy, ItemID.ShipInABottle,
                ItemID.SeaweedPlanter, ItemID.NotSoLostInParadise, ItemID.Crustography,
                ItemID.WhatLurksBelow, ItemID.Fangs, ItemID.CouchGag, ItemID.SilentFish, ItemID.TheDuke,
                // Bait Rewards
                ItemID.MasterBait, ItemID.JourneymanBait, ItemID.ApprenticeBait
            );

            AddCustomizedSourceToItems("PartyGirlGiven",
                ItemID.SliceOfCake
            );

            AddCustomizedSourceToItems("DyeTraderQuest",
                // Hardmode
                ItemID.AcidDye, ItemID.RedAcidDye, ItemID.BlueAcidDye, ItemID.MushroomDye,
                ItemID.PurpleOozeDye, ItemID.ReflectiveDye, ItemID.ReflectiveGoldDye,
                ItemID.ReflectiveSilverDye, ItemID.ReflectiveObsidianDye, ItemID.ReflectiveCopperDye,
                ItemID.ReflectiveMetalDye, ItemID.NegativeDye, ItemID.ShadowDye, ItemID.MirageDye,
                ItemID.TwilightDye, ItemID.HadesDye, ItemID.BurningHadesDye, ItemID.ShadowflameHadesDye,
                ItemID.GrimDye, ItemID.PhaseDye, ItemID.ShiftingSandsDye, ItemID.GelDye,
                ItemID.ChlorophyteDye, ItemID.LivingFlameDye, ItemID.LivingRainbowDye, ItemID.LivingOceanDye,
                ItemID.WispDye, ItemID.PixieDye, ItemID.UnicornWispDye, ItemID.InfernalWispDye,
                // After defeating Martian Madness
                ItemID.MartianArmorDye, ItemID.MidnightRainbowDye,
                // After defeating Moon Lord
                ItemID.DevDye
            );

            // 任意树 (Any trees)
            AddCustomizedSourceToItems("ShakingAnyTree",
                ItemID.Acorn, ItemID.Wood, ItemID.Apple, ItemID.Apricot, ItemID.Grapefruit,
                ItemID.Lemon, ItemID.Peach, ItemID.LivingWoodWand,
                ItemID.LeafWand, ItemID.EucaluptusSap
            );

            // 森林树 (Forest trees)
            AddCustomizedSourceToItems("ShakingForestTree",
                ItemID.Acorn, ItemID.Wood, ItemID.Apple, ItemID.Apricot, ItemID.Grapefruit,
                ItemID.Lemon, ItemID.Peach, ItemID.LivingWoodWand,
                ItemID.LeafWand, ItemID.EucaluptusSap
            );

            // 红木树 (Mahogany trees)
            AddCustomizedSourceToItems("ShakingMahoganyTree",
                ItemID.RichMahogany, ItemID.Mango, ItemID.Pineapple,
                ItemID.LivingMahoganyWand, ItemID.LivingMahoganyLeafWand
            );

            // 乌木树 (Ebonwood trees)
            AddCustomizedSourceToItems("ShakingEbonwoodTree",
                ItemID.Ebonwood, ItemID.Elderberry, ItemID.BlackCurrant
            );

            // 暗影木树 (Shadewood trees)
            AddCustomizedSourceToItems("ShakingShadewoodTree",
                ItemID.Shadewood, ItemID.BloodOrange, ItemID.Rambutan
            );

            // 珍珠木树 (Pearlwood trees)
            AddCustomizedSourceToItems("ShakingPearlwoodTree",
                ItemID.Acorn, ItemID.Pearlwood, ItemID.Dragonfruit, ItemID.Starfruit
            );

            // 棕榈树 (Palm trees)
            AddCustomizedSourceToItems("ShakingPalmTree",
                ItemID.PalmWood, ItemID.Coconut, ItemID.Banana
            );

            // 针叶树 (Boreal trees)
            AddCustomizedSourceToItems("ShakingBorealTree",
                ItemID.BorealWood, ItemID.Cherry, ItemID.Plum
            );

            // 灰烬树 (Ash trees)
            AddCustomizedSourceToItems("ShakingAshTree",
                ItemID.Acorn, ItemID.AshWood, ItemID.SpicyPepper, ItemID.Pomegranate
            );

            // 巨型夜光蘑菇 (Giant Glowing Mushrooms)
            AddCustomizedSourceToItems("ShakingGlowingMushroom",
                ItemID.MushroomGrassSeeds, ItemID.GlowingMushroom
            );

            // --- Breaking Pots ---
            AddCustomizedSourceToItems("BreakingPots",
                // Special Drops
                ItemID.GoldenKey, ItemID.FallenStar, ItemID.Rope,

                // Potions (All Layers)
                ItemID.IronskinPotion, ItemID.ShinePotion, ItemID.NightOwlPotion, ItemID.SwiftnessPotion,
                ItemID.MiningPotion, ItemID.CalmingPotion, ItemID.BuilderPotion, ItemID.RecallPotion,
                ItemID.RegenerationPotion, ItemID.ArcheryPotion, ItemID.GillsPotion, ItemID.HunterPotion,
                ItemID.TrapsightPotion, ItemID.SpelunkerPotion, ItemID.FeatherfallPotion, ItemID.WaterWalkingPotion,
                ItemID.GravitationPotion, ItemID.InvisibilityPotion, ItemID.ThornsPotion, ItemID.HeartreachPotion,
                ItemID.FlipperPotion, ItemID.ManaRegenerationPotion, ItemID.ObsidianSkinPotion, ItemID.MagicPowerPotion,
                ItemID.BattlePotion, ItemID.TitanPotion, ItemID.PotionOfReturn, ItemID.WormholePotion,

                // Torches & Glowsticks
                ItemID.Torch, ItemID.Glowstick, ItemID.StickyGlowstick, ItemID.HallowedTorch,
                ItemID.CorruptTorch, ItemID.CrimsonTorch, ItemID.JungleTorch, ItemID.IceTorch,
                ItemID.DesertTorch,

                // Ammo
                ItemID.WoodenArrow, ItemID.Shuriken, ItemID.Grenade, ItemID.HellfireArrow,
                ItemID.UnholyArrow, ItemID.SilverBullet, ItemID.TungstenBullet,

                // Healing & Bombs
                ItemID.LesserHealingPotion, ItemID.HealingPotion, ItemID.ScarabBomb, ItemID.Bomb
            );

            // --- Breaking Shadow Orbs ---
            AddCustomizedSourceToItems("BreakingShadowOrb",
                ItemID.Musket, ItemID.MusketBall, ItemID.ShadowOrb, ItemID.Vilethorn,
                ItemID.BallOHurt, ItemID.BandofStarpower
            );

            // --- Breaking Crimson Hearts ---
            AddCustomizedSourceToItems("BreakingCrimsonHeart",
                ItemID.TheUndertaker, ItemID.MusketBall, ItemID.CrimsonHeart, ItemID.PanicNecklace,
                ItemID.CrimsonRod, ItemID.TheRottedFork
            );

            // --- Slime Drops ---
            AddCustomizedSourceToItems("SlimeDrops",
                // Potions
                ItemID.SwiftnessPotion, ItemID.IronskinPotion, ItemID.SpelunkerPotion,
                ItemID.MiningPotion, ItemID.RecallPotion, ItemID.WormholePotion,

                // Ores
                ItemID.CopperOre, ItemID.TinOre, ItemID.IronOre, ItemID.LeadOre,
                ItemID.SilverOre, ItemID.TungstenOre, ItemID.GoldOre, ItemID.PlatinumOre,

                // Miscellaneous
                ItemID.Torch, ItemID.Bomb, ItemID.Rope
            );

            // 阿比盖尔的花 (在墓碑附近生成)
            AddCustomizedSourceToItems("AbigailsFlower", ItemID.AbigailsFlower);

            // 机械矿车 (专家/大师模式下击败所有机械 Boss 后获得)
            AddCustomizedSourceToItems("MechCart", ItemID.MinecartMech);

            // 火把神的恩宠 (在“火把神”事件中存活下来获得)
            AddCustomizedSourceToItems("TorchGodsFavor", ItemID.TorchGodsFavor);

            // 花园侏儒 (暴露在阳光下的侏儒变成)
            AddCustomizedSourceToItems("GardenGnome", ItemID.GardenGnome);

            // 熟棉花糖 (在篝火上使用棉花糖)
            AddCustomizedSourceToItems("CookedMarshmallow", ItemID.CookedMarshmallow);

            // 魔法南瓜子 (树妖在血月期间出售)
            AddCustomizedSourceToItems("MagicalPumpkinSeed", ItemID.MagicalPumpkinSeed);

            AddCustomizedSourceToItems("PlayerDeath", 321, 1173, 1174, 1175, 1176, 1177, 3230, 3231, 3229, 3233, 3232);


            string templateBiomeHardmode = Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.DroppedInBiomeHardmode");
            string templateBiome = Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.DroppedInBiome");

            // 使用 DroppedInBiomeHardmode 模板
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.UndergroundHallow")), ItemID.SoulofLight);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.UndergroundCorruption") + "/" + Language.GetTextValue("Bestiary_Biomes.UndergroundCrimson")), ItemID.SoulofNight);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.Ocean")), ItemID.PirateMap);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.TheUnderworld")), ItemID.LivingFireBlock, ItemID.HelFire);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.TheSnow")), ItemID.Amarok);

            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.Jungle")), ItemID.JungleKey);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.TheCorruption")), ItemID.CorruptionKey);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.Crimson")), ItemID.CrimsonKey);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.TheHallow")), ItemID.HallowedKey);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.Snow")), ItemID.FrozenKey);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.Desert")), ItemID.DungeonDesertKey);

            AddCustomizedSourceToItemsString(string.Format(templateBiome, Language.GetTextValue("Bestiary_Events.Halloween")), ItemID.Present);
            AddCustomizedSourceToItemsString(string.Format(templateBiome, Language.GetTextValue("Bestiary_Events.Christmas")), ItemID.GoodieBag, ItemID.BloodyMachete, ItemID.BladedGlove);

            // --- Pre-Hardmode Underworld Yoyo Drop (Post-Skeletron) ---
            AddCustomizedSourceToItems("CascadeDrop", ItemID.Cascade);

            // --- Hardmode Jungle Yoyo Drop (Post-Mech Boss) ---
            AddCustomizedSourceToItems("YeletsDrop", ItemID.Yelets);

            // --- Hardmode Dungeon Yoyo Drop (Post-Plantera) ---
            AddCustomizedSourceToItems("KrakenDrop", ItemID.Kraken);
        }
        public static void InitializeCustomizedSources_Mods(Dictionary<int, List<SourceInfo>> CustomizedSources) {
            const string calamity = "CalamityMod";

            AddCustomizedSourceToModItems("AnglerQuest", calamity, "GrandMarquisBait");

            var exhumeRelationships = new Dictionary<string, string>(){
                { "TheCommunity", "ShatteredCommunity" },
                { "EntropysVigil", "CindersOfLament" },
                { "StaffoftheMechworm", "Metastasis" },
                { "GhastlyVisage", "GruesomeEminence" },
                { "BurningSea", "Rancor" }
            };

            foreach (var pair in exhumeRelationships) {
                string baseItemName = pair.Key;
                string upgradedItemName = pair.Value;
                if (Utils.TryGetModItemId(calamity, baseItemName, out int baseItemId)) {
                    string sourceText = string.Format(Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.ExhumedFrom"), Lang.GetItemNameValue(baseItemId));
                    AddCustomizedSourceToModItemsString(sourceText, calamity, upgradedItemName);
                }
            }
        }
        public static void InitializeCustomizedSources_Detect(Dictionary<int, List<SourceInfo>> CustomizedSources) {
            var musicIdToPath = MusicLoader.musicByPath.ToDictionary(pair => pair.Value, pair => pair.Key);

            foreach (var pair in MusicLoader.musicToItem) {
                int musicId = pair.Key;
                int musicBoxItemId = pair.Value;

                if (musicIdToPath.TryGetValue(musicId, out string musicPath)) {
                    string format = Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.MusicBoxFormat");
                    string tooltip = string.Format(format, Path.GetFileName(musicPath));
                    AddCustomizedSourceToItemsString(tooltip, musicBoxItemId);
                }
            }
            for (int type = 1; type < ItemLoader.ItemCount; type++) {
                Item item = ContentSamples.ItemsByType[type];
                if (item.createTile == 139 && item.placeStyle > 0) {
                    MusicIdToMusicName.TryGetValue(item.placeStyle, out string musicName);
                    string format = Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.MusicBoxFormat");
                    string tooltip = string.Format(format, musicName);
                    AddCustomizedSourceToItemsString(tooltip, type);
                }
            }
            var tempPlayer = new Player();

            var vanillaJourneyItems = new List<Item> {new Item(ItemID.IronShortsword), new Item(ItemID.IronPickaxe), new Item(ItemID.IronAxe), new Item(ItemID.IronHammer), new Item(ItemID.Torch),
                new Item(ItemID.BabyBirdStaff), new Item(ItemID.Rope), new Item(ItemID.MagicMirror), new Item(ItemID.GrapplingHook), new Item(ItemID.CreativeWings), new Item(ItemID.WolfMountItem)};

            var vanillaItems = new List<Item> { new Item(ItemID.CopperShortsword), new Item(ItemID.CopperPickaxe), new Item(ItemID.CopperAxe) };

            var normalPlayer = new Player();
            List<Item> normalStartingItems = PlayerLoader.GetStartingItems(normalPlayer, vanillaItems);
            var normalStartingItemIds = normalStartingItems.Select(item => item.type).ToHashSet();

            var journeyPlayer = new Player { difficulty = 3 };
            List<Item> journeyStartingItems = PlayerLoader.GetStartingItems(journeyPlayer, vanillaJourneyItems);
            var journeyStartingItemIds = journeyStartingItems.Select(item => item.type).Distinct().ToList();

            int[] journeyExclusiveItemIds = journeyStartingItemIds.Where(id => !normalStartingItemIds.Contains(id)).ToArray();

            string normalSourceText = Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.StartingItem");
            AddCustomizedSourceToItemsString(normalSourceText, normalStartingItemIds.ToArray());

            string journeySourceText = Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.JourneyStartingItem");
            AddCustomizedSourceToItemsString(journeySourceText, journeyExclusiveItemIds);

            foreach (var registration in RegisteredCustomSources) {
                string fullKey = registration.Key;
                List<int> itemIDs = registration.Value;

                foreach (SourceInfo source in itemIDs) {
                    var itemID = source.id;
                    if (!CustomizedSources.TryGetValue(itemID, out List<SourceInfo> sources)) {
                        sources = new List<SourceInfo>();
                        CustomizedSources[itemID] = sources;
                    }

                    if (!sources.Contains(fullKey)) {
                        sources.Add(fullKey);
                    }
                }
            }
        }
        public static void InitializeFishingSources_Vanilla(Dictionary<int, List<SourceInfo>> FishingSources) {
            AddFishingSource(ItemID.ArmoredCavefish, FishingRarity.Uncommon, FishingSuffix.None, "Underground", "Cavern", "TheUnderworld");
            AddFishingSource(ItemID.AtlanticCod, FishingRarity.Common, FishingSuffix.None, "Snow");
            AddFishingSource(ItemID.Bass, FishingRarity.Plentiful, FishingSuffix.None, "Any");
            AddFishingSource(ItemID.BlueJellyfish, FishingRarity.Rare, FishingSuffix.None, "Underground", "Cavern", "TheUnderworld");
            AddFishingSource(ItemID.ChaosFish, FishingRarity.VeryRare, FishingSuffix.None, "Underground", "Hallow");
            AddFishingSource(ItemID.CrimsonTigerfish, FishingRarity.Common, FishingSuffix.None, "Crimson");
            AddFishingSource(ItemID.Damselfish, FishingRarity.Uncommon, FishingSuffix.None, "Sky");
            AddFishingSource(ItemID.DoubleCod, FishingRarity.Uncommon, FishingSuffix.None, "Surface", "Jungle");
            AddFishingSource(ItemID.Ebonkoi, FishingRarity.Uncommon, FishingSuffix.None, "TheCorruption");
            AddFishingSource(ItemID.FlarefinKoi, FishingRarity.VeryRare, FishingSuffix.None, "Lava");
            AddFishingSource(ItemID.Flounder, FishingRarity.Plentiful, FishingSuffix.None, "Oasis");
            AddFishingSource(ItemID.FrostMinnow, FishingRarity.Uncommon, FishingSuffix.None, "Snow");
            AddFishingSource(ItemID.GoldenCarp, FishingRarity.ExtremelyRare, FishingSuffix.None, "Underground");
            AddFishingSource(ItemID.GreenJellyfish, FishingRarity.Rare, FishingSuffix.Hardmode, "Underground");
            AddFishingSource(ItemID.Hemopiranha, FishingRarity.Uncommon, FishingSuffix.None, "Crimson");
            AddFishingSource(ItemID.Honeyfin, FishingRarity.Uncommon, FishingSuffix.None, "Honey");
            AddFishingSource(ItemID.NeonTetra, FishingRarity.Common, FishingSuffix.None, "Jungle");
            AddFishingSource(ItemID.Obsidifish, FishingRarity.Rare, FishingSuffix.None, "Lava");
            AddFishingSource(ItemID.PinkJellyfish, FishingRarity.Rare, FishingSuffix.None, "Ocean");
            AddFishingSource(ItemID.PrincessFish, FishingRarity.Uncommon, FishingSuffix.None, "Hallow");
            AddFishingSource(ItemID.Prismite, FishingRarity.Rare, FishingSuffix.None, "Hallow");
            AddFishingSource(ItemID.RedSnapper, FishingRarity.Common, FishingSuffix.None, "Ocean");
            AddFishingSource(ItemID.RockLobster, FishingRarity.Plentiful, FishingSuffix.None, "Oasis");
            AddFishingSource(ItemID.Salmon, FishingRarity.Common, FishingSuffix.None, "Forest", "LargeLakes");
            AddFishingSource(ItemID.Shrimp, FishingRarity.Uncommon, FishingSuffix.None, "Ocean");
            AddFishingSource(ItemID.SpecularFish, FishingRarity.Common, FishingSuffix.None, "Underground", "Cavern");
            AddFishingSource(ItemID.Stinkfish, FishingRarity.Rare, FishingSuffix.None, "Underground");
            AddFishingSource(ItemID.Trout, FishingRarity.Plentiful, FishingSuffix.None, "Forest", "Snow");
            AddFishingSource(ItemID.Tuna, FishingRarity.Common, FishingSuffix.None, "Ocean");
            AddFishingSource(ItemID.VariegatedLardfish, FishingRarity.Uncommon, FishingSuffix.None, "Underground", "Jungle");

            // == Usable Items (功能性物品) ==
            AddFishingSource(ItemID.FrogLeg, FishingRarity.ExtremelyRare, FishingSuffix.None, "Any");
            AddFishingSource(ItemID.BalloonPufferfish, FishingRarity.ExtremelyRare, FishingSuffix.None, "Any");
            AddFishingSource(ItemID.BombFish, FishingRarity.Uncommon, FishingSuffix.None, "Any");
            AddFishingSource(ItemID.PurpleClubberfish, FishingRarity.Rare, FishingSuffix.None, "TheCorruption");
            AddFishingSource(ItemID.ReaverShark, FishingRarity.VeryRare, FishingSuffix.None, "Ocean");
            AddFishingSource(ItemID.Rockfish, FishingRarity.VeryRare, FishingSuffix.None, "Underground");
            AddFishingSource(ItemID.SawtoothShark, FishingRarity.VeryRare, FishingSuffix.None, "Ocean");
            AddFishingSource(ItemID.FrostDaggerfish, FishingRarity.Uncommon, FishingSuffix.None, "Snow");
            AddFishingSource(ItemID.Swordfish, FishingRarity.Rare, FishingSuffix.None, "Ocean");
            AddFishingSource(ItemID.ZephyrFish, FishingRarity.ExtremelyRare, FishingSuffix.None, "Any");
            AddFishingSource(ItemID.Toxikarp, FishingRarity.ExtremelyRare, FishingSuffix.Hardmode, "TheCorruption");
            AddFishingSource(ItemID.Bladetongue, FishingRarity.ExtremelyRare, FishingSuffix.Hardmode, "Crimson");
            AddFishingSource(ItemID.CrystalSerpent, FishingRarity.ExtremelyRare, FishingSuffix.Hardmode, "Hallow");
            AddFishingSource(ItemID.ScalyTruffle, FishingRarity.ExtremelyRare, FishingSuffix.Hardmode, "CorruptIce", "CrimsonIce", "HallowIce");
            AddFishingSource(ItemID.ObsidianSwordfish, FishingRarity.ExtremelyRare, FishingSuffix.Hardmode, "Lava");
            AddFishingSource(ItemID.AlchemyTable, FishingRarity.VeryRare, FishingSuffix.None, "TheDungeon");
            AddFishingSource(ItemID.Oyster, FishingRarity.Uncommon, FishingSuffix.None, "Oasis");
            AddFishingSource(ItemID.CombatBook, FishingRarity.ExtremelyRare, FishingSuffix.None, "BloodMoon");
            AddFishingSource(ItemID.BottomlessLavaBucket, FishingRarity.ExtremelyRare, FishingSuffix.None, "Lava");
            AddFishingSource(ItemID.LavaAbsorbantSponge, FishingRarity.ExtremelyRare, FishingSuffix.None, "Lava");
            AddFishingSource(ItemID.DemonConch, FishingRarity.ExtremelyRare, FishingSuffix.None, "Lava");
            AddFishingSource(ItemID.DreadoftheRedSea, FishingRarity.ExtremelyRare, FishingSuffix.None, "BloodMoon");
            AddFishingSource(ItemID.LadyOfTheLake, FishingRarity.ExtremelyRare, FishingSuffix.Hardmode, "Hallow");

            // == Crates (板条箱) ==
            AddFishingSource(ItemID.WoodenCrate, FishingRarity.Plentiful, FishingSuffix.None, "Any");
            AddFishingSource(ItemID.WoodenCrateHard, FishingRarity.Plentiful, FishingSuffix.Hardmode, "Any");
            AddFishingSource(ItemID.IronCrate, FishingRarity.Uncommon, FishingSuffix.None, "Any");
            AddFishingSource(ItemID.IronCrateHard, FishingRarity.Uncommon, FishingSuffix.Hardmode, "Any");
            AddFishingSource(ItemID.GoldenCrate, FishingRarity.VeryRare, FishingSuffix.None, "Any");
            AddFishingSource(ItemID.GoldenCrateHard, FishingRarity.VeryRare, FishingSuffix.Hardmode, "Any");
            AddFishingSource(ItemID.JungleFishingCrate, FishingRarity.Rare, FishingSuffix.None, "Jungle");
            AddFishingSource(ItemID.JungleFishingCrateHard, FishingRarity.Rare, FishingSuffix.Hardmode, "Jungle");
            AddFishingSource(ItemID.FloatingIslandFishingCrate, FishingRarity.Rare, FishingSuffix.None, "Sky");
            AddFishingSource(ItemID.FloatingIslandFishingCrateHard, FishingRarity.Rare, FishingSuffix.Hardmode, "Sky");
            AddFishingSource(ItemID.CorruptFishingCrate, FishingRarity.Rare, FishingSuffix.None, "TheCorruption");
            AddFishingSource(ItemID.CorruptFishingCrateHard, FishingRarity.Rare, FishingSuffix.Hardmode, "TheCorruption");
            AddFishingSource(ItemID.CrimsonFishingCrate, FishingRarity.Rare, FishingSuffix.None, "Crimson");
            AddFishingSource(ItemID.CrimsonFishingCrateHard, FishingRarity.Rare, FishingSuffix.Hardmode, "Crimson");
            AddFishingSource(ItemID.HallowedFishingCrate, FishingRarity.Rare, FishingSuffix.None, "Hallow");
            AddFishingSource(ItemID.HallowedFishingCrateHard, FishingRarity.Rare, FishingSuffix.Hardmode, "Hallow");
            AddFishingSource(ItemID.DungeonFishingCrate, FishingRarity.Rare, FishingSuffix.None, "TheDungeon");
            AddFishingSource(ItemID.DungeonFishingCrateHard, FishingRarity.Rare, FishingSuffix.Hardmode, "TheDungeon");
            AddFishingSource(ItemID.FrozenCrate, FishingRarity.Rare, FishingSuffix.None, "Snow");
            AddFishingSource(ItemID.FrozenCrateHard, FishingRarity.Rare, FishingSuffix.Hardmode, "Snow");
            AddFishingSource(ItemID.OasisCrate, FishingRarity.Rare, FishingSuffix.None, "Desert");
            AddFishingSource(ItemID.OasisCrateHard, FishingRarity.Rare, FishingSuffix.Hardmode, "Desert");
            AddFishingSource(ItemID.LavaCrate, FishingRarity.Plentiful, FishingSuffix.None, "Lava");
            AddFishingSource(ItemID.LavaCrateHard, FishingRarity.Plentiful, FishingSuffix.Hardmode, "Lava");
            AddFishingSource(ItemID.OceanCrate, FishingRarity.Rare, FishingSuffix.None, "Ocean");
            AddFishingSource(ItemID.OceanCrateHard, FishingRarity.Rare, FishingSuffix.Hardmode, "Ocean");

            // == Junk (垃圾) ==
            AddFishingSource(ItemID.OldShoe, FishingRarity.Junk, FishingSuffix.None, "AnyLowPower");
            AddFishingSource(ItemID.Seaweed, FishingRarity.Junk, FishingSuffix.None, "AnyLowPower");
            AddFishingSource(ItemID.TinCan, FishingRarity.Junk, FishingSuffix.None, "AnyLowPower");
            AddFishingSource(ItemID.JojaCola, FishingRarity.Junk, FishingSuffix.None, "AnyLowPower");

            // == Quest Fish (任务鱼) ==
            AddFishingSource(ItemID.AmanitaFungifin, FishingRarity.Quest, FishingSuffix.None, "GlowingMushroom");
            AddFishingSource(ItemID.Angelfish, FishingRarity.Quest, FishingSuffix.None, "Sky");
            AddFishingSource(ItemID.Batfish, FishingRarity.Quest, FishingSuffix.None, "Underground", "Cavern");
            AddFishingSource(ItemID.BloodyManowar, FishingRarity.Quest, FishingSuffix.None, "Crimson");
            AddFishingSource(ItemID.Bonefish, FishingRarity.Quest, FishingSuffix.None, "Underground", "Cavern");
            AddFishingSource(ItemID.BumblebeeTuna, FishingRarity.Quest, FishingSuffix.None, "Honey");
            AddFishingSource(ItemID.Bunnyfish, FishingRarity.Quest, FishingSuffix.None, "Surface", "Forest");
            AddFishingSource(ItemID.CapnTunabeard, FishingRarity.Quest, FishingSuffix.Hardmode, "Ocean");
            AddFishingSource(ItemID.Catfish, FishingRarity.Quest, FishingSuffix.None, "Surface", "Jungle");
            AddFishingSource(ItemID.Cloudfish, FishingRarity.Quest, FishingSuffix.None, "Sky");
            AddFishingSource(ItemID.Clownfish, FishingRarity.Quest, FishingSuffix.None, "Ocean");
            AddFishingSource(ItemID.Cursedfish, FishingRarity.Quest, FishingSuffix.Hardmode, "TheCorruption");
            AddFishingSource(ItemID.DemonicHellfish, FishingRarity.Quest, FishingSuffix.None, "Cavern", "TheUnderworld");
            AddFishingSource(ItemID.Derpfish, FishingRarity.Quest, FishingSuffix.Hardmode, "Surface", "Jungle");
            AddFishingSource(ItemID.Dirtfish, FishingRarity.Quest, FishingSuffix.None, "Surface", "Underground");
            AddFishingSource(ItemID.DynamiteFish, FishingRarity.Quest, FishingSuffix.None, "Surface");
            AddFishingSource(ItemID.EaterofPlankton, FishingRarity.Quest, FishingSuffix.None, "TheCorruption");
            AddFishingSource(ItemID.FallenStarfish, FishingRarity.Quest, FishingSuffix.None, "Sky", "Surface");
            AddFishingSource(ItemID.Fishotron, FishingRarity.Quest, FishingSuffix.None, "Cavern");
            AddFishingSource(ItemID.Fishron, FishingRarity.Quest, FishingSuffix.Hardmode, "Underground", "Snow");
            AddFishingSource(ItemID.GuideVoodooFish, FishingRarity.Quest, FishingSuffix.None, "Cavern", "TheUnderworld");
            AddFishingSource(ItemID.Harpyfish, FishingRarity.Quest, FishingSuffix.None, "Sky", "Surface");
            AddFishingSource(ItemID.Hungerfish, FishingRarity.Quest, FishingSuffix.Hardmode, "Cavern", "TheUnderworld");
            AddFishingSource(ItemID.Ichorfish, FishingRarity.Quest, FishingSuffix.Hardmode, "Crimson");
            AddFishingSource(ItemID.InfectedScabbardfish, FishingRarity.Quest, FishingSuffix.None, "TheCorruption");
            AddFishingSource(ItemID.Jewelfish, FishingRarity.Quest, FishingSuffix.None, "Underground", "Cavern");
            AddFishingSource(ItemID.MirageFish, FishingRarity.Quest, FishingSuffix.Hardmode, "Underground", "Hallow");
            AddFishingSource(ItemID.Mudfish, FishingRarity.Quest, FishingSuffix.None, "Jungle");
            AddFishingSource(ItemID.MutantFlinxfin, FishingRarity.Quest, FishingSuffix.None, "Underground", "Snow");
            AddFishingSource(ItemID.Pengfish, FishingRarity.Quest, FishingSuffix.None, "Surface", "Snow");
            AddFishingSource(ItemID.Pixiefish, FishingRarity.Quest, FishingSuffix.Hardmode, "Surface", "Hallow");
            AddFishingSource(ItemID.ScarabFish, FishingRarity.Quest, FishingSuffix.None, "Desert");
            AddFishingSource(ItemID.ScorpioFish, FishingRarity.Quest, FishingSuffix.None, "Desert");
            AddFishingSource(ItemID.Slimefish, FishingRarity.Quest, FishingSuffix.None, "Surface", "Forest");
            AddFishingSource(ItemID.Spiderfish, FishingRarity.Quest, FishingSuffix.None, "Underground", "Cavern");
            AddFishingSource(ItemID.TheFishofCthulu, FishingRarity.Quest, FishingSuffix.None, "Surface");
            AddFishingSource(ItemID.TropicalBarracuda, FishingRarity.Quest, FishingSuffix.None, "Surface", "Jungle");
            AddFishingSource(ItemID.TundraTrout, FishingRarity.Quest, FishingSuffix.None, "Surface", "Snow");
            AddFishingSource(ItemID.UnicornFish, FishingRarity.Quest, FishingSuffix.Hardmode, "Hallow");
            AddFishingSource(ItemID.Wyverntail, FishingRarity.Quest, FishingSuffix.Hardmode, "Sky");
            AddFishingSource(ItemID.ZombieFish, FishingRarity.Quest, FishingSuffix.None, "Surface", "Forest");
        }
        public static void InitializeFishingSources_Mods(Dictionary<int, List<SourceInfo>> FishingSources) {
            // --- Continent of Journey Mod Integration---
            string hj = "ContinentOfJourney";
            AddModdedFishingSource(hj, "AnglerCoin", FishingRarity.Rare, FishingSuffix.None, "Any");
            AddModdedFishingSource(hj, "AnglerGoldCoin", FishingRarity.Rare, FishingSuffix.Hardmode, "Any");
            AddModdedFishingSource(hj, "ForeverCrate", FishingRarity.Rare, FishingSuffix.PostWoS, "Oasis");
            AddModdedFishingSource(hj, "CountdownCrate", FishingRarity.Rare, FishingSuffix.PostSupreme, "Oasis");
            AddModdedFishingSource(hj, "CubistCrate", FishingRarity.Rare, FishingSuffix.PostWoS, "Snow");
            AddModdedFishingSource(hj, "CubeCrate", FishingRarity.Rare, FishingSuffix.PostSupreme, "Snow");
            AddModdedFishingSource(hj, "LivingCrate", FishingRarity.Rare, FishingSuffix.PostWoS, "Jungle");
            AddModdedFishingSource(hj, "MembraneCrate", FishingRarity.Rare, FishingSuffix.PostSupreme, "Jungle");


            // --- Calamity Mod Integration ---
            string calamity = "CalamityMod";
            // Quest Fish (任务鱼)
            AddModdedFishingSource(calamity, "EutrophicSandfish", FishingRarity.Quest, FishingSuffix.None, "SunkenSea");
            AddModdedFishingSource(calamity, "SurfClam", FishingRarity.Quest, FishingSuffix.None, "SunkenSea");
            AddModdedFishingSource(calamity, "Serpentuna", FishingRarity.Quest, FishingSuffix.None, "SunkenSea");
            AddModdedFishingSource(calamity, "Brimlish", FishingRarity.Quest, FishingSuffix.None, "BrimstoneCrags");
            AddModdedFishingSource(calamity, "Slurpfish", FishingRarity.Quest, FishingSuffix.None, "BrimstoneCrags");
            // Crates (板条箱)
            AddModdedFishingSource(calamity, "MonolithCrate", FishingRarity.Rare, FishingSuffix.None, "AstralInfection");
            AddModdedFishingSource(calamity, "AstralCrate", FishingRarity.Rare, FishingSuffix.Hardmode, "AstralInfection");
            AddModdedFishingSource(calamity, "EutrophicCrate", FishingRarity.Rare, FishingSuffix.None, "SunkenSea");
            AddModdedFishingSource(calamity, "PrismCrate", FishingRarity.Rare, FishingSuffix.Hardmode, "SunkenSea");
            AddModdedFishingSource(calamity, "SulphurousCrate", FishingRarity.Rare, FishingSuffix.None, "SulphurousSea");
            AddModdedFishingSource(calamity, "HydrothermalCrate", FishingRarity.Rare, FishingSuffix.Hardmode, "SulphurousSea");
            AddModdedFishingSource(calamity, "SlagCrate", FishingRarity.Rare, FishingSuffix.None, "BrimstoneCrags", "Lava");
            AddModdedFishingSource(calamity, "BrimstoneCrate", FishingRarity.Rare, FishingSuffix.Hardmode, "BrimstoneCrags", "Lava");

            // Biome-Specific Fish (特定环境渔获)
            // Sunken Sea (深渊)
            AddModdedFishingSource(calamity, "PrismaticGuppy", FishingRarity.Plentiful, FishingSuffix.None, "SunkenSea");
            AddModdedFishingSource(calamity, "SunkenSailfish", FishingRarity.Uncommon, FishingSuffix.None, "SunkenSea");
            AddModdedFishingSource(calamity, "GreenwaveLoach", FishingRarity.VeryRare, FishingSuffix.None, "SunkenSea");
            AddModdedFishingSource(calamity, "RustedJingleBell", FishingRarity.ExtremelyRare, FishingSuffix.None, "SunkenSea");
            AddModdedFishingSource(calamity, "SparklingEmpress", FishingRarity.ExtremelyRare, FishingSuffix.None, "SunkenSea");
            AddModdedFishingSource(calamity, "SerpentsBite", FishingRarity.ExtremelyRare, FishingSuffix.Hardmode, "SunkenSea");
            // Astral Infection (星辉瘟疫)
            AddModdedFishingSource(calamity, "TwinklingPollox", FishingRarity.Plentiful, FishingSuffix.None, "AstralInfection");
            AddModdedFishingSource(calamity, "ProcyonidPrawn", FishingRarity.Uncommon, FishingSuffix.None, "AstralInfection");
            AddModdedFishingSource(calamity, "ArcturusAstroidean", FishingRarity.Uncommon, FishingSuffix.None, "AstralInfection");
            AddModdedFishingSource(calamity, "AldebaranAlewife", FishingRarity.Uncommon, FishingSuffix.None, "AstralInfection");
            AddModdedFishingSource(calamity, "PolarisParrotfish", FishingRarity.ExtremelyRare, FishingSuffix.None, "AstralInfection");
            AddModdedFishingSource(calamity, "GacruxianMollusk", FishingRarity.ExtremelyRare, FishingSuffix.None, "AstralInfection");
            AddModdedFishingSource(calamity, "UrsaSergeant", FishingRarity.ExtremelyRare, FishingSuffix.None, "AstralInfection");
            // Sulphurous Sea (硫磺海)
            AddModdedFishingSource(calamity, "PlantyMush", FishingRarity.Common, FishingSuffix.None, "SulphurousSea");
            AddModdedFishingSource(calamity, "AlluringBait", FishingRarity.ExtremelyRare, FishingSuffix.None, "SulphurousSea");
            AddModdedFishingSource(calamity, "AbyssalAmulet", FishingRarity.ExtremelyRare, FishingSuffix.None, "SulphurousSea");
            // Brimstone Crags (硫火之崖 - 岩浆)
            AddModdedFishingSource(calamity, "CragBullhead", FishingRarity.Plentiful, FishingSuffix.None, "BrimstoneCrags", "Lava");
            AddModdedFishingSource(calamity, "CoastalDemonfish", FishingRarity.Uncommon, FishingSuffix.None, "BrimstoneCrags", "Lava");
            AddModdedFishingSource(calamity, "Havocfish", FishingRarity.Uncommon, FishingSuffix.Hardmode, "BrimstoneCrags", "Lava");
            AddModdedFishingSource(calamity, "Bloodfin", FishingRarity.Rare, FishingSuffix.Hardmode, "BrimstoneCrags", "Lava");
            AddModdedFishingSource(calamity, "CharredLasher", FishingRarity.VeryRare, FishingSuffix.Hardmode, "BrimstoneCrags", "Lava");
            AddModdedFishingSource(calamity, "DragoonDrizzlefish", FishingRarity.ExtremelyRare, FishingSuffix.None, "BrimstoneCrags", "Lava");

            // General Fish (通用渔获)
            AddModdedFishingSource(calamity, "StuffedFish", FishingRarity.Uncommon, FishingSuffix.None, "Surface", "DayTime");
            AddModdedFishingSource(calamity, "EnchantedStarfish", FishingRarity.Uncommon, FishingSuffix.None, "Surface", "NightTime");
            AddModdedFishingSource(calamity, "GlimmeringGemfish", FishingRarity.Uncommon, FishingSuffix.None, "Caverns");
            AddModdedFishingSource(calamity, "Spadefish", FishingRarity.VeryRare, FishingSuffix.None, "Underground");
            AddModdedFishingSource(calamity, "Gorecodile", FishingRarity.Uncommon, FishingSuffix.None, "BloodMoon");
            AddModdedFishingSource(calamity, "Shadowfish", FishingRarity.Uncommon, FishingSuffix.None, "Any", "Night");
            AddModdedFishingSource(calamity, "FishofLight", FishingRarity.Uncommon, FishingSuffix.Hardmode, "Underground", "Hallow");
            AddModdedFishingSource(calamity, "FishofNight", FishingRarity.Uncommon, FishingSuffix.Hardmode, "Underground", "TheCorruption", "Crimson");
            AddModdedFishingSource(calamity, "SunbeamFish", FishingRarity.Uncommon, FishingSuffix.Hardmode, "Sky");
            AddModdedFishingSource(calamity, "FishofFlight", FishingRarity.Uncommon, FishingSuffix.Hardmode, "Sky");
            AddModdedFishingSource(calamity, "FishofEleum", FishingRarity.Uncommon, FishingSuffix.Hardmode, "Snow");
            AddModdedFishingSource(calamity, "Floodtide", FishingRarity.ExtremelyRare, FishingSuffix.None, "Any");


            // --- Secrets of the Shadows (SOTS) Mod Integration ---
            string sots = "SOTS";

            // Standard Catches
            AddModdedFishingSource(sots, "TinyPlanetFish", FishingRarity.Uncommon, FishingSuffix.None, "Sky");
            AddModdedFishingSource(sots, "PistolShrimp", FishingRarity.Rare, FishingSuffix.None, "Ocean");
            AddModdedFishingSource(sots, "CrabClaw", FishingRarity.VeryRare, FishingSuffix.None, "Ocean");
            AddModdedFishingSource(sots, "PinkJellyfishStaff", FishingRarity.VeryRare, FishingSuffix.None, "Caverns");

            // Pyramid Biome Catches
            AddModdedFishingSource(sots, "SeaSnake", FishingRarity.Common, FishingSuffix.None, "Pyramid");
            AddModdedFishingSource(sots, "PhantomFish", FishingRarity.Common, FishingSuffix.None, "Pyramid");
            AddModdedFishingSource(sots, "Curgeon", FishingRarity.Common, FishingSuffix.None, "Pyramid");
            AddModdedFishingSource(sots, "ZephyrousZeppelin", FishingRarity.Rare, FishingSuffix.None, "Pyramid");
            AddModdedFishingSource(sots, "PyramidCrate", FishingRarity.Rare, FishingSuffix.None, "Pyramid");

            // Special Condition Crates
            AddModdedFishingSource(sots, "PlanetariumCrate", FishingRarity.Rare, FishingSuffix.None, "PlanetariumFishing");
            AddModdedFishingSource(sots, "OtherworldCrate", FishingRarity.Rare, FishingSuffix.Hardmode, "PlanetariumFishing");
        }


        public struct SimScenario {
            public string BiomeKey;
            public Action<Player> SetBiomeState;

            public int HeightLevel;
            public Action<Player> SetHeightState;

            public bool InLava;
            public bool InHoney;

            public bool RollCommon;
            public bool RollUncommon;
            public bool RollRare;
            public bool RollVeryRare;
            public bool RollLegendary;
            public bool RollCrate;

            public bool IsHardmode;

            public int FishingPower;
        }
        public static List<SimScenario> GetScenarios() {
            var scenarios = new List<SimScenario>();

            var hardmodes = new[] { false, true };
            var fishingPowers = new[] { 50, 75 , 100, 125, 150, 175 };

            var biomeSetters = new Dictionary<string, Action<Player>> {
                { "Any", p => Main.bloodMoon = false },
                { "TheCorruption", p => {p.ZoneCorrupt = true; } },
                { "Crimson", p => {p.ZoneCrimson = true; } },
                { "Jungle", p => {p.ZoneJungle = true; } },
                { "Snow", p => {p.ZoneSnow = true; } },
                { "TheDungeon", p => {p.ZoneDungeon = true; } },
                { "TheHallow", p => {p.ZoneHallow = true; } },
                { "Ocean", p => {p.ZoneBeach = true; } },
                { "Desert", p => {p.ZoneDesert = true; } },
                { "GlowingMushroom", p => {p.ZoneGlowshroom = true; } },
                { "BloodMoon", p => Main.bloodMoon = true }
            };

            var heightSetters = new Dictionary<int, (string Key, Action<Player> Action)> {
                { 0, ("_Sky", p => p.ZoneSkyHeight = true) },
                { 1, ("", p => p.ZoneOverworldHeight = true) },
                { 2, ("_Underground", p => p.ZoneDirtLayerHeight = true) },
                { 3, ("_Cavern", p => p.ZoneRockLayerHeight = true) },
                { 4, ("_Underworld", p => p.ZoneUnderworldHeight = true) }
            };

            var liquidSetters = new[] {
                (Key: "", InLava: false, InHoney: false),
                (Key: "Lava", InLava: true, InHoney: false),
                (Key: "Honey", InLava: false, InHoney: true)
            };

            var raritySetters = new[] {
                (Name: "Common", C: true, UC: false, R: false, VR: false, L: false, Cr: false),
                (Name: "Uncommon", C: false, UC: true, R: false, VR: false, L: false, Cr: false),
                (Name: "Rare", C: false, UC: false, R: true, VR: false, L: false, Cr: false),
                (Name: "VeryRare", C: false, UC: false, R: false, VR: true, L: false, Cr: false),
                (Name: "Legendary", C: false, UC: false, R: false, VR: false, L: true, Cr: false),
                (Name: "Crate", C: false, UC: true, R: false, VR: false, L: false, Cr: true),
                (Name: "Crate2", C: false, UC: false, R: true, VR: false, L: false, Cr: true),
                (Name: "Crate3", C: false, UC: false, R: false, VR: true, L: false, Cr: true)
            };

            foreach (var isHardmode in hardmodes)
                foreach (var power in fishingPowers)
                    foreach (var biome in biomeSetters)
                        foreach (var height in heightSetters)
                            foreach (var liquid in liquidSetters)
                                foreach (var rarity in raritySetters) {
                                    if (liquid.Key == "Lava" && height.Key != 4) continue;
                                    if (liquid.Key == "Honey" && biome.Key != "Jungle") continue;
                                    if (biome.Key == "BloodMoon" && height.Key != 1) continue;
                                    if (height.Key == 4 && biome.Key != "Any") continue;
                                    if (height.Key == 0 && biome.Key != "Any") continue;
                                    string biomeKey = biome.Key;
                                    if (height.Key == 4) biomeKey = "TheUnderworld";
                                    if (height.Key == 0) biomeKey = "Sky";
                                    if (liquid.Key != "")biomeKey = liquid.Key;
                                    scenarios.Add(new SimScenario {
                                        BiomeKey = biomeKey,
                                        SetBiomeState = biome.Value,
                                        HeightLevel = height.Key,
                                        SetHeightState = height.Value.Action,
                                        InLava = liquid.InLava,
                                        InHoney = liquid.InHoney,
                                        RollCommon = rarity.C,
                                        RollUncommon = rarity.UC,
                                        RollRare = rarity.R,
                                        RollVeryRare = rarity.VR,
                                        RollLegendary = rarity.L,
                                        RollCrate = rarity.Cr,
                                        IsHardmode = isHardmode,
                                        FishingPower = power
                                    });
                                }
            return scenarios;
        }
        private struct SimFinding {
            public HashSet<string> Biomes;
            public bool FoundInPreHardmode;
            public bool FoundInHardmode;
            public HashSet<FishingRarity> Rarities;

            public SimFinding() {
                Biomes = new HashSet<string>();
                FoundInPreHardmode = false;
                FoundInHardmode = false;
                Rarities = new HashSet<FishingRarity>();
            }
        }
        public static void InitializeFishingSources_Detect(Dictionary<int, List<SourceInfo>> fishingSources) {
            Player simPlayer = new Player();
            simPlayer.whoAmI = 255;
            Main.player[255] = simPlayer;

            Projectile simProj = new Projectile();
            simProj.owner = 255;

            var tempFindings = new Dictionary<int, SimFinding>();
            bool origBloodMoon = Main.bloodMoon;
            bool origHardMode = Main.hardMode;
            bool origDownedBoss3 = NPC.downedBoss3;

            NPC.downedBoss3 = true;
            var scenarios = GetScenarios();
            var iterationsPerScenario = 20;

            var cnt = 0;

            
            try {
                var pole = new Item(ItemID.GoldenFishingRod);
                var bait = new Item(ItemID.MasterBait);
                foreach (var scenario in scenarios) {
                    Main.hardMode = scenario.IsHardmode;
                    
                    simPlayer.ZoneCorrupt = false;
                    simPlayer.ZoneCrimson = false; 
                    simPlayer.ZoneJungle = false;
                    simPlayer.ZoneSnow = false;
                    simPlayer.ZoneDungeon = false;
                    simPlayer.ZoneHallow = false;
                    simPlayer.ZoneBeach = false;
                    simPlayer.ZoneDesert = false;
                    simPlayer.ZoneGlowshroom = false;
                    Main.bloodMoon = false;
                    simPlayer.ZoneSkyHeight = false;
                    simPlayer.ZoneOverworldHeight = false;
                    simPlayer.ZoneDirtLayerHeight = false;
                    simPlayer.ZoneRockLayerHeight = false;
                    simPlayer.ZoneUnderworldHeight = false;

                    scenario.SetBiomeState(simPlayer);
                    scenario.SetHeightState(simPlayer);

                    simPlayer.fishingSkill = scenario.FishingPower;
                    simPlayer.active = true;
                    simPlayer.dead = false;
                    simPlayer.Center = new Vector2(Main.maxTilesX / 2 * 16, (int)Main.worldSurface / 2 * 16);

                    for (int i = 0; i < iterationsPerScenario; i++) {
                        cnt += 1;
                        FishingAttempt fisher = default;
                        fisher.waterTilesCount = 3000;
                        fisher.waterNeededToFish = 300;
                        fisher.waterQuality = 1f;
                        fisher.fishingLevel = scenario.FishingPower;
                        fisher.heightLevel = scenario.HeightLevel;

                        fisher.inLava = scenario.InLava;
                        fisher.inHoney = scenario.InHoney;

                        fisher.playerFishingConditions = new PlayerFishingConditions {
                            Pole = pole,
                            Bait = bait,
                            FinalFishingLevel = scenario.FishingPower,
                            LevelMultipliers = 1f,
                        };
                        fisher.CanFishInLava = true;

                        fisher.common = scenario.RollCommon;
                        fisher.uncommon = scenario.RollUncommon;
                        fisher.rare = scenario.RollRare;
                        fisher.veryrare = scenario.RollVeryRare;
                        fisher.legendary = scenario.RollLegendary;
                        fisher.crate = scenario.RollCrate;

                        simProj.FishingCheck_RollItemDrop(ref fisher);
                        AdvancedPopupRequest sonar = new();
                        Vector2 sonarPosition = new(-1145141f, -919810f);
                        PlayerLoader.CatchFish(simPlayer, fisher, ref fisher.rolledItemDrop, ref fisher.rolledEnemySpawn, ref sonar, ref sonarPosition);

                        if (fisher.rolledItemDrop > ItemID.None) {
                            int itemID = fisher.rolledItemDrop;

                            if (!tempFindings.TryGetValue(itemID, out var finding)) {
                                finding = new SimFinding();
                                tempFindings[itemID] = finding;
                            }

                            finding.Biomes.Add(scenario.BiomeKey);
                            if (scenario.RollCommon) finding.Rarities.Add(FishingRarity.Common);
                            else if (scenario.RollUncommon) finding.Rarities.Add(FishingRarity.Uncommon);
                            else if (scenario.RollRare) finding.Rarities.Add(FishingRarity.Rare);
                            else if (scenario.RollVeryRare) finding.Rarities.Add(FishingRarity.VeryRare);
                            else if (scenario.RollLegendary) finding.Rarities.Add(FishingRarity.ExtremelyRare);
                            if (scenario.IsHardmode) {
                                finding.FoundInHardmode = true;
                            } else {
                                finding.FoundInPreHardmode = true;
                            }
                        }
                    }
                }
            } catch (Exception) { /* 静默失败 */ }
            foreach (var pair in tempFindings) {
                int itemID = pair.Key;
                var finding = pair.Value;

                if (fishingSources.ContainsKey(itemID)) {
                    continue;
                }

                FishingSuffix suffix;
                if (finding.FoundInHardmode && !finding.FoundInPreHardmode) {
                    suffix = FishingSuffix.Hardmode;
                } else {
                    suffix = FishingSuffix.None;
                }

                string[] environmentKeys;
                if (finding.Biomes.Count > 5) {
                    environmentKeys = new string[] { "Any" };
                } else {
                    var biomeList = finding.Biomes.ToList();
                    if (biomeList.Contains("Any") && biomeList.Count > 1) {
                        biomeList.Remove("Any");
                    }
                    environmentKeys = biomeList.ToArray();
                }
                FishingRarity finalRarity;
                if (finding.Rarities.Count == 1) {
                    finalRarity = finding.Rarities.First(); // 仅在 1 个池中找到
                } else {
                    finalRarity = FishingRarity.None; // 在 0 个或 多个池中找到
                }
                AddFishingSource(itemID, finalRarity, suffix, environmentKeys);
            }
        

            Main.bloodMoon = origBloodMoon;
            Main.hardMode = origHardMode;
            NPC.downedBoss3 = origDownedBoss3;
            Main.player[255] = new Player();
        }


        public static void AddFishingSource(int itemId, FishingRarity rarity, FishingSuffix suffix, params string[] environmentKeys) {
            string localizedEnvironmentText = string.Join(", ", environmentKeys
                .Select(key => {
                    string localizationKey = $"Bestiary_Biomes.{key}";
                    string localizedText = Language.GetTextValue(localizationKey);
                    if (localizedText != localizationKey) {
                        return localizedText;
                    }
                    localizationKey = $"Bestiary_Events.{key}";
                    localizedText = Language.GetTextValue(localizationKey);
                    if (localizedText != localizationKey) {
                        return localizedText;
                    }
                    localizationKey = $"Bestiary_Times.{key}";
                    localizedText = Language.GetTextValue(localizationKey);
                    if (localizedText != localizationKey) {
                        return localizedText;
                    }
                    return Language.GetTextValue($"Mods.MoreObtainingTooltips.Tooltips.Fishing.Environments.{key}");
                }));

            var source = new SourceInfo(
                id: (int)rarity, 
                num: (int)suffix,
                str: localizedEnvironmentText 
            );
            FishingSources[itemId] = new List<SourceInfo> { source };
        }
        public static void AddModdedFishingSource(string modName, string itemName, FishingRarity rarity, FishingSuffix suffix, params string[] environmentKeys) {
            int itemId = Utils.GetModItemId(modName, itemName);
            if (itemId > 0) {
                AddFishingSource(itemId, rarity, suffix, environmentKeys);
            }
        }
        
        public static void AddCustomizedSourceToItems(string key, params int[] itemIDs) {
            string sourceText = Language.GetTextValue($"Mods.MoreObtainingTooltips.Tooltips.{key}");
            foreach (int itemID in itemIDs) {
                AddCustomizedSource(itemID, sourceText);
            }
        }
        public static void AddCustomizedSourceToItemsString(string source, params int[] itemIDs) {
            foreach (int itemID in itemIDs) {
                AddCustomizedSource(itemID, source);
            }
        }
        public static void AddCustomizedSourceToModItems(string key, string modName, params string[] itemNames) {
            string sourceText = Language.GetTextValue($"Mods.MoreObtainingTooltips.Tooltips.{key}");
            foreach (string itemName in itemNames) {
                if (Utils.TryGetModItemId(modName, itemName, out int itemID)) {
                    AddCustomizedSource(itemID, sourceText);
                }
            }
        }
        public static void AddCustomizedSourceToModItemsString(string source, string modName, params string[] itemNames) {
            foreach (string itemName in itemNames) {
                if (Utils.TryGetModItemId(modName, itemName, out int itemID)) {
                    AddCustomizedSource(itemID, source);
                }
            }
        }
        private static void AddCustomizedSource(int itemID, string sourceText) {
            if (itemID <= ItemID.None) return;
            if (!CustomizedSources.TryGetValue(itemID, out List<SourceInfo> sources)) {
                sources = new List<SourceInfo>();
                CustomizedSources[itemID] = sources;
            }
            if (!sources.Contains(sourceText)) {
                sources.Add(sourceText);
            }
        }
    }
}
