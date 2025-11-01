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
    }


    public class FindUnobtainableItemsCommand : ModCommand {
        public override string Command => "findunobtainable";

        public override CommandType Type => CommandType.Chat;

        public override string Description => "Find all unobtainable items";

        public override void Action(CommandCaller caller, string input, string[] args) {
            var noSourceItems = new List<string>();

            for (int itemType = ItemID.None + 1; itemType < ItemLoader.ItemCount; itemType++) {
                var item = ContentSamples.ItemsByType[itemType];
                if (item.IsAir || item.type != itemType || (item.createTile <= 246 && item.createTile >= 240)) continue;

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
                    noSourceItems.Add(Lang.GetItemNameValue(itemType));
                }
            }

            if (noSourceItems.Count > 0) {
                Main.NewText($"[c/FF0000:Found {noSourceItems.Count} unobtainable items:]");
                for (int i = 0; i < noSourceItems.Count; i += 50) {
                    var currentChunk = noSourceItems.Skip(i).Take(50);
                    Main.NewText(string.Join(", ", currentChunk));
                }
            }
        }
    }
}