using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

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
}