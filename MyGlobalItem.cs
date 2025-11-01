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

namespace MoreObtainingTooltips {
    public class MyGlobalItem : GlobalItem {
        // Cached instance of the config.
        private static TooltipConfig _config;
        private static TooltipConfig Config => _config ??= ModContent.GetInstance<TooltipConfig>();

        private string GenerateItemSourceTooltip(string format, List<int> sources, int maxCount) {
            var distinctSources = sources.Distinct().ToList();
            if (!distinctSources.Any()) return null;

            var itemIcons = new StringBuilder();
            int countToShow = System.Math.Min(distinctSources.Count, maxCount);

            for (int i = 0; i < countToShow; i++) {
                if (Config.ItemShowMode == ItemShowMode.Icon)itemIcons.Append($"[i:{distinctSources[i]}]");
                if (Config.ItemShowMode == ItemShowMode.Name) { 
                    if (i != 0) itemIcons.Append(", "); 
                    itemIcons.Append(Lang.GetItemNameValue(distinctSources[i])); 
                }
                if (Config.ItemShowMode == ItemShowMode.IconAndName) {
                    itemIcons.Append($"[i:{distinctSources[i]}]");
                    itemIcons.Append($"({Lang.GetItemNameValue(distinctSources[i])})");
                }
            }

            var tooltipText = new StringBuilder(string.Format(format, itemIcons));

            if (distinctSources.Count > countToShow) {
                tooltipText.Append($"… ({(distinctSources.Count - countToShow)} more)");
            }

            return tooltipText.ToString();
        }

        private string GenerateNpcSourceTooltip(string format, List<int> npcIds, int maxCount) {
            var uniqueNames = npcIds.Select(Lang.GetNPCNameValue).Distinct().ToList();
            if (!uniqueNames.Any()) return null;

            string nameList = string.Join(", ", uniqueNames.Take(maxCount));
            var tooltipText = new StringBuilder(string.Format(format, nameList));

            if (uniqueNames.Count > maxCount) {
                tooltipText.Append($"… ({(uniqueNames.Count - maxCount)} more)");
            }

            return tooltipText.ToString();
        }

        private string GenerateShopSourceTooltip(string format, List<ShopSourceInfo> sources, int maxCount) {
            var distinctSources = sources.Distinct().ToList();
            if (!distinctSources.Any()) return null;

            var tooltipParts = new List<string>();
            int countToShow = System.Math.Min(distinctSources.Count, maxCount);

            for (int i = 0; i < countToShow; i++) {
                var source = distinctSources[i];
                string npcName = Lang.GetNPCNameValue(source.NpcId);
                if (source.NpcId == -1) npcName = GetText("AnyNPC");
                if (source.Conditions.Any() && Config.ShowShopCondition) {
                    string conditionText = string.Join(", ", source.Conditions);
                    tooltipParts.Add($"{npcName}({conditionText})");
                } else {
                    tooltipParts.Add(npcName);
                }
            }

            string nameList = string.Join(", ", tooltipParts);
            var tooltipText = new StringBuilder(string.Format(format, nameList));

            if (distinctSources.Count > countToShow) {
                tooltipText.Append($"… ({(distinctSources.Count - countToShow)} more)");
            }

            return tooltipText.ToString();
        }

        private static string GetText(string key) => Language.GetTextValue($"Mods.MoreObtainingTooltips.Tooltips.{key}");

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
            if (item.type <= ItemID.None || Config == null || Config.ShowMode == TooltipShowMode.Never) {
                return;
            }

            var obtainingMethods = new List<string>();
            //Main.NewText(NPCShopDatabase.AllShops.ToArray()[24].FullName);

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

            // Crafting
            if (Config.Crafting.Enabled && CraftingSources.TryGetValue(item.type, out var ingredients))
                TryAddMethod(Config.Crafting.MaxCount == 0 ? GetText("Craftable") : GenerateItemSourceTooltip(GetText("CraftedWith"), ingredients, Config.Crafting.MaxCount));

            // Shimmering
            if (Config.Shimmering.Enabled && ShimmerSources.TryGetValue(item.type, out var shimmerSources))
                TryAddMethod(Config.Shimmering.MaxCount == 0 ? GetText("Shimmered") : GenerateItemSourceTooltip(GetText("ShimmeredFrom"), shimmerSources, Config.Shimmering.MaxCount));

            // Decrafting
            if (Config.Decrafting.Enabled && DecraftSources.TryGetValue(item.type, out var decraftSources))
                TryAddMethod(Config.Decrafting.MaxCount == 0 ? GetText("Decrafted") : GenerateItemSourceTooltip(GetText("DecraftedFrom"), decraftSources, Config.Decrafting.MaxCount));

            // Grab Bags
            if (Config.GrabBags.Enabled && GrabBagSources.TryGetValue(item.type, out var grabBagSources))
                TryAddMethod(Config.GrabBags.MaxCount == 0 ? GetText("Grabbed") : GenerateItemSourceTooltip(GetText("GrabbedFrom"), grabBagSources, Config.GrabBags.MaxCount));

            // Chests
            if (Config.Chests.Enabled && ChestSources.TryGetValue(item.type, out var chestSources))
                TryAddMethod(Config.Chests.MaxCount == 0 ? GetText("FoundInChests") : GenerateItemSourceTooltip(GetText("FoundIn"), chestSources, Config.Chests.MaxCount));

            // Extractinator
            if (Config.Extractinator.Enabled && ExtractinatorSources.TryGetValue(item.type, out var extractSources))
                TryAddMethod(Config.Extractinator.MaxCount == 0 ? GetText("FromExtractinator") : GenerateItemSourceTooltip(GetText("ExtractedFrom"), extractSources, Config.Extractinator.MaxCount));

            // Chlorophyte Extractinator
            if (Config.ChlorophyteExtractinator.Enabled && ChlorophyteExtractinatorSources.TryGetValue(item.type, out var chloroExtractSources))
                TryAddMethod(Config.ChlorophyteExtractinator.MaxCount == 0 ? GetText("FromChlorophyteExtractinator") : GenerateItemSourceTooltip(GetText("ChlorophyteExtractedFrom"), chloroExtractSources, Config.ChlorophyteExtractinator.MaxCount));

            // Shops
            if (Config.Shops.Enabled && ShopSources.TryGetValue(item.type, out var shopSources))
                TryAddMethod(Config.Shops.MaxCount == 0 ? GetText("Purchasable") : GenerateShopSourceTooltip(GetText("SoldBy"), shopSources, Config.Shops.MaxCount));

            // Drops
            if (Config.Drops.Enabled && DropSources.TryGetValue(item.type, out var npcSources))
                TryAddMethod(Config.Drops.MaxCount == 0 ? GetText("Droppable") : GenerateNpcSourceTooltip(GetText("DroppedBy"), npcSources, Config.Drops.MaxCount));

            // Catching
            if (Config.Catching.Enabled && CatchNPCSources.TryGetValue(item.type, out var catchNpcSources))
                TryAddMethod(Config.Catching.MaxCount == 0 ? GetText("Catchable") : GenerateNpcSourceTooltip(GetText("CatchedFrom"), catchNpcSources, Config.Catching.MaxCount));
            
            // Fishing
            if (Config.Fishing.Enabled && FishingSources.TryGetValue(item.type, out FishingInfo fishingInfo)) {
                if (Config.Fishing.MaxCount == 0) {
                    obtainingMethods.Add(GetText("FishingCatch"));
                } else {
                    string rarityText = Language.GetTextValue($"Mods.MoreObtainingTooltips.Tooltips.Fishing.Rarities.{fishingInfo.RarityKey}");

                    string separator = GetText("Fishing.ListSeparator");

                    List<string> localizedEnvs = fishingInfo.EnvironmentKeys
                        .Select(key => Language.GetTextValue($"Mods.MoreObtainingTooltips.Tooltips.Fishing.Environments.{key}"))
                        .ToList();
                    string environmentText = string.Join(separator, localizedEnvs);

                    if (fishingInfo.IsHardmode) {
                        environmentText += GetText("Fishing.HardmodeSuffix");
                    }

                    obtainingMethods.Add(string.Format(GetText("FishingCatchDetails"), rarityText, environmentText));
                }
            }

            // Customized
            if (Config.Customized.Enabled && CustomizedSources.TryGetValue(item.type, out List<string> keys)) {
                for (int i = 0; i < keys.Count; i++) {
                    obtainingMethods.Add($"{keys[i]}");
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
                    tooltips.Insert(insertIndex + i, line);
                }
            } else {
                var hintLine = new TooltipLine(Mod, "ObtainingHint", "<Press Shift for obtaining info>") {
                    OverrideColor = Config.TooltipColor
                };
                tooltips.Insert(insertIndex, hintLine);
            }
        }
    }
}