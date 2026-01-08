using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.Tiles;

public sealed class TileRegenPreservationSystem : ModSystem
{
    public override void Load()
    {
        On_WorldGen.SaveAndQuitCallBack += PreserveRegeneration;
    }

    private void PreserveRegeneration(On_WorldGen.orig_SaveAndQuitCallBack orig, object threadContext)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                if (projectile.type == ModContent.ProjectileType<DeadTissueBlockGrowth>() || projectile.type == ModContent.ProjectileType<DeadTissueWallGrowth>())
                {
                    // Kill growths so they instantly place their tile
                    projectile.Kill();
                }
            }
        }

        orig(threadContext);
    }
}