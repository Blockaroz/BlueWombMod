using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.HushBoss.Drops;

public sealed class SurgicalKnife : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToAccessory(newwidth: 30, newheight: 28);
        Item.rare = ItemRarityID.Green;
    }
}
