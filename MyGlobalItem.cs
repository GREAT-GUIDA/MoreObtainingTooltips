using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using static MoreObtainingTooltips.ObtainingSystem;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Terraria.Localization;
using System;
using Terraria.Map;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace MoreObtainingTooltips {
    public class MyGlobalItem : GlobalItem {
        private static TooltipConfig _config;
        private static TooltipConfig Config => _config ??= ModContent.GetInstance<TooltipConfig>();

        private string GenerateItemSourceTooltip(string format, List<SourceInfo> sources, int maxCount) {
            var distinctSources = sources.Distinct().ToList();
            if (!distinctSources.Any()) return null;

            var itemIcons = new StringBuilder();
            int countToShow = Math.Min(distinctSources.Count, maxCount);

            for (int i = 0; i < countToShow; i++) {
                int itemId = distinctSources[i].id;
                if (itemId == -3) {
                    itemIcons.Append(GetText("UnknownChest"));
                    continue;
                }
                if (Config.ItemShowMode == ItemShowMode.Icon) {
                    itemIcons.Append($"[i:{itemId}]");
                }
                if (Config.ItemShowMode == ItemShowMode.Name) {
                    if (i != 0) itemIcons.Append(", ");
                    itemIcons.Append(Lang.GetItemNameValue(itemId));
                }
                if (Config.ItemShowMode == ItemShowMode.IconAndName) {
                    itemIcons.Append($"[i:{itemId}]");
                    itemIcons.Append($"({Lang.GetItemNameValue(itemId)})");
                }
            }

            var tooltipText = new StringBuilder(string.Format(format, itemIcons));

            if (distinctSources.Count > countToShow) {
                tooltipText.Append(Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.More", distinctSources.Count - countToShow));
            }

            return tooltipText.ToString();
        }

        private string GenerateNpcSourceTooltip(string format, List<SourceInfo> npcSources, int maxCount) {
            var uniqueNames = npcSources
                .Select(source => Lang.GetNPCNameValue(source.id))
                .Distinct()
                .ToList();

            if (!uniqueNames.Any()) return null;

            string nameList = string.Join(", ", uniqueNames.Take(maxCount));
            var tooltipText = new StringBuilder(string.Format(format, nameList));

            if (uniqueNames.Count > maxCount) {
                tooltipText.Append(Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.More", uniqueNames.Count - maxCount));
            }

            return tooltipText.ToString();
        }

        private string GenerateShopSourceTooltip(string format, List<SourceInfo> sources, int maxCount) {
            var distinctSources = sources.Distinct().ToList();
            if (!distinctSources.Any()) return null;

            var tooltipParts = new List<string>();
            int countToShow = System.Math.Min(distinctSources.Count, maxCount);

            for (int i = 0; i < countToShow; i++) {
                var source = distinctSources[i];
                string npcName = Lang.GetNPCNameValue(source.id);
                if (source.id == -1) npcName = GetText("AnyNPC");
                if (source.str != "" && Config.ShowShopCondition) {
                    tooltipParts.Add($"{npcName} ({source.str})");
                } else {
                    tooltipParts.Add(npcName);
                }
            }

            string nameList = string.Join(", ", tooltipParts);
            var tooltipText = new StringBuilder(string.Format(format, nameList));

            if (distinctSources.Count > countToShow) {
                tooltipText.Append(Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.More", distinctSources.Count - countToShow));
            }

            return tooltipText.ToString();
        }

        private string GenerateTileSourceTooltip(string format, List<SourceInfo> tileTypes, int maxCount) {
            var uniqueNames = tileTypes
                .Select(sourceInfo => {
                    int tileId = sourceInfo.id;
                    string name = Lang.GetMapObjectName(MapHelper.TileToLookup(tileId, 0));
                    if (string.IsNullOrEmpty(name)) {
                        name = TileID.Search.GetName(tileId);
                    }
                    return name;
                })
                .Distinct()
                .ToList();

            if (!uniqueNames.Any()) return null;

            string nameList = string.Join(", ", uniqueNames.Take(maxCount));
            var tooltipText = new StringBuilder(string.Format(format, nameList));

            if (uniqueNames.Count > maxCount) {
                tooltipText.Append(Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.More", uniqueNames.Count - maxCount));
            }

            return tooltipText.ToString();
        }

        private static string GetText(string key) => Language.GetTextValue($"Mods.MoreObtainingTooltips.Tooltips.{key}");

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
            if (item.type <= ItemID.None || Config == null || Config.ShowMode == TooltipShowMode.Never) {
                return;
            }

            var obtainingMethods = new List<string>();

            void TryAddMethod(string line) {
                if (string.IsNullOrEmpty(line)) {
                    return;
                }

                if (line.Length <= Config.MaxTooltipLength) {
                    obtainingMethods.Add(line);
                    return;
                }

                int currentPosition = 0;
                while (currentPosition < line.Length) {
                    int segmentEnd = System.Math.Min(currentPosition + Config.MaxTooltipLength, line.Length);
                    string segment = line.Substring(currentPosition, segmentEnd - currentPosition);

                    if (segmentEnd == line.Length) {
                        obtainingMethods.Add(segment.TrimStart());
                        break;
                    }

                    int breakIndexInSegment = -1;

                    int searchStart = (segment.Length > Config.MaxTooltipLength / 2) ? Config.MaxTooltipLength / 2 : 0;

                    int commaIndex = segment.LastIndexOf(", ", segment.Length - 1, segment.Length - searchStart);
                    int spaceIndex = segment.LastIndexOf(' ', segment.Length - 1, segment.Length - searchStart);

                    breakIndexInSegment = System.Math.Max(commaIndex, spaceIndex);

                    if (breakIndexInSegment > 0) {
                        string lineSegment = segment.Substring(0, breakIndexInSegment);
                        obtainingMethods.Add(lineSegment.TrimStart());

                        int skipLength = (segment[breakIndexInSegment] == ',') ? 2 : 1;
                        currentPosition += breakIndexInSegment + skipLength;
                    } else {
                        obtainingMethods.Add(segment.TrimStart());
                        currentPosition += segment.Length;
                    }
                }
            }

            // Breaking Tiles
            if (Config.BreakableTiles.Enabled && BreakTileSources.TryGetValue(item.type, out var tileSources))
                TryAddMethod(Config.BreakableTiles.MaxCount == 0 ? GetText("Breakable") : GenerateTileSourceTooltip(GetText("ObtainedByBreaking"), tileSources, Config.BreakableTiles.MaxCount));

            // Crafting
            if (Config.Crafting.Enabled && CraftingSources.TryGetValue(item.type, out var ingredients))
                TryAddMethod(Config.Crafting.MaxCount == 0 ? GetText("Craftable") : GenerateItemSourceTooltip(GetText("CraftedWith"), ingredients, Config.Crafting.MaxCount));

            // Grab Bags
            if (Config.GrabBags.Enabled && GrabBagSources.TryGetValue(item.type, out var grabBagSources))
                TryAddMethod(Config.GrabBags.MaxCount == 0 ? GetText("Grabbed") : GenerateItemSourceTooltip(GetText("GrabbedFrom"), grabBagSources, Config.GrabBags.MaxCount));

            // Chests
            if (Config.Chests.Enabled && ChestSources.TryGetValue(item.type, out var chestSources))
                TryAddMethod(Config.Chests.MaxCount == 0 ? GetText("FoundInChests") : GenerateItemSourceTooltip(GetText("FoundIn"), chestSources, Config.Chests.MaxCount));

            // Shops
            if (Config.Shops.Enabled && ShopSources.TryGetValue(item.type, out var shopSources))
                TryAddMethod(Config.Shops.MaxCount == 0 ? GetText("Purchasable") : GenerateShopSourceTooltip(GetText("SoldBy"), shopSources, Config.Shops.MaxCount));

            // Drops
            if (Config.Drops.Enabled && DropSources.TryGetValue(item.type, out var npcSources))
                TryAddMethod(Config.Drops.MaxCount == 0 ? GetText("Droppable") : GenerateNpcSourceTooltip(GetText("DroppedBy"), npcSources, Config.Drops.MaxCount));

            // Catching
            if (Config.Catching.Enabled && CatchNPCSources.TryGetValue(item.type, out var catchNpcSources))
                TryAddMethod(Config.Catching.MaxCount == 0 ? GetText("Catchable") : GenerateNpcSourceTooltip(GetText("CatchedFrom"), catchNpcSources, Config.Catching.MaxCount));
            
            if (Config.Banners.Enabled && NPCBannerSources.TryGetValue(item.type, out var bannerSources)) {
                if (Config.Banners.MaxCount == 0) TryAddMethod(GetText("ObtainableAsBanner"));
                else {
                    var str = GenerateNpcSourceTooltip("{0}", bannerSources, Config.Banners.MaxCount);
                    TryAddMethod(string.Format(GetText("ObtainedAfterKilling"), bannerSources[0].num.ToString(), str));
                }
            }

            // Shimmering
            if (Config.Shimmering.Enabled && ShimmerSources.TryGetValue(item.type, out var shimmerSources))
                TryAddMethod(Config.Shimmering.MaxCount == 0 ? GetText("Shimmered") : GenerateItemSourceTooltip(GetText("ShimmeredFrom"), shimmerSources, Config.Shimmering.MaxCount));

            // Decrafting
            if (Config.Decrafting.Enabled && DecraftSources.TryGetValue(item.type, out var decraftSources))
                TryAddMethod(Config.Decrafting.MaxCount == 0 ? GetText("Decrafted") : GenerateItemSourceTooltip(GetText("DecraftedFrom"), decraftSources, Config.Decrafting.MaxCount));

            // Extractinator
            if (Config.Extractinator.Enabled && ExtractinatorSources.TryGetValue(item.type, out var extractSources))
                TryAddMethod(Config.Extractinator.MaxCount == 0 ? GetText("FromExtractinator") : GenerateItemSourceTooltip(GetText("ExtractedFrom"), extractSources, Config.Extractinator.MaxCount));

            // Chlorophyte Extractinator
            if (Config.ChlorophyteExtractinator.Enabled && ChlorophyteExtractinatorSources.TryGetValue(item.type, out var chloroExtractSources))
                TryAddMethod(Config.ChlorophyteExtractinator.MaxCount == 0 ? GetText("FromChlorophyteExtractinator") : GenerateItemSourceTooltip(GetText("ChlorophyteExtractedFrom"), chloroExtractSources, Config.ChlorophyteExtractinator.MaxCount));


            // Fishing
            if (Config.Fishing.Enabled && FishingSources.TryGetValue(item.type, out var fishingSources)) {
                if (Config.Fishing.MaxCount == 0) {
                    obtainingMethods.Add(GetText("FishingCatch"));
                } else {
                    var fishingInfo = fishingSources[0];
                    var rarityId = (FishingRarity)fishingInfo.id;
                    string environmentText = fishingInfo.str;
                    string rarityKey = Enum.GetName(typeof(FishingRarity), rarityId);
                    string rarityText = (rarityId == FishingRarity.None) ? "" : Language.GetTextValue($"Mods.MoreObtainingTooltips.Tooltips.Fishing.Rarities.{rarityKey}");
                    if (fishingInfo.num > 0) {
                        environmentText += Language.GetTextValue($"Mods.MoreObtainingTooltips.Tooltips.Fishing.Suffix.{Enum.GetName(typeof(FishingSuffix), fishingInfo.num)}");
                    }
                    obtainingMethods.Add(string.Format(GetText("FishingCatchDetails"), rarityText, environmentText));
                }
            }

            // Customized
            if (Config.Customized.Enabled && CustomizedSources.TryGetValue(item.type, out List<SourceInfo> keys)) {
                for (int i = 0; i < keys.Count; i++) {
                    obtainingMethods.Add($"{keys[i].str}");
                }
            }

            // --- Add all generated tooltips to the item ---
            if (!obtainingMethods.Any()) {
                return;
            }

            int insertIndex = -1;

            string[] anchorNames = {
                "Master", "Expert", "SetBonus", "OneDropLogo",
                "BuffTime", "WellFedExpert", "EtherianManaWarning",
                "Material", "Consumable", "Ammo", "Placeable",
                "WandConsumes", "Equipable", "Vanity", "Quest", "Defense",
                "HealLife", "HealMana", "UseMana",
                "TileBoost", "HammerPower", "AxePower", "PickPower",
                "BaitPower", "NeedsBait", "FishingPower",
                "Knockback", "SpecialSpeedScaling", "NoSpeedScaling", "Speed", "CritChance", "Damage",
                "SocialDesc", "Social", "NoTransfer", "FavoriteDesc", "Favorite",
                "ItemName"
            };

            foreach (var name in anchorNames) {
                int foundIndex = tooltips.FindIndex(line => line.Name == name);
                if (foundIndex != -1) {
                    insertIndex = foundIndex + 1;
                    break;
                }
            }

            int lastPrefixIndex = tooltips.FindLastIndex(line => line.Name.StartsWith("Prefix"));
            insertIndex = (int)MathHelper.Max(lastPrefixIndex + 1, insertIndex);

            int lastTooltipIndex = tooltips.FindLastIndex(line => line.Name.StartsWith("Tooltip"));
            insertIndex = (int)MathHelper.Max(lastTooltipIndex + 1, insertIndex);

            if (insertIndex == -1) {
                insertIndex = tooltips.Count;
            }

            bool showDetails = Config.ShowMode == TooltipShowMode.Always || Main.keyState.IsKeyDown(Keys.LeftShift);

            if (showDetails) {
                for (int i = 0; i < obtainingMethods.Count; i++) {
                    var line = new TooltipLine(Mod, $"ObtainingInfo{i}", obtainingMethods[i]) {
                        OverrideColor = Config.TooltipColor
                    };
                    if(i >= Config.AutomaticallyFoldRows && obtainingMethods.Count - i > 1 && !Main.keyState.IsKeyDown(Keys.LeftShift)) {
                        var hintLine = new TooltipLine(Mod, "ObtainingHint", Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.PressShiftHintNum", obtainingMethods.Count - i)) {
                            OverrideColor = Color.Lerp(Config.TooltipColor, Color.White, 0.5f)
                        };
                        tooltips.Insert(insertIndex + i, hintLine);
                        break;
                    } else {
                        tooltips.Insert(insertIndex + i, line);
                    }
                }
            } else {
                var hintLine = new TooltipLine(Mod, "ObtainingHint", Language.GetTextValue("Mods.MoreObtainingTooltips.Tooltips.PressShiftHint")) {
                    OverrideColor = Color.Lerp(Config.TooltipColor, Color.White, 0.5f)
                };
                tooltips.Insert(insertIndex, hintLine);
            }
        }
    }
}