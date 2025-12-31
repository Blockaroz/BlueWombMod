using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.HushBoss.Projectiles;

public sealed class HolySkyCrack : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;

        Projectile.penetrate = -1;
        Projectile.tileCollide = false;

        Projectile.hostile = true;
        Projectile.friendly = false;

        Projectile.hide = true;
    }

    public ref float Radius => ref Projectile.ai[0];

    public bool Bursting { get => Projectile.ai[1] == 1; set => Projectile.ai[1] = value ? 1 : 0; }

    public override void AI()
    {

    }

    public override bool PreKill(int timeLeft)
    {
        if (!Bursting)
        {
            Bursting = true;
            Projectile.timeLeft = 30;
            return false;
        }

        return Bursting;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (Bursting && Projectile.timeLeft > 10)
        {
            return false;
        }

        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}
