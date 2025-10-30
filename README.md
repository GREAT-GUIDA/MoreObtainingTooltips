More Obtaining Tooltips

Shows how to obtain an item in its tooltip.

[b]Currently Supports:[/b]
[list]
[*][b]Crafting:[/b] Displays the materials required to craft the item.
[*][b]Sold by NPCs:[/b] Displays the NPCs that sell the item.
[*][b]Dropped by NPCs:[/b] Displays the NPCs that drop the item.
[*][b]Critter Catching:[/b] Displays the critters that can be caught to obtain the item.
[*][b]Shimmer Transmutation:[/b] Displays the item that can be transmuted into this one.
[*][b]Shimmer Decrafting:[/b] Displays the item that can be decrafted to obtain this one.
[*][b]Grab Bags:[/b] Displays the grab bags that can contain the item.
[*][b]Chests & Crates:[/b] Displays the chests where the item can be found.
[*][b]Fishing:[/b] Displays the required biome and rarity to fish up the item (Currently Vanilla Only).
[*][b]Extractinator:[/b] Displays the blocks that can be extracted to obtain the item.
[*][b]Chlorophyte Extractinator:[/b] Displays the blocks that can be extracted to obtain the item.
[*][b]Special Obtaining:[/b] Angler quests, Dye Trader trades, Party Girl gifts, shaking tree, breaking pots (Currently Vanilla Only).
[/list]
The display of all information is configurable.
Unless specified as Vanilla Only, all information supports mods.

[b]Future Plans:[/b]

[*][b]Special Mod Support[/b]
[*][b]Tile Breaking Drops[/b]
[*]And other various obtaining methods

Discord: https://discord.gg/eVR94prTmq

Open source

Support ModCall to add source, example here:

public override void PostSetupContent() {
    if (ModLoader.TryGetMod("MoreObtainingTooltips", out Mod moreObtainingTooltips)) {
        moreObtainingTooltips.Call(
            "AddCustomizedSource",
            Language.GetTextValue($"Mods.MyMod.Tooltips.SomeKey"),
            new int[] {
                ModContent.ItemType<Items.CoolSword>(),
                ModContent.ItemType<Items.ShinyArmor>()
            };
        );
    }
}