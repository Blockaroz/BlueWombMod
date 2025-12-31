using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BlueWombMod.Content.DeadWomb.Tiles;

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
