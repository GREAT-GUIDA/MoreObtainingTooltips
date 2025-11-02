using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using static MoreObtainingTooltips.ObtainingSystem;

namespace MoreObtainingTooltips {
    public class MoreObtainingTooltips : Mod {
        public override object Call(params object[] args) {
            if (args.Length == 3 && args[0] is string command && command == "AddCustomizedSource") {
                if (args[1] is string fullLocalizationKey && args[2] is IEnumerable<int> itemIDs) {
                    ObtainingSystem.RegisterCustomSource(fullLocalizationKey, itemIDs);
                    return "Success";
                }
            }
            return "Error: Invalid arguments.";
        }
        public static bool TryGetModItemId(string modName, string itemName, out int itemId) {
            itemId = 0;
            if (ModLoader.TryGetMod(modName, out Mod mod)) {
                if (mod.TryFind<ModItem>(itemName, out ModItem foundModItem)) {
                    itemId = foundModItem.Type;
                    return true;
                }
            }
            return false;
        }

        public static int GetModItemId(string modName, string itemName) {
            if (TryGetModItemId(modName, itemName, out int itemId)) {
                return itemId;
            }
            return ItemID.None; // µÈÍ¬ÓÚ 0
        }
    }


    public class FindUnobtainableItemsCommand : ModCommand {
        public override string Command => "findunobtainable";

        public override CommandType Type => CommandType.Chat;

        public override string Description => "Find all unobtainable items";
        public override void Action(CommandCaller caller, string input, string[] args) {
            // Use a dictionary to group item names by their mod name.
            var noSourceItemsByMod = new Dictionary<string, List<string>>();

            for (int itemType = ItemID.None + 1; itemType < ItemLoader.ItemCount; itemType++) {
                var item = ContentSamples.ItemsByType[itemType];
                if (item.IsAir || item.type != itemType/* || (item.createTile <= 246 && item.createTile >= 240)*/) continue;

                bool hasSource =
                    BreakTileSources.ContainsKey(itemType) ||
                    CraftingSources.ContainsKey(itemType) ||
                    ShimmerSources.ContainsKey(itemType) ||
                    DecraftSources.ContainsKey(itemType) ||
                    GrabBagSources.ContainsKey(itemType) ||
                    ChestSources.ContainsKey(itemType) ||
                    ExtractinatorSources.ContainsKey(itemType) ||
                    ChlorophyteExtractinatorSources.ContainsKey(itemType) ||
                    ShopSources.ContainsKey(itemType) ||
                    DropSources.ContainsKey(itemType) ||
                    CatchNPCSources.ContainsKey(itemType) ||
                    NPCBannerSources.ContainsKey(itemType) ||
                    FishingSources.ContainsKey(itemType) ||
                    CustomizedSources.ContainsKey(itemType);

                if (!hasSource) {
                    // Determine the mod name. Use "Terraria" for vanilla items.\
                    string modName = item.ModItem?.Mod?.Name ?? "Terraria";

                    // If this is the first item from this mod, create a new list for it.
                    if (!noSourceItemsByMod.ContainsKey(modName)) {
                        noSourceItemsByMod[modName] = new List<string>();
                    }

                    // Add the item name to the correct mod's list.
                    noSourceItemsByMod[modName].Add(Lang.GetItemNameValue(itemType));
                }
            }

            // Now, print the grouped results.
            if (noSourceItemsByMod.Count > 0) {
                int totalCount = noSourceItemsByMod.Sum(pair => pair.Value.Count);
                Main.NewText($"[c/FF0000:Found {totalCount} unobtainable items across {noSourceItemsByMod.Count} sources:]");

                // Iterate through each mod in the dictionary.
                foreach (var pair in noSourceItemsByMod) {
                    string modName = pair.Key;
                    var itemsInMod = pair.Value;

                    // Print a header for the current mod.
                    Main.NewText($"[c/FFFF00:--- {modName} ({itemsInMod.Count}) ---]");

                    // Print the items for this mod in chunks to avoid overly long lines.
                    const int itemsPerLine = 15;
                    for (int i = 0; i < itemsInMod.Count; i += itemsPerLine) {
                        var currentChunk = itemsInMod.Skip(i).Take(itemsPerLine);
                        Main.NewText(string.Join(", ", currentChunk));
                    }
                }
            } else {
                Main.NewText("[c/00FF00:No unobtainable items found.]");
            }
        }
    }
}