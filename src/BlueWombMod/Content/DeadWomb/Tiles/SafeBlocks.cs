using Microsoft.Xna.Framework;
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
        Main.tileBrick[Type] = true;
        Main.tileMergeDirt[Type] = true;

        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        HitSound = null;
        DustType = ModContent.DustType<DeadTissueDust>();
        AddMapEntry(new Color(63, 81, 114));
    }
}

public sealed class DeadTissueWallItem : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<DeadTissueWallUnsafeItem>();
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<DeadTissueWall>());
    }

    public override void AddRecipes()
    {
        CreateRecipe(4)
            .AddIngredient<DeadTissueBlockUnsafeItem>()
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public sealed class DeadTissueWall : ModWall
{
    public override void SetStaticDefaults()
    {
        HitSound = null;
        DustType = ModContent.DustType<DeadTissueDust>();
        AddMapEntry(new Color(51, 66, 94));
    }
}