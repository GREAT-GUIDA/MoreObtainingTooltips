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
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public static Dictionary<int, List<SourceInfo>> BreakTileSources { get; set; } = new();
        public static Dictionary<int, List<SourceInfo>> ShimmerSources { get; set; } = new();
        public static Dictionary<int, List<SourceInfo>> DecraftSources { get; set; } = new();
        public static Dictionary<int, List<SourceInfo>> DropSources { get; set; } = new();
        public static Dictionary<int, List<SourceInfo>> GrabBagSources { get; set; } = new();
        public static Dictionary<int, List<SourceInfo>> CraftingSources { get; set; } = new();
        public static Dictionary<int, List<SourceInfo>> ShopSources { get; set; } = new();
        public static Dictionary<int, List<SourceInfo>> ChestSources { get; set; } = new();
        public static Dictionary<int, List<SourceInfo>> CatchNPCSources { get; set; } = new();
        public static Dictionary<int, List<SourceInfo>> NPCBannerSources { get; set; } = new();
        public static Dictionary<int, List<SourceInfo>> FishingSources { get; set; } = new();
        public static Dictionary<int, List<SourceInfo>> ExtractinatorSources { get; set; } = new();
        public static Dictionary<int, List<SourceInfo>> ChlorophyteExtractinatorSources { get; set; } = new();
        public static Dictionary<int, List<SourceInfo>> CustomizedSources { get; set; } = new();

        public static Dictionary<string, List<int>> RegisteredCustomSources = new();
        
        private static bool _loadedChestSourcesFromTag = false;
        public static Dictionary<int, string> MusicIdToMusicName { get; set; } = new();

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
        private string GetItemSavableString(int id) {
            if (id < ItemID.Count)
                return id.ToString();

            return ItemLoader.GetItem(id)?.FullName;
        }

        private string GetTileSavableString(int id) {
            if (id < TileID.Count)
                return id.ToString();

            return TileLoader.GetTile(id)?.FullName;
        }

        private int GetItemIDFromSavable(string value) {
            if (string.IsNullOrEmpty(value))
                return -1;

            if (int.TryParse(value, out int intId)) {
                return intId;
            }

            if (ModContent.TryFind<ModItem>(value, out var modItem)) {
                return modItem.Type;
            }

            return -1;
        }

        private int GetTileIDFromSavable(string value) {
            if (string.IsNullOrEmpty(value))
                return -1;

            if (int.TryParse(value, out int intId)) {
                return intId;
            }

            if (ModContent.TryFind<ModTile>(value, out var modTile)) {
                return modTile.Type;
            }

            return -1;
        }

        private void SaveSourceDictionary(TagCompound tag, string tagName, Dictionary<int, List<SourceInfo>> dictionary, Func<int, string> keyConverter, Func<int, string> sourceConverter) {
            if (dictionary == null || dictionary.Count == 0) return;

            var list = new List<TagCompound>();
            foreach (var pair in dictionary) {
                string keyName = keyConverter(pair.Key);
                if (keyName == null) continue;

                List<string> sourceNames = pair.Value
                  .Select(source => sourceConverter(source.id))
                  .Where(name => name != null)
                  .ToList();

                if (sourceNames.Count > 0) {
                    list.Add(new TagCompound {
                        ["key"] = keyName,
                        ["sources"] = sourceNames
                    });
                }
            }
            tag[tagName] = list;
        }

        private Dictionary<int, List<SourceInfo>> LoadSourceDictionary(TagCompound tag, string tagName, Func<string, int> keyConverter, Func<string, int> sourceConverter) {
            var dictionary = new Dictionary<int, List<SourceInfo>>();
            if (tag.TryGet(tagName, out List<TagCompound> list)) {
                foreach (var entryTag in list) {
                    if (entryTag.TryGet("key", out string keyName) &&
                      entryTag.TryGet("sources", out List<string> sourceNames)) {

                        int key = keyConverter(keyName);
                        if (key != -1) {
                            List<SourceInfo> sources = sourceNames
                                            .Select(name => new SourceInfo(sourceConverter(name)))
                                            .Where(source => source.id != -1)
                                    .ToList();

                            if (sources.Count > 0) {
                                dictionary[key] = sources;
                            }
                        }
                    }
                }
            }
            return dictionary;
        }

        public override void SaveWorldData(TagCompound tag) {
            SaveSourceDictionary(tag, "ChestSources", ChestSources, GetItemSavableString, GetItemSavableString);
            SaveSourceDictionary(tag, "BreakTileSources", BreakTileSources, GetItemSavableString, GetTileSavableString);
        }

        public override void LoadWorldData(TagCompound tag) {
            ChestSources = LoadSourceDictionary(tag, "ChestSources", GetItemIDFromSavable, GetItemIDFromSavable);
            _loadedChestSourcesFromTag = ChestSources.Count > 0;

            BreakTileSources = LoadSourceDictionary(tag, "BreakTileSources", GetItemIDFromSavable, GetTileIDFromSavable);
            _loadedBreakTileSourcesFromTag = BreakTileSources.Count > 0;
        }
        public override void OnWorldUnload() {
            ChestSources.Clear();
            _loadedChestSourcesFromTag = false;
            BreakTileSources.Clear();
            _loadedBreakTileSourcesFromTag = false;
        }
        public override void PostUpdateTime() {
            //ReloadInfo();
        }
        public override void PostWorldLoad() {
            ReloadInfo();
        }

        public static void ReloadInfo() {
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
                        if (sourceList.Count >= 20 || sourceList[0].id == -1) {
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
                        chestItemID = -3;
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
            void AddItemSetsToBagSource(int bagId, IEnumerable<int[]> itemSets) {
                foreach (var set in itemSets) {
                    foreach (int itemId in set) {
                        AddSource(GrabBagSources, itemId, bagId);
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
            };
            var devSetsAsInts = ModLoaderMod.DeveloperSets.Select(set => set.Select(item => item.Type).ToArray());
            var patronSetsAsInts = ModLoaderMod.PatronSets.Select(set => set.Select(item => item.Type).ToArray());
            for (int bagId = ItemID.None + 1; bagId < ItemLoader.ItemCount; bagId++) {
                if (ItemID.Sets.BossBag[bagId] && !ItemID.Sets.PreHardmodeLikeBossBag[bagId]) {
                    AddItemSetsToBagSource(bagId, goodieBagDeveloperSets);
                    
                    AddItemSetsToBagSource(bagId, devSetsAsInts);
                    AddItemSetsToBagSource(bagId, patronSetsAsInts);
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
                        if(DecraftSources.TryGetValue(resultType, out var decraftSources)) {
                            if (decraftSources.Contains(new SourceInfo(ingredient.type))) continue;
                        }
                        AddSource(CraftingSources, resultType, ingredient.type);
                    }
                }
            }

            var optionsList = ItemTrader.ChlorophyteExtractinator._options;

            foreach (var option in optionsList) {
                AddSource(ChlorophyteExtractinatorSources, option.GivingITemType, option.TakingItemType);
            }

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

            DataPopulator.InitializeFishingSources(FishingSources);

            DataPopulator.InitializeCustomizedSources(CustomizedSources);
        }

        public static void AddSource(Dictionary<int, List<SourceInfo>> sourcesDict, int key, SourceInfo sourceValue) {
            if (!sourcesDict.ContainsKey(key)) {
                sourcesDict[key] = new List<SourceInfo>();
            }

            if (!sourcesDict[key].Contains(sourceValue)) {
                sourcesDict[key].Add(sourceValue);
            }
        }

        public static void PopulateExtractinatorSources() {
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