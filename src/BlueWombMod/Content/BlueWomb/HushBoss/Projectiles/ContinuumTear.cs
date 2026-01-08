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

public sealed class ContinuumTear : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 14;

        Projectile.friendly = false;
        Projectile.hostile = true;

        Projectile.penetrate = 1;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 300;
    }

    public ref float Time => ref Projectile.ai[0];

    public ref float Curvature => ref Projectile.ai[1];

    public ref float MiscTime => ref Projectile.localAI[0];

    public override void OnSpawn(IEntitySource source)
    {
        MiscTime = Main.rand.Next(30);
        Projectile.scale *= Main.rand.NextFloat(0.8f, 1.2f);
    }

    public override void AI()
    {
        MiscTime++;
        Time++;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.tileCollide = false;
        Projectile.velocity = oldVelocity;

        return false;
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}
