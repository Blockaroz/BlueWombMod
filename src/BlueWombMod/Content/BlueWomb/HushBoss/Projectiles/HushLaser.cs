using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss.Projectiles;

public sealed class HushLaser : ModProjectile
{
    public override void SetStaticDefaults()
    {
        
    }

    public override void SetDefaults()
    {
        Projectile.width = 48;
        Projectile.height = 48;

        Projectile.damage = 60;
    }

    public ref float HostIndex => ref Projectile.ai[0];

    public Hush Hush
    {
        get
        {
            if (HostIndex >= 0 && HostIndex < Main.npc.Length)
            {
                NPC npc = Main.npc[(int)HostIndex];
                if (npc.ModNPC is Hush hush)
                    return hush;
            }

            return null;
        }
    }

    public ref float Time => ref Projectile.ai[1];

    public ref float State => ref Projectile.ai[2];

    public override void OnSpawn(IEntitySource source)
    {
        HostIndex = -1;
    }

    public override void AI()
    {
        if (Hush == null)
        {
            return;
        }

        var target = Hush.NPC.GetTargetData();

        if (!target.Invalid)
        {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(target.Center), 0.02f).SafeNormalize(Vector2.Zero) * 8f;
        }
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
