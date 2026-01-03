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

[Autoload(Side = ModSide.Client)]
public static class BlueWombBiomeExtension
{
    extension(Player player)
    {
        public bool ZoneBlueWomb => player.InModBiome<BlueWombBiome>();
    }
}

public sealed class BlueWombDarknessSystem : ModSystem
{
    public override void Load()
    {
        On_TileLightScanner.GetTileLight += ApplyBlueWomblight;
    }

    private void ApplyBlueWomblight(On_TileLightScanner.orig_GetTileLight orig, TileLightScanner self, int x, int y, out Vector3 outputColor)
    {
        orig(self, x, y, out outputColor);

        if (!HushSystem.WombInWorld)
        {
            return;
        }

        if (!WorldGen.SolidOrSlopedTile(x, y))
        {
            float i = x - HushSystem.WombPosition.X;
            float j = y - HushSystem.WombPosition.Y;
            float distance = MathF.Sqrt(i * i + j * j);
            outputColor += Vector3.One * Utils.GetLerpValue(HushSystem.WOMB_RADIUS, 0, distance, true) * MathF.Sqrt(lightFade);
        }
    }

    private static float lightFade;

    public override void ModifyLightingBrightness(ref float scale)
    {
        if (Main.LocalPlayer.ZoneBlueWomb)
        {
            lightFade += 0.025f;
        }
        else
        {
            lightFade -= 0.075f;
        }

        lightFade = Math.Clamp(lightFade, 0f, 1f);

        if (lightFade > 0f)
        {
            scale *= 1f - lightFade * 0.133f * (1f + MathF.Sin(Main.GlobalTimeWrappedHourly) * 0.2f);
        }
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (lightFade > 0f)
        {
            tileColor = tileColor.MultiplyRGBA(Color.Lerp(Color.White, new Color(30, 33, 40), lightFade * 0.5f));
            backgroundColor = backgroundColor.MultiplyRGBA(Color.Lerp(Color.White, new Color(30, 33, 40), lightFade));
        }
    }
}