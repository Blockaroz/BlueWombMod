using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.Tiles;

public sealed class DeadTissueBlockItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<DeadTissueBlock>());
    }
}

public sealed class DeadTissueBlock : ModTile
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.DoesntGetReplacedWithTileReplacement[Type] = true;

        Main.tileBrick[Type] = true;
        Main.tileMergeDirt[Type] = true;

        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        MineResist = 3f;

    }

    public override bool CanDrop(int i, int j)
    {
        return false;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
    }
}