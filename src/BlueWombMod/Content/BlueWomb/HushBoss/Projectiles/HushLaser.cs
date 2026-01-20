using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss.Projectiles;

public sealed class HushLaser : ModProjectile
{
    public override void SetStaticDefaults()
    {
        
    }

    public override void SetDefaults()
    {
        
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        return false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        // Draw over hush eye

        // Draw actual laser

        return false;
    }
}
