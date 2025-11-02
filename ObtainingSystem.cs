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
using System;
using System.Net.Sockets;
using static System.Net.Mime.MediaTypeNames;
using static Terraria.ModLoader.BackupIO;
using static Terraria.ModLoader.NPCShopDatabase;
using ReLogic.Utilities;
using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;
using System.IO;
using System.Reflection;
using Player = Terraria.Player;
using Terraria.ModLoader.Default;
using static MoreObtainingTooltips.ObtainingSystem;

namespace MoreObtainingTooltips {
    public class ObtainingSystem : ModSystem {
        public struct FishingInfo {
            public string RarityKey;
            public List<string> EnvironmentKeys;
            public bool IsHardmode;

            public FishingInfo(string rarityKey, bool isHardmode, params string[] environmentKeys) {
                RarityKey = rarityKey;
                EnvironmentKeys = new List<string>(environmentKeys);
                IsHardmode = isHardmode;
            }
        }
        public struct SourceInfo {
            public int id;
            public int num;
            public string str;
            public SourceInfo(int id, int num = 0, string str = "") {
                this.id = id;
                this.num = num;
                this.str = str;
            }

            public static implicit operator SourceInfo(int id) => new SourceInfo(id);
            public static implicit operator SourceInfo(string str) => new SourceInfo(0, 0, str);
            public bool Equals(SourceInfo other) {
                return id == other.id && num == other.num && str == other.str;
            }

            public override bool Equals(object obj) {
                return obj is SourceInfo other && Equals(other);
            }

            public override int GetHashCode() {
                return id.GetHashCode();
            }

            public static bool operator ==(SourceInfo left, SourceInfo right) {
                return left.Equals(right);
            }

            public static bool operator !=(SourceInfo left, SourceInfo right) {
                return !left.Equals(right);
            }
        }
        public static Dictionary<int, List<SourceInfo>> BreakTileSources { get; private set; } = new();
        public static Dictionary<int, List<SourceInfo>> ShimmerSources { get; private set; } = new();
        public static Dictionary<int, List<SourceInfo>> DecraftSources { get; private set; } = new();
        public static Dictionary<int, List<SourceInfo>> DropSources { get; private set; } = new();
        public static Dictionary<int, List<SourceInfo>> GrabBagSources { get; private set; } = new();
        public static Dictionary<int, List<SourceInfo>> CraftingSources { get; private set; } = new();
        public static Dictionary<int, List<SourceInfo>> ShopSources { get; private set; } = new();
        public static Dictionary<int, List<SourceInfo>> ChestSources { get; private set; } = new();
        public static Dictionary<int, List<SourceInfo>> CatchNPCSources { get; private set; } = new();
        public static Dictionary<int, List<SourceInfo>> NPCBannerSources { get; private set; } = new();
        public static Dictionary<int, List<SourceInfo>> FishingSources { get; private set; } = new();
        public static Dictionary<int, List<SourceInfo>> ExtractinatorSources { get; private set; } = new();
        public static Dictionary<int, List<SourceInfo>> ChlorophyteExtractinatorSources { get; private set; } = new();
        public static Dictionary<int, List<SourceInfo>> CustomizedSources { get; private set; } = new();

        public static Dictionary<string, List<int>> RegisteredCustomSources = new();
        
        private static bool _loadedChestSourcesFromTag = false;
        public static Dictionary<int, string> MusicIdToMusicName { get; private set; } = new();

        private static bool _loadedBreakTileSourcesFromTag = false;
        public override void OnModLoad() {
        }
        public override void PostSetupContent() {
            MusicIdToMusicName.Clear();

            foreach (FieldInfo field in typeof(MusicID).GetFields(BindingFlags.Public | BindingFlags.Static)) {
                if (field.FieldType == typeof(short)) {
                    int musicId = (short)field.GetValue(null);
                    string musicName = field.Name;
                    MusicIdToMusicName[musicId] = musicName;
                }
            }
        }
        // 保存世界数据
        public override void SaveWorldData(TagCompound tag) {
            if (ChestSources != null && ChestSources.Count > 0) {
                var list = new List<TagCompound>();
                foreach (var pair in ChestSources) {
                    list.Add(new TagCompound {
                        ["key"] = pair.Key,
                        ["sources"] = pair.Value.Select(source => source.id).ToList()
                    });
                }
                tag["ChestSources"] = list;
            }

            // 修正: < 0 改为 > 0
            if (BreakTileSources != null && BreakTileSources.Count > 0) {
                var breakTileList = new List<TagCompound>();
                foreach (var pair in BreakTileSources) {
                    breakTileList.Add(new TagCompound {
                        ["key"] = pair.Key,
                        // 同样在这里提取 id
                        ["sources"] = pair.Value.Select(source => source.id).ToList()
                    });
                }
                tag["BreakTileSources"] = breakTileList;
            }
        }

        // 加载世界数据
        public override void LoadWorldData(TagCompound tag) {
            /*ChestSources.Clear();
            _loadedChestSourcesFromTag = false;
            if (tag.TryGet("ChestSources", out List<TagCompound> list)) {
                foreach (var entryTag in list) {
                    if (entryTag.TryGet("key", out int key) && entryTag.TryGet("sources", out List<int> sourceIds)) {
                        ChestSources[key] = sourceIds.Select(id => new SourceInfo(id)).ToList();
                    }
                }
                _loadedChestSourcesFromTag = true;
            }*/

            BreakTileSources.Clear();
            _loadedBreakTileSourcesFromTag = false;
            if (tag.TryGet("BreakTileSources", out List<TagCompound> breakTileList)) {
                foreach (var entryTag in breakTileList) {
                    if (entryTag.TryGet("key", out int key) && entryTag.TryGet("sources", out List<int> sourceIds)) {
                        BreakTileSources[key] = sourceIds.Select(id => new SourceInfo(id)).ToList();
                    }
                }
                _loadedBreakTileSourcesFromTag = true;
            }
        }

        public override void OnWorldUnload() {
            ChestSources.Clear();
            _loadedChestSourcesFromTag = false;
            BreakTileSources.Clear();
            _loadedBreakTileSourcesFromTag = false;
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
            NPCBannerSources.Clear();
            FishingSources.Clear();
            ExtractinatorSources.Clear();
            ChlorophyteExtractinatorSources.Clear();

            // --- Populate Breakable Tile Sources ---
            if (!_loadedBreakTileSourcesFromTag) {
                for (int x = 0; x < Main.maxTilesX; x++) {
                    for (int y = 0; y < Main.maxTilesY; y++) {
                        Tile tile = Main.tile[x, y];
                        if (tile == null || !tile.HasTile) {
                            continue;
                        }
                        ModTile modTile = TileLoader.GetTile(tile.TileType);
                        if (modTile != null) {
                            IEnumerable<Item> itemDrops = modTile.GetItemDrops(x, y);
                            if (itemDrops != null) {
                                foreach (Item item in itemDrops) {
                                    AddSource(BreakTileSources, item.type, tile.type);
                                }
                                continue;
                            }
                        }

                        WorldGen.KillTile_GetItemDrops(x, y, tile, out var dropItem, out var dropItemStack, out var secondaryItem, out var secondaryItemStack, true);
                        if (dropItem > ItemID.None) {
                            AddSource(BreakTileSources, dropItem, tile.type);
                        }
                        if (secondaryItem > ItemID.None) {
                            AddSource(BreakTileSources, secondaryItem, tile.type);
                        }

                        var dropItemID = TileLoader.GetItemDropFromTypeAndStyle(tile.type, TileObjectData.GetTileStyle(tile));
                        if (dropItemID > ItemID.None) {
                            AddSource(BreakTileSources, dropItemID, tile.type);
                        }
                    }
                }
            }

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

            // --- Populate NPC Banner Sources ---
            for (int i = -10; i < NPCLoader.NPCCount; i++) {
                NPC npc = new NPC();
                npc.SetDefaults(i);
                NPCLoader.SetDefaults(npc);
                var it = Item.BannerToItem(Item.NPCtoBanner(npc.BannerID()));
                if (it > ItemID.None) {
                    AddSource(NPCBannerSources, it, new SourceInfo(npc.BannerID(), ItemID.Sets.KillsToBanner[it]));
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
                            .Select(text => text.StartsWith("需要") ? text.Substring(2) : text)
                            .ToList();
                        
                        var sourceInfo = new SourceInfo(shop.NpcType, 0, string.Join(", ", conditionTexts));
                        AddSource(ShopSources, entry.Item.type, sourceInfo);

                        ShopSources.TryGetValue(entry.Item.type, out var sourceList);
                        if (sourceList.Count >= 21 || sourceList[0].id == -1) {
                            sourceList.Clear();
                            sourceList.Add(new SourceInfo(-1, 0, string.Join(", ", conditionTexts)));
                        }
                    }
                }
            }
            int[] travelerItemIds = {
                2260, 2261, 2262, 4555, 4556, 4557, 4321, 4322,
                4323, 4324, 4365, 5390, 5386, 5387, 4666, 4664, 4665,
                3637, 3642, 3621, 3622, 3634, 3639, 3633, 3638, 3635, 3640, 3636, 3641
            };
            foreach (int itemId in travelerItemIds) {
                AddSource(ShopSources, itemId, new SourceInfo(NPCID.TravellingMerchant));
            }

            // --- Populate Chest Sources ---
            if (!_loadedChestSourcesFromTag) {
                for (int i = 0; i < Main.maxChests; i++) {
                    Chest chest = Main.chest[i];
                    if (chest == null) continue;
                    Tile tile = Main.tile[chest.x, chest.y];
                    if (tile == null || (!TileID.Sets.IsAContainer[tile.type] && !Main.tileContainer[tile.type])) continue;

                    int style = TileObjectData.GetTileStyle(tile);
                    int chestItemID = TileLoader.GetItemDropFromTypeAndStyle(tile.type, style);
                    bool locked = false;
                    string name = Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.UnknownChest");

                    if (chestItemID <= ItemID.None) {
                        if (tile.type == 21) {
                            if(style >= 23 && style <= 27) chestItemID = TileLoader.GetItemDropFromTypeAndStyle(tile.type, style - 5);
                            else chestItemID = TileLoader.GetItemDropFromTypeAndStyle(tile.type, style - 1);
                        }
                        if(tile.type >= TileID.Count){
                            for (var j = style - 1; j >= 0; j -= 1) {
                                chestItemID = TileLoader.GetItemDropFromTypeAndStyle(tile.type, j);
                                if (chestItemID > ItemID.None) {
                                    break;
                                }
                            }
                        }
                        if (chestItemID > ItemID.None) {
                            locked = true;
                        }
                    }
                    if (tile.type == 21)
                        name = Lang.chestType[tile.frameX / 36].Value;
                    else if (tile.type == 467 && tile.frameX / 36 == 4)
                        name = Lang.GetItemNameValue(3988);
                    else if (tile.type == 467)
                        name = Lang.chestType2[tile.frameX / 36].Value;
                    else if (tile.type == 88)
                        name = Lang.dresserType[tile.frameX / 54].Value;
                    else if (TileID.Sets.BasicChest[tile.type] || TileID.Sets.BasicDresser[tile.type])
                        name = TileLoader.DefaultContainerName(tile.type, tile.TileFrameX, tile.TileFrameY);

                    if(name == string.Empty) name = Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.UnknownChest");

                    if (chestItemID <= ItemID.None) {
                        chestItemID = 1;
                    }
                    foreach (Item item in chest.item) {
                        if (item != null && !item.IsAir) {
                            AddSource(ChestSources, item.type, chestItemID);
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
            int[][] goodieBagDeveloperSets = {
                new[] { 666, 667, 668, 665, 3287 }, new[] { 1554, 1555, 1556, 1586 },
                new[] { 1554, 1587, 1588, 1586 }, new[] { 1557, 1558, 1559, 1585 },
                new[] { 1560, 1561, 1562, 1584 }, new[] { 1563, 1564, 1565, 3582 },
                new[] { 1566, 1567, 1568 }, new[] { 1580, 1581, 1582, 1583 },
                new[] { 3226, 3227, 3228, 3288 }, new[] { 3583, 3581, 3578, 3579, 3580 },
                new[] { 3585, 3586, 3587, 3588, 3024 }, new[] { 3589, 3590, 3591, 3592, 3599 },
                new[] { 3368, 3921, 3922, 3923, 3924 }, new[] { 3925, 3926, 3927, 3928, 3929 },
                new[] { 4732, 4733, 4734, 4730 }, new[] { 4747, 4748, 4749, 4746 },
                new[] { 4751, 4752, 4753, 4750 }, new[] { 4755, 4756, 4757, 4754 } 
            }
            ;
            for (int bagId = ItemID.None + 1; bagId < ItemLoader.ItemCount; bagId++) {
                if (ItemID.Sets.BossBag[bagId] && !ItemID.Sets.PreHardmodeLikeBossBag[bagId]) {
                    for (int i = 0; i < ModLoaderMod.DeveloperSets.Length; i++) {
                        for (int j = 0; j < ModLoaderMod.DeveloperSets[i].Length; j++) {
                            int devItemId = ModLoaderMod.DeveloperSets[i][j].Type;
                            AddSource(GrabBagSources, devItemId, bagId);
                        }
                    }
                    for (int i = 0; i < ModLoaderMod.PatronSets.Length; i++) {
                        for (int j = 0; j < ModLoaderMod.PatronSets[i].Length; j++) {
                            int patronItemId = ModLoaderMod.PatronSets[i][j].Type;
                            AddSource(GrabBagSources, patronItemId, bagId);
                        }
                    }
                    foreach (int[] set in goodieBagDeveloperSets) {
                        foreach (int devItemId in set) {
                            AddSource(GrabBagSources, devItemId, bagId);
                        }
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
                        AddSource(CraftingSources, resultType, ingredient.type);
                    }
                }
            }

            var optionsList = ItemTrader.ChlorophyteExtractinator._options;

            foreach (var option in optionsList) {
                AddSource(ChlorophyteExtractinatorSources, option.GivingITemType, option.TakingItemType);
            }


            PopulateExtractinatorSources();

            InitializeFishingSources();

            InitializeCustomizedSources();

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

        private void AddSource(Dictionary<int, List<SourceInfo>> sourcesDict, int key, SourceInfo sourceValue) {
            if (!sourcesDict.ContainsKey(key)) {
                sourcesDict[key] = new List<SourceInfo>();
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
        public enum FishingRarity {
            Junk = -1, Quest = 0, Plentiful = 1, Common = 2, Uncommon = 3,
            Rare = 4, VeryRare = 5, ExtremelyRare = 6
        }
        public enum FishingSuffix {
            None, Hardmode, 
            PostWoS, PostSupreme,
            PostDesertScourge, PostLeviathan, PostProvidence
        }
        public static void InitializeFishingSources() {
            FishingSources = new Dictionary<int, List<SourceInfo>>();

            void AddFishingSource(int itemId, FishingRarity rarity, FishingSuffix suffix, params string[] environmentKeys) {
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
                    id: (int)rarity,                 // 存入稀有度ID
                    num: (int)suffix,         // 存入是否为困难模式
                    str: localizedEnvironmentText    // 直接存入处理好的环境字符串
                );
                FishingSources[itemId] = new List<SourceInfo> { source };
            }
            void AddModdedFishingSource(string modName, string itemName, FishingRarity rarity, FishingSuffix suffix, params string[] environmentKeys) {
                int itemId = MoreObtainingTooltips.GetModItemId(modName, itemName);
                if (itemId > 0) {
                    AddFishingSource(itemId, rarity, suffix, environmentKeys);
                }
            }
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
            AddFishingSource(ItemID.AmanitaFungifin, FishingRarity.Quest, FishingSuffix.None, "SurfaceMushroom", "UndergroundMushroom");
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
            const string sots = "SOTS";

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


        public static void InitializeCustomizedSources() {
            CustomizedSources = new Dictionary<int, List<SourceInfo>>();

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
                ItemID.Lemon, ItemID.Peach,  ItemID.LivingWoodWand,
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

            AddCustomizedSourceToItems("PlayerDeath",321,1173,1174,1175,1176,1177,3230,3231,3229,3233,3232);


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
            AddCustomizedSourceToItems("CascadeDrop", ItemID.Cascade);

            // --- Hardmode Jungle Yoyo Drop (Post-Mech Boss) ---
            AddCustomizedSourceToItems("YeletsDrop", ItemID.Yelets);



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
                if (MoreObtainingTooltips.TryGetModItemId(calamity, baseItemName, out int baseItemId)) {
                    string sourceText = string.Format(Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.ExhumedFrom"), Lang.GetItemNameValue(baseItemId));
                    AddCustomizedSourceToModItemsString(sourceText, calamity, upgradedItemName);
                }
            }
        }

        public static void AddCustomizedSourceToModItems(string key, string modName, params string[] itemNames) {
            var source = Language.GetTextValue($"Mods.MoreObtainingTooltips.Tooltips.{key}");

            foreach (string itemName in itemNames) {
                int itemID = MoreObtainingTooltips.GetModItemId(modName, itemName);

                if (itemID > ItemID.None) {
                    if (!CustomizedSources.TryGetValue(itemID, out List<SourceInfo> sources)) {
                        sources = new List<SourceInfo>();
                        CustomizedSources[itemID] = sources;
                    }
                    if (!sources.Contains(source)) {
                        sources.Add(source);
                    }
                }
            }
        }

        public static void AddCustomizedSourceToModItemsString(string source, string modName, params string[] itemNames) {
            foreach (string itemName in itemNames) {
                int itemID = MoreObtainingTooltips.GetModItemId(modName, itemName);

                if (itemID > ItemID.None) {
                    if (!CustomizedSources.TryGetValue(itemID, out List<SourceInfo> sources)) {
                        sources = new List<SourceInfo>();
                        CustomizedSources[itemID] = sources;
                    }
                    if (!sources.Contains(source)) {
                        sources.Add(source);
                    }
                }
            }
        }
        public static void AddCustomizedSourceToItems(string key, params int[] itemIDs) {
            foreach (int itemID in itemIDs) {
                var source = Language.GetTextValue($"Mods.MoreObtainingTooltips.Tooltips.{key}");

                if (!CustomizedSources.TryGetValue(itemID, out List<SourceInfo> sources)) {
                    sources = new List<SourceInfo>();
                    CustomizedSources[itemID] = sources;
                }

                if (!sources.Contains(source)) {
                    sources.Add(source);
                }
            }
        }

        public static void AddCustomizedSourceToItemsString(string source, params int[] itemIDs) {
            foreach (int itemID in itemIDs) {

                if (!CustomizedSources.TryGetValue(itemID, out List<SourceInfo> sources)) {
                    sources = new List<SourceInfo>();
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