using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.Tiles;

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
            .AddIngredient<DeadTissueBlockItem>()
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}

public sealed class DeadTissueWallUnsafeItem : ModItem
{
    public override string Texture => ModContent.GetInstance<DeadTissueWallItem>().Texture;

    public override void SetStaticDefaults()
    {
        ItemID.Sets.DrawUnsafeIndicator[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<DeadTissueWallUnsafe>());
    }

    public override void AddRecipes()
    {
		CreateRecipe()
	        .AddIngredient<DeadTissueBlockItem>()
	        .AddCondition(Terraria.Condition.InGraveyard)
	        .Register();
	}
}

public sealed class DeadTissueWall : ModWall
{
    public override void SetStaticDefaults()
    {
        DustType = ModContent.DustType<DeadTissueDust>();
    }
}

public sealed class DeadTissueWallUnsafe : ModWall
{
	public override string Texture => ModContent.GetInstance<DeadTissueWall>().Texture;

    public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;
		Main.wallBlend[Type] = ModContent.WallType<DeadTissueWallUnsafe>();
	}

    public override bool Drop(int i, int j, ref int type)
    {
        return false;
    }
}
