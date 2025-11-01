using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria.GameContent;
using Terraria.Utilities;
using Terraria.Enums;
using Terraria.GameContent.ItemDropRules;
using Terraria.ObjectData;
using Terraria.ModLoader.IO;
using tModPorter;
using Terraria.Localization;
using static Terraria.GameContent.Bestiary.IL_BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions;

namespace MoreObtainingTooltips {
    public class ObtainingSystem : ModSystem {
        public struct FishingInfo {
            public readonly string RarityKey;
            public readonly List<string> EnvironmentKeys;
            public readonly bool IsHardmode;

            public FishingInfo(string rarityKey, bool isHardmode, params string[] environmentKeys) {
                RarityKey = rarityKey;
                EnvironmentKeys = new List<string>(environmentKeys);
                IsHardmode = isHardmode;
            }
        }
        public struct ShopSourceInfo {
            public readonly int NpcId;
            public readonly List<string> Conditions;

            public ShopSourceInfo(int npcId, List<string> conditions) {
                NpcId = npcId;
                Conditions = conditions;
            }
        }
        public static Dictionary<int, List<int>> ShimmerSources { get; private set; }
        public static Dictionary<int, List<int>> DecraftSources { get; private set; }
        public static Dictionary<int, List<int>> DropSources { get; private set; }
        public static Dictionary<int, List<int>> GrabBagSources { get; private set; }
        public static Dictionary<int, List<int>> CraftingSources { get; private set; }
        public static Dictionary<int, List<ShopSourceInfo>> ShopSources { get; private set; }
        public static Dictionary<int, List<int>> ChestSources { get; private set; }
        public static Dictionary<int, List<int>> CatchNPCSources { get; private set; }
        public static Dictionary<int, FishingInfo> FishingSources { get; private set; }
        public static Dictionary<int, List<int>> ExtractinatorSources { get; private set; }
        public static Dictionary<int, List<int>> ChlorophyteExtractinatorSources { get; private set; }
        public static Dictionary<int, List<string>> CustomizedSources { get; private set; }

        public static Dictionary<string, List<int>> RegisteredCustomSources = new();
        
        private static bool _loadedChestSourcesFromTag;

        public override void OnModLoad() {
            ShimmerSources = new Dictionary<int, List<int>>();
            DecraftSources = new Dictionary<int, List<int>>();
            DropSources = new Dictionary<int, List<int>>();
            GrabBagSources = new Dictionary<int, List<int>>();
            CraftingSources = new Dictionary<int, List<int>>();
            ShopSources = new Dictionary<int, List<ShopSourceInfo>>();
            ChestSources = new Dictionary<int, List<int>>();
            CatchNPCSources = new Dictionary<int, List<int>>();
            FishingSources = new Dictionary<int, FishingInfo>();
            ExtractinatorSources = new Dictionary<int, List<int>>();
            ChlorophyteExtractinatorSources = new Dictionary<int, List<int>>();

            _loadedChestSourcesFromTag = false;
        }

        // 保存世界数据
        public override void SaveWorldData(TagCompound tag) {
            // 如果 ChestSources 为空或未初始化，则不保存
            if (ChestSources == null || ChestSources.Count == 0) return;

            var list = new List<TagCompound>();
            foreach (var pair in ChestSources) {
                list.Add(new TagCompound {
                    ["key"] = pair.Key,
                    ["sources"] = pair.Value
                });
            }
            tag["ChestSources"] = list;
        }

        // 加载世界数据
        public override void LoadWorldData(TagCompound tag) {
            // 在加载前总是清空
            ChestSources.Clear();
            _loadedChestSourcesFromTag = false;

            if (tag.TryGet("ChestSources", out List<TagCompound> list)) {
                foreach (var entryTag in list) {
                    if (entryTag.TryGet("key", out int key) && entryTag.TryGet("sources", out List<int> sources)) {
                        ChestSources[key] = sources;
                    }
                }
                _loadedChestSourcesFromTag = true;
            }
        }

        public override void OnWorldUnload() {
            ChestSources.Clear();
            _loadedChestSourcesFromTag = false;
        }

        public override void PostWorldLoad() {
            // --- Reset Dictionaries ---
            ShimmerSources.Clear();
            DecraftSources.Clear();
            DropSources.Clear();
            GrabBagSources.Clear();
            CraftingSources.Clear();
            ShopSources.Clear();
            CatchNPCSources.Clear();
            FishingSources.Clear();
            ExtractinatorSources.Clear();
            ChlorophyteExtractinatorSources.Clear();


            // --- Populate Shimmer and Decraft Sources ---
            for (int i = 1; i < ItemLoader.ItemCount; i++) {
                Item sourceItem = new Item();
                sourceItem.SetDefaults(i);
                ItemLoader.SetDefaults(sourceItem);

                if (sourceItem.type <= ItemID.None) continue;

                int shimmerEquivalentType = sourceItem.GetShimmerEquivalentType();
                int decraftingRecipeIndex = ShimmerTransforms.GetDecraftingRecipeIndex(shimmerEquivalentType);

                if(ItemID.Sets.CoinLuckValue[sourceItem.type] > 0) continue;

                if (ItemID.Sets.ShimmerTransformToItem[shimmerEquivalentType] > 0) {
                    int resultItemType = ItemID.Sets.ShimmerTransformToItem[shimmerEquivalentType];
                    AddSource(ShimmerSources, resultItemType, sourceItem.type);
                } else if (decraftingRecipeIndex >= 0) {
                    Recipe recipe = Main.recipe[decraftingRecipeIndex];
                    List<Item> resultItems = recipe.customShimmerResults ?? recipe.requiredItem;

                    foreach (var resultItem in resultItems) {
                        if (resultItem != null && !resultItem.IsAir) {
                            AddSource(DecraftSources, resultItem.type, sourceItem.type);
                        }
                    }
                } else if (sourceItem.createTile == TileID.MusicBoxes) {
                    AddSource(ShimmerSources, ItemID.MusicBox, sourceItem.type);
                }
            }
            for (int i = 5401; i < 5409; i++) {
                AddSource(ShimmerSources, i, 3461);
            }
            AddSource(ShimmerSources, 5335, 1326);
            AddSource(ShimmerSources, 5134, 779);
            AddSource(ShimmerSources, 5364, 3031);
            AddSource(ShimmerSources, 3031, 5364);


            // --- Populate NPC Drop Sources ---
            for (int i = NPCID.NegativeIDCount; i < NPCLoader.NPCCount; i++) {
                List<IItemDropRule> dropRules = Main.ItemDropsDB.GetRulesForNPCID(i, false);
                var list = new List<DropRateInfo>();
                var ratesInfo = new DropRateInfoChainFeed(1f);
                foreach (IItemDropRule item in dropRules) {
                    item.ReportDroprates(list, ratesInfo);
                }

                foreach (DropRateInfo dropRateInfo in list) {
                    AddSource(DropSources, dropRateInfo.itemId, i);
                }
            }

            // --- Populate Catch NPC Sources ---
            for (int i = 1; i < NPCLoader.NPCCount; i++) {
                NPC npc = new NPC();
                npc.SetDefaults(i);
                NPCLoader.SetDefaults(npc);
                if (npc.catchItem > ItemID.None) {
                    AddSource(CatchNPCSources, npc.catchItem, i);
                }
            }


            // --- Populate Shop Sources ---
            foreach (var shop in NPCShopDatabase.AllShops) {
                if (shop.NpcType == 0) continue;
                foreach (var entry in shop.ActiveEntries) {
                    if (!entry.Item.IsAir) {
                        var conditionTexts = entry.Conditions
                            .Select(condition => condition.Description.Value)
                            .Where(text => !string.IsNullOrEmpty(text))
                            .ToList();
                        
                        var sourceInfo = new ShopSourceInfo(shop.NpcType, conditionTexts);

                        if (!ShopSources.TryGetValue(entry.Item.type, out var sourceList)) {
                            sourceList = new List<ShopSourceInfo>();
                            ShopSources[entry.Item.type] = sourceList;
                        }

                        sourceList.Add(sourceInfo);
                        if(sourceList.Count >= 21 || sourceList[0].NpcId == -1) {
                            sourceList.Clear();
                            sourceList.Add(new ShopSourceInfo(-1, conditionTexts));
                        }
                    }
                }
            }

            // --- Populate Chest Sources ---
            if (!_loadedChestSourcesFromTag) {
                for (int i = 0; i < Main.maxChests; i++) {
                    Chest chest = Main.chest[i];
                    if (chest == null) continue;

                    Tile tile = Main.tile[chest.x, chest.y];
                    if (tile == null || !TileID.Sets.IsAContainer[tile.type]) continue;

                    int style = TileObjectData.GetTileStyle(tile);
                    int chestItemID = TileLoader.GetItemDropFromTypeAndStyle(tile.type, style);

                    if (chestItemID > ItemID.None) {
                        foreach (Item item in chest.item) {
                            if (item != null && !item.IsAir) {
                                AddSource(ChestSources, item.type, chestItemID);
                            }
                        }
                    }
                }
            }


            // --- Populate Grab Bag Sources ---
            for (int type = 1; type < ItemLoader.ItemCount; type++) {
                Item grabBagItem = ContentSamples.ItemsByType[type];
                if (grabBagItem.type <= ItemID.None) continue;

                bool isGrabBag = ItemLoader.CanRightClick(grabBagItem) &&
                                 Main.ItemDropsDB.GetRulesForItemID(grabBagItem.type).Count > 0;

                if (isGrabBag) {
                    List<IItemDropRule> rules = Main.ItemDropsDB.GetRulesForItemID(grabBagItem.type);
                    var lootItems = new HashSet<int>();
                    var list = new List<DropRateInfo>();
                    var ratesInfo = new DropRateInfoChainFeed(1f);
                    foreach (IItemDropRule item in rules) {
                        item.ReportDroprates(list, ratesInfo);
                    }

                    foreach (DropRateInfo item2 in list) {
                        lootItems.Add(item2.itemId);
                    }

                    foreach (int lootId in lootItems) {
                        AddSource(GrabBagSources, lootId, grabBagItem.type);
                    }
                }
            }


            // --- Populate Crafting Sources ---
            var tempCraftingSources = new Dictionary<int, HashSet<int>>();
            for (int i = 0; i < Recipe.numRecipes; i++) {
                Recipe recipe = Main.recipe[i];
                if (recipe.createItem.IsAir) continue;

                int resultType = recipe.createItem.type;
                if (!tempCraftingSources.ContainsKey(resultType)) {
                    tempCraftingSources[resultType] = new HashSet<int>();
                }

                foreach (Item ingredient in recipe.requiredItem) {
                    if (!ingredient.IsAir) {
                        tempCraftingSources[resultType].Add(ingredient.type);
                    }
                }
            }
            foreach (var pair in tempCraftingSources) {
                CraftingSources[pair.Key] = pair.Value.ToList();
            }

            var optionsList = ItemTrader.ChlorophyteExtractinator._options;

            foreach (var option in optionsList) {
                AddSource(ChlorophyteExtractinatorSources, option.GivingITemType, option.TakingItemType);
            }


            PopulateExtractinatorSources();

            InitializeFishingSources();

            InitializeCustomizedSources();

            foreach (var registration in RegisteredCustomSources) {
                string fullKey = registration.Key;
                List<int> itemIDs = registration.Value;

                foreach (int itemID in itemIDs) {
                    if (!CustomizedSources.TryGetValue(itemID, out List<string> sources)) {
                        sources = new List<string>();
                        CustomizedSources[itemID] = sources;
                    }

                    if (!sources.Contains(fullKey)) {
                        sources.Add(fullKey);
                    }
                }
            }
        }

        private void AddSource(Dictionary<int, List<int>> sourcesDict, int key, int sourceValue) {
            if (!sourcesDict.ContainsKey(key)) {
                sourcesDict[key] = new List<int>();
            }

            if (!sourcesDict[key].Contains(sourceValue)) {
                sourcesDict[key].Add(sourceValue);
            }
        }

        private void PopulateExtractinatorSources() {
            // Silt/Slush Drops
            List<int> siltSlushDrops = new List<int>
            {
                ItemID.PlatinumCoin, ItemID.GoldCoin, ItemID.SilverCoin, ItemID.CopperCoin,
                ItemID.AmberMosquito,
                ItemID.Amethyst, ItemID.Topaz, ItemID.Sapphire, ItemID.Emerald, ItemID.Ruby, ItemID.Diamond, ItemID.Amber,
                ItemID.CopperOre, ItemID.TinOre, ItemID.IronOre, ItemID.LeadOre,
                ItemID.SilverOre, ItemID.TungstenOre, ItemID.GoldOre, ItemID.PlatinumOre
            };

            foreach (int drop in siltSlushDrops) {
                AddSource(ExtractinatorSources, drop, ItemID.SiltBlock);
                AddSource(ExtractinatorSources, drop, ItemID.SlushBlock);
                AddSource(ExtractinatorSources, drop, ItemID.DesertFossil);
            }

            AddSource(ExtractinatorSources, ItemID.FossilOre, ItemID.DesertFossil);

            // Junk Drops
            List<int> junkDrops = new List<int> { ItemID.Snail, ItemID.ApprenticeBait, ItemID.Worm, ItemID.JourneymanBait };
            List<int> junkSources = new List<int> { ItemID.OldShoe, ItemID.Seaweed, ItemID.TinCan };
            foreach (int drop in junkDrops) {
                foreach (int source in junkSources) {
                    AddSource(ExtractinatorSources, drop, source);
                }
            }

            List<int> mossTypes = new List<int> {
                ItemID.GreenMoss, ItemID.BrownMoss, ItemID.RedMoss, ItemID.BlueMoss, ItemID.PurpleMoss
            };

            foreach (int sourceMoss in mossTypes) {
                foreach (int dropMoss in mossTypes) {
                    if (sourceMoss == dropMoss) continue;
                    AddSource(ExtractinatorSources, dropMoss, sourceMoss);
                }
            }
        }

        public static void InitializeFishingSources() {
            FishingSources = new Dictionary<int, FishingInfo>{
            // == Fish ==
            { ItemID.ArmoredCavefish, new FishingInfo("Uncommon", false, "Underground", "Cavern", "Underworld") },
            { ItemID.AtlanticCod, new FishingInfo("Common", false, "Snow") },
            { ItemID.Bass, new FishingInfo("Plentiful", false, "Any") },
            { ItemID.BlueJellyfish, new FishingInfo("Rare", false, "Underground", "Cavern", "Underworld") },
            { ItemID.ChaosFish, new FishingInfo("VeryRare", false, "Underground", "Hallow") },
            { ItemID.CrimsonTigerfish, new FishingInfo("Common", false, "Crimson") },
            { ItemID.Damselfish, new FishingInfo("Uncommon", false, "Sky") },
            { ItemID.DoubleCod, new FishingInfo("Uncommon", false, "Surface", "Jungle") },
            { ItemID.Ebonkoi, new FishingInfo("Uncommon", false, "Corruption") },
            { ItemID.FlarefinKoi, new FishingInfo("VeryRare", false, "Lava") },
            { ItemID.Flounder, new FishingInfo("Plentiful", false, "Oasis") },
            { ItemID.FrostMinnow, new FishingInfo("Uncommon", false, "Snow") },
            { ItemID.GoldenCarp, new FishingInfo("ExtremelyRare", false, "Underground") },
            { ItemID.GreenJellyfish, new FishingInfo("Rare", true, "Underground") },
            { ItemID.Hemopiranha, new FishingInfo("Uncommon", false, "Crimson") },
            { ItemID.Honeyfin, new FishingInfo("Uncommon", false, "Honey") },
            { ItemID.NeonTetra, new FishingInfo("Common", false, "Jungle") },
            { ItemID.Obsidifish, new FishingInfo("Rare", false, "Lava") },
            { ItemID.PinkJellyfish, new FishingInfo("Rare", false, "Ocean") },
            { ItemID.PrincessFish, new FishingInfo("Uncommon", false, "Hallow") },
            { ItemID.Prismite, new FishingInfo("Rare", false, "Hallow") },
            { ItemID.RedSnapper, new FishingInfo("Common", false, "Ocean") },
            { ItemID.RockLobster, new FishingInfo("Plentiful", false, "Oasis") },
            { ItemID.Salmon, new FishingInfo("Common", false, "Forest", "LargeLakes") },
            { ItemID.Shrimp, new FishingInfo("Uncommon", false, "Ocean") },
            { ItemID.SpecularFish, new FishingInfo("Common", false, "Underground", "Cavern") },
            { ItemID.Stinkfish, new FishingInfo("Rare", false, "Underground") },
            { ItemID.Trout, new FishingInfo("Plentiful", false, "Forest", "Snow") },
            { ItemID.Tuna, new FishingInfo("Common", false, "Ocean") },
            { ItemID.VariegatedLardfish, new FishingInfo("Uncommon", false, "Underground", "Jungle") },

            // == Usable Items ==
            { ItemID.FrogLeg, new FishingInfo("ExtremelyRare", false, "Any") },
            { ItemID.BalloonPufferfish, new FishingInfo("ExtremelyRare", false, "Any") },
            { ItemID.BombFish, new FishingInfo("Uncommon", false, "Any") },
            { ItemID.PurpleClubberfish, new FishingInfo("Rare", false, "Corruption") },
            { ItemID.ReaverShark, new FishingInfo("VeryRare", false, "Ocean") },
            { ItemID.Rockfish, new FishingInfo("VeryRare", false, "Underground") },
            { ItemID.SawtoothShark, new FishingInfo("VeryRare", false, "Ocean") },
            { ItemID.FrostDaggerfish, new FishingInfo("Uncommon", false, "Snow") },
            { ItemID.Swordfish, new FishingInfo("Rare", false, "Ocean") },
            { ItemID.ZephyrFish, new FishingInfo("ExtremelyRare", false, "Any") },
            { ItemID.Toxikarp, new FishingInfo("ExtremelyRare", true, "Corruption") },
            { ItemID.Bladetongue, new FishingInfo("ExtremelyRare", true, "Crimson") },
            { ItemID.CrystalSerpent, new FishingInfo("ExtremelyRare", true, "Hallow") },
            { ItemID.ScalyTruffle, new FishingInfo("ExtremelyRare", true, "OverlappingIceEvilHallow") },
            { ItemID.ObsidianSwordfish, new FishingInfo("ExtremelyRare", true, "Lava") },
            { ItemID.AlchemyTable, new FishingInfo("VeryRare", false, "Dungeon") },
            { ItemID.Oyster, new FishingInfo("Uncommon", false, "Oasis") },
            { ItemID.CombatBookVolumeTwo, new FishingInfo("ExtremelyRare", false, "BloodMoon") },
            { ItemID.BottomlessLavaBucket, new FishingInfo("ExtremelyRare", false, "Lava") },
            { ItemID.LavaAbsorbantSponge, new FishingInfo("ExtremelyRare", false, "Lava") },
            { ItemID.DemonConch, new FishingInfo("ExtremelyRare", false, "Lava") },
            { ItemID.DreadoftheRedSea, new FishingInfo("ExtremelyRare", false, "BloodMoon") },
            { ItemID.LadyOfTheLake, new FishingInfo("ExtremelyRare", true, "Hallow") },

            // == Crates ==
            { ItemID.WoodenCrate, new FishingInfo("Plentiful", false, "Any") },
            { ItemID.WoodenCrateHard, new FishingInfo("Plentiful", true, "Any") },
            { ItemID.IronCrate, new FishingInfo("Uncommon", false, "Any") },
            { ItemID.IronCrateHard, new FishingInfo("Uncommon", true, "Any") },
            { ItemID.GoldenCrate, new FishingInfo("VeryRare", false, "Any") },
            { ItemID.GoldenCrateHard, new FishingInfo("VeryRare", true, "Any") },
            { ItemID.JungleFishingCrate, new FishingInfo("Rare", false, "Jungle") },
            { ItemID.JungleFishingCrateHard, new FishingInfo("Rare", true, "Jungle") },
            { ItemID.FloatingIslandFishingCrate, new FishingInfo("Rare", false, "Sky") },
            { ItemID.FloatingIslandFishingCrateHard, new FishingInfo("Rare", true, "Sky") },
            { ItemID.CorruptFishingCrate, new FishingInfo("Rare", false, "Corruption") },
            { ItemID.CorruptFishingCrateHard, new FishingInfo("Rare", true, "Corruption") },
            { ItemID.CrimsonFishingCrate, new FishingInfo("Rare", false, "Crimson") },
            { ItemID.CrimsonFishingCrateHard, new FishingInfo("Rare", true, "Crimson") },
            { ItemID.HallowedFishingCrate, new FishingInfo("Rare", false, "Hallow") },
            { ItemID.HallowedFishingCrateHard, new FishingInfo("Rare", true, "Hallow") },
            { ItemID.DungeonFishingCrate, new FishingInfo("Rare", false, "Dungeon") },
            { ItemID.DungeonFishingCrateHard, new FishingInfo("Rare", true, "Dungeon") },
            { ItemID.FrozenCrate, new FishingInfo("Rare", false, "Snow") },
            { ItemID.FrozenCrateHard, new FishingInfo("Rare", true, "Snow") },
            { ItemID.OasisCrate, new FishingInfo("Rare", false, "Desert") },
            { ItemID.OasisCrateHard, new FishingInfo("Rare", true, "Desert") },
            { ItemID.LavaCrate, new FishingInfo("Plentiful", false, "Lava") },
            { ItemID.LavaCrateHard, new FishingInfo("Plentiful", true, "Lava") },
            { ItemID.OceanCrate, new FishingInfo("Rare", false, "Ocean") },
            { ItemID.OceanCrateHard, new FishingInfo("Rare", true, "Ocean") },

            // == Junk ==
            { ItemID.OldShoe, new FishingInfo("Junk", false, "AnyLowPower") },
            { ItemID.Seaweed, new FishingInfo("Junk", false, "AnyLowPower") },
            { ItemID.TinCan, new FishingInfo("Junk", false, "AnyLowPower") },
            { ItemID.JojaCola, new FishingInfo("Junk", false, "AnyLowPower") },

            // == Quest Fish ==
            { ItemID.AmanitaFungifin, new FishingInfo("Quest", false, "GlowingMushroom") },
            { ItemID.Angelfish, new FishingInfo("Quest", false, "Sky") },
            { ItemID.Batfish, new FishingInfo("Quest", false, "Underground", "Cavern") },
            { ItemID.BloodyManowar, new FishingInfo("Quest", false, "Crimson") },
            { ItemID.Bonefish, new FishingInfo("Quest", false, "Underground", "Cavern") },
            { ItemID.BumblebeeTuna, new FishingInfo("Quest", false, "Honey") },
            { ItemID.Bunnyfish, new FishingInfo("Quest", false, "Surface", "Forest") },
            { ItemID.CapnTunabeard, new FishingInfo("Quest", true, "Ocean") },
            { ItemID.Catfish, new FishingInfo("Quest", false, "Surface", "Jungle") },
            { ItemID.Cloudfish, new FishingInfo("Quest", false, "Sky") },
            { ItemID.Clownfish, new FishingInfo("Quest", false, "Ocean") },
            { ItemID.Cursedfish, new FishingInfo("Quest", true, "Corruption") },
            { ItemID.DemonicHellfish, new FishingInfo("Quest", false, "Cavern", "Underworld") },
            { ItemID.Derpfish, new FishingInfo("Quest", true, "Surface", "Jungle") },
            { ItemID.Dirtfish, new FishingInfo("Quest", false, "Surface", "Underground") },
            { ItemID.DynamiteFish, new FishingInfo("Quest", false, "Surface") },
            { ItemID.EaterofPlankton, new FishingInfo("Quest", false, "Corruption") },
            { ItemID.FallenStarfish, new FishingInfo("Quest", false, "Sky", "Surface") },
            { ItemID.Fishotron, new FishingInfo("Quest", false, "Cavern") },
            { ItemID.Fishron, new FishingInfo("Quest", true, "Underground", "Snow") },
            { ItemID.GuideVoodooFish, new FishingInfo("Quest", false, "Cavern", "Underworld") },
            { ItemID.Harpyfish, new FishingInfo("Quest", false, "Sky", "Surface") },
            { ItemID.Hungerfish, new FishingInfo("Quest", true, "Cavern", "Underworld") },
            { ItemID.Ichorfish, new FishingInfo("Quest", true, "Crimson") },
            { ItemID.InfectedScabbardfish, new FishingInfo("Quest", false, "Corruption") },
            { ItemID.Jewelfish, new FishingInfo("Quest", false, "Underground", "Cavern") },
            { ItemID.MirageFish, new FishingInfo("Quest", true, "Underground", "Hallow") },
            { ItemID.Mudfish, new FishingInfo("Quest", false, "Jungle") },
            { ItemID.MutantFlinxfin, new FishingInfo("Quest", false, "Underground", "Snow") },
            { ItemID.Pengfish, new FishingInfo("Quest", false, "Surface", "Snow") },
            { ItemID.Pixiefish, new FishingInfo("Quest", true, "Surface", "Hallow") },
            { ItemID.ScarabFish, new FishingInfo("Quest", false, "Desert") },
            { ItemID.ScorpioFish, new FishingInfo("Quest", false, "Desert") },
            { ItemID.Slimefish, new FishingInfo("Quest", false, "Surface", "Forest") },
            { ItemID.Spiderfish, new FishingInfo("Quest", false, "Underground", "Cavern") },
            { ItemID.TheFishofCthulu, new FishingInfo("Quest", false, "Surface") },
            { ItemID.TropicalBarracuda, new FishingInfo("Quest", false, "Surface", "Jungle") },
            { ItemID.TundraTrout, new FishingInfo("Quest", false, "Surface", "Snow") },
            { ItemID.UnicornFish, new FishingInfo("Quest", true, "Hallow") },
            { ItemID.Wyverntail, new FishingInfo("Quest", true, "Sky") },
            { ItemID.ZombieFish, new FishingInfo("Quest", false, "Surface", "Forest") }
        };
        }

        public static void InitializeCustomizedSources() {
            CustomizedSources = new Dictionary<int, List<string>>();

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

            // 森林树 (Forest trees)
            AddCustomizedSourceToItems("ShakingForestTree",
                ItemID.Acorn, ItemID.Wood, ItemID.Apple, ItemID.Apricot, ItemID.Grapefruit,
                ItemID.Lemon, ItemID.Peach, ItemID.RottenEgg, ItemID.LivingWoodWand,
                ItemID.LeafWand, ItemID.EucaluptusSap
            );

            // 红木树 (Mahogany trees)
            AddCustomizedSourceToItems("ShakingMahoganyTree",
                ItemID.RichMahogany, ItemID.Mango, ItemID.Pineapple, ItemID.RottenEgg,
                ItemID.LivingMahoganyWand, ItemID.LivingMahoganyLeafWand
            );

            // 乌木树 (Ebonwood trees)
            AddCustomizedSourceToItems("ShakingEbonwoodTree",
                ItemID.Ebonwood, ItemID.Elderberry, ItemID.BlackCurrant, ItemID.RottenEgg
            );

            // 暗影木树 (Shadewood trees)
            AddCustomizedSourceToItems("ShakingShadewoodTree",
                ItemID.Shadewood, ItemID.BloodOrange, ItemID.Rambutan, ItemID.RottenEgg
            );

            // 珍珠木树 (Pearlwood trees)
            AddCustomizedSourceToItems("ShakingPearlwoodTree",
                ItemID.Acorn, ItemID.Pearlwood, ItemID.Dragonfruit, ItemID.Starfruit,
                ItemID.RottenEgg
            );

            // 棕榈树 (Palm trees)
            AddCustomizedSourceToItems("ShakingPalmTree",
                ItemID.PalmWood, ItemID.Coconut, ItemID.Banana, ItemID.RottenEgg
            );

            // 灰烬树 (Ash trees)
            AddCustomizedSourceToItems("ShakingAshTree",
                ItemID.Acorn, ItemID.AshWood, ItemID.SpicyPepper, ItemID.Pomegranate,
                ItemID.RottenEgg
            );

            // 巨型夜光蘑菇 (Giant Glowing Mushrooms)
            AddCustomizedSourceToItems("ShakingGlowingMushroom",
                ItemID.MushroomGrassSeeds, ItemID.GlowingMushroom, ItemID.RottenEgg
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





            string templateBiomeHardmode = Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.DroppedInBiomeHardmode");
            string templateBiome = Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.DroppedInBiome");

            // 使用 DroppedInBiomeHardmode 模板
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.UndergroundHallow")), ItemID.SoulofLight);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.UndergroundCorruption") + "/" + Language.GetTextValue("Bestiary_Biomes.UndergroundCrimson")), ItemID.SoulofNight);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.Ocean")),ItemID.PirateMap);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.TheUnderworld")), ItemID.LivingFireBlock, ItemID.HelFire);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.TheSnow")),ItemID.Amarok);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.TheDungeon")),ItemID.Kraken);

            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.Jungle")), ItemID.JungleKey);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.TheCorruption")), ItemID.CorruptionKey);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.Crimson")), ItemID.CrimsonKey);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.TheHallow")), ItemID.HallowedKey);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.Snow")), ItemID.FrozenKey);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Biomes.Desert")), ItemID.DungeonDesertKey);

            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Events.Halloween")), ItemID.Present);
            AddCustomizedSourceToItemsString(string.Format(templateBiomeHardmode, Language.GetTextValue("Bestiary_Events.Christmas")), ItemID.GoodieBag, ItemID.BloodyMachete, ItemID.BladedGlove);

            // --- Pre-Hardmode Underworld Yoyo Drop (Post-Skeletron) ---
            AddCustomizedSourceToItems("CascadeDrop",
                ItemID.Cascade
            );

            // --- Hardmode Jungle Yoyo Drop (Post-Mech Boss) ---
            AddCustomizedSourceToItems("YeletsDrop",
                ItemID.Yelets
            );
        }


        public static void AddCustomizedSourceToItems(string key, params int[] itemIDs) {
            foreach (int itemID in itemIDs) {
                var source = Language.GetTextValue($"Mods.MoreObtainingTooltips.Tooltips.{key}");

                if (!CustomizedSources.TryGetValue(itemID, out List<string> sources)) {
                    sources = new List<string>();
                    CustomizedSources[itemID] = sources;
                }

                if (!sources.Contains(source)) {
                    sources.Add(source);
                }
            }
        }

        public static void AddCustomizedSourceToItemsString(string source, params int[] itemIDs) {
            foreach (int itemID in itemIDs) {

                if (!CustomizedSources.TryGetValue(itemID, out List<string> sources)) {
                    sources = new List<string>();
                    CustomizedSources[itemID] = sources;
                }

                if (!sources.Contains(source)) {
                    sources.Add(source);
                }
            }
        }

        public static void RegisterCustomSource(string fullLocalizationKey, IEnumerable<int> itemIDs) {
            if (!RegisteredCustomSources.ContainsKey(fullLocalizationKey)) {
                RegisteredCustomSources[fullLocalizationKey] = new List<int>();
            }
            RegisteredCustomSources[fullLocalizationKey]
                .AddRange(itemIDs.Where(id => !RegisteredCustomSources[fullLocalizationKey].Contains(id)));
        }
    }
}