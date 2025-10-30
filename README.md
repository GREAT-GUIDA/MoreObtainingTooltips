# More Obtaining Tooltips

Shows how to obtain an item in its tooltip.

**Currently Supports:**

  * **Crafting:** Displays the materials required to craft the item.
  * **Sold by NPCs:** Displays the NPCs that sell the item.
  * **Dropped by NPCs:** Displays the NPCs that drop the item.
  * **Critter Catching:** Displays the critters that can be caught to obtain the item.
  * **Shimmer Transmutation:** Displays the item that can be transmuted into this one.
  * **Shimmer Decrafting:** Displays the item that can be decrafted to obtain this one.
  * **Grab Bags:** Displays the grab bags that can contain the item.
  * **Chests & Crates:** Displays the chests where the item can be found.
  * **Fishing:** Displays the required biome and rarity to fish up the item (Currently Vanilla Only).
  * **Extractinator:** Displays the blocks that can be extracted to obtain the item.
  * **Chlorophyte Extractinator:** Displays the blocks that can be extracted to obtain the item.
  * **Special Obtaining:** Angler quests, Dye Trader trades, Party Girl gifts, shaking tree, breaking pots (Currently Vanilla Only).

The display of all information is configurable.
Unless specified as Vanilla Only, all information supports mods.

**Future Plans:**

  * **Special Mod Support**
  * **Tile Breaking Drops**
  * And other various obtaining methods

Discord: [https://discord.gg/eVR94prTmq](https://discord.gg/eVR94prTmq)

Open source

**Support ModCall to add source, example here:**

```csharp
public override void PostSetupContent() {
    if (ModLoader.TryGetMod("MoreObtainingTooltips", out Mod moreObtainingTooltips)) {
        moreObtainingTooltips.Call(
            "AddCustomizedSource",
            Language.GetTextValue($"Mods.MyMod.Tooltips.SomeKey"),
            new int[] {
                ModContent.ItemType<Items.CoolSword>(),
                ModContent.ItemType<Items.ShinyArmor>()
            }
        );
    }
}
```