using BlueWombMod.Content.DeadWomb.HushBoss;
using BlueWombMod.Content.DeadWomb.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb;

public sealed class BlueWombBiome : ModBiome
{
    public override Color? BackgroundColor => Color.Black;

	public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

    public override int Music => MusicID.OtherworldlyUGCrimson;

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

public static class BlueWombBiomeExtension
{
    extension(Player player)
    {
        public bool ZoneBlueWomb => player.InModBiome<BlueWombBiome>();
    }
}

public sealed class BlueWombDarknessSystem : ModSystem
{
    private static float lightFade;

    public override void ModifyLightingBrightness(ref float scale)
    {
        if (Main.LocalPlayer.ZoneBlueWomb)
        {
            lightFade += 0.02f;
        }
        else
        {
            lightFade -= 0.05f;
        }
        lightFade = Math.Clamp(lightFade, 0f, 1f);

        scale *= 1f - lightFade * 0.2f;

    }
}