using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace MoreObtainingTooltips
{
    public enum TooltipShowMode {
        Never,
        OnShift,
        Always
    }

    public enum ItemShowMode {
        Icon,
        Name,
        IconAndName
    }
    public class TooltipSettings {
        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        [DefaultValue(5)]
        [Range(0, 15)]
        [Slider]
        public int MaxCount { get; set; } = 5;
    }

    public class TooltipConfig : ModConfig {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(TooltipShowMode.Always)]
        public TooltipShowMode ShowMode { get; set; }

        [DefaultValue(ItemShowMode.Icon)]
        public ItemShowMode ItemShowMode { get; set; }

        [DefaultValue(typeof(Color), "180, 180, 180, 255")]
        public Color TooltipColor { get; set; }

        public TooltipSettings Crafting { get; set; } = new();
        public TooltipSettings Shops { get; set; } = new();
        public TooltipSettings Drops { get; set; } = new();
        public TooltipSettings Catching { get; set; } = new();
        public TooltipSettings Shimmering { get; set; } = new();
        public TooltipSettings Decrafting { get; set; } = new();
        public TooltipSettings GrabBags { get; set; } = new();
        public TooltipSettings Chests { get; set; } = new();
        public TooltipSettings Fishing { get; set; } = new();
        public TooltipSettings Extractinator { get; set; } = new();
        public TooltipSettings ChlorophyteExtractinator { get; set; } = new();
        public TooltipSettings Customized { get; set; } = new();
    }
}
