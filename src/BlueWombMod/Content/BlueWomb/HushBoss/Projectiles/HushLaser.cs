using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
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

        Projectile.friendly = false;
        Projectile.hostile = true;

        Projectile.penetrate = -1;
        Projectile.tileCollide = false;

        Projectile.damage = 60;
        Projectile.timeLeft = 60;
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

    public ref float Side => ref Projectile.ai[1];

    public ref float Time => ref Projectile.ai[2];

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

        var offset = Projectile.DirectionTo(Hush.TargetPosition).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2 * Side) * 122f;
        Projectile.velocity += Projectile.DirectionTo(Hush.TargetPosition + offset).SafeNormalize(Vector2.Zero) * 3f;
        Projectile.velocity *= 0.7f;

        for (int i = 0; i < 13; i++)
        {
            Dust light = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2CircularEdge(40, 40), DustID.AncientLight, Projectile.velocity * 0.1f, Scale: Main.rand.NextFloat(0.5f, 2f));
            light.noGravity = true;
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        return false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Hush == null)
            return false;

        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle frame = texture.Frame();
        var origin = new Vector2(frame.Width / 2f, frame.Height / 1.5f);
        
        // Draw over hush eye

        if ((Side < 0 && Hush.Renderer.EyeStateLeft == HushRenderer.EyeAnimationState.Glowing) || 
            (Side > 0 && Hush.Renderer.EyeStateRight == HushRenderer.EyeAnimationState.Glowing))
        {
            Vector2 eyeOff = new Vector2(72 * Side, -20).RotatedBy(Hush.Renderer.Face.Rotation + Hush.NPC.rotation) * Hush.Renderer.DrawScale;
            Vector2 eyePosition = Hush.NPC.Center + Hush.Renderer.DrawOffset + eyeOff;
            float wobble = MathF.Abs(MathF.Sin(Main.GlobalTimeWrappedHourly * 12)) * 0.1f;
            Main.EntitySpriteDraw(texture, eyePosition - Main.screenPosition, frame, Color.White, 0f, origin, Projectile.scale * new Vector2(0.65f + wobble * 0.5f, 0.65f - wobble), 0, 0);
        }

        // Draw actual laser

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, 0f, origin, Projectile.scale * 1.1f, 0, 0);

        return false;
    }
}
