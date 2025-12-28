using BlueWombMod.Content.DeadWomb.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb;

public sealed class DeadWombBiome : ModBiome
{
    public override Color? BackgroundColor => Color.Black;

	public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

	public override bool IsBiomeActive(Player player)
    {
        int x = Math.Clamp((int)(player.Center.X / 16), 2, Main.maxTilesX - 2);
        int y = Math.Clamp((int)(player.Center.Y / 16), 2, Main.maxTilesY - 2);

        bool behindWall = Main.tile[x, y].WallType == ModContent.WallType<DeadTissueWallUnsafe>();

        return behindWall;
    }

    public override float GetWeight(Player player)
    {
        return 1f;
    }
}
