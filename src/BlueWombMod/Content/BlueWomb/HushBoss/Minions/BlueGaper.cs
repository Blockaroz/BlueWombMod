using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss.Minions;

public sealed class BlueGaper : ModNPC
{
    public override void SetDefaults()
    {
        NPC.width = 28;
        NPC.height = 28;

        NPC.lifeMax = 110;
        NPC.noGravity = true;
        NPC.knockBackResist = 0.5f;

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;

        NPC.damage = 50;

        SpawnModBiomes = [ModContent.GetInstance<BlueWombBiome>().Type];
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.AddTags(new FlavorTextBestiaryInfoElement(Mod.GetLocalizationKey($"NPCs.{nameof(BlueGaper)}.FlavorText")));
    }

    public ref float Time => ref NPC.ai[0];

    public ref float DashTime => ref NPC.ai[1];

    public ref float Mode => ref NPC.ai[2];

    public override void OnSpawn(IEntitySource source)
    {
        NPC.soundDelay = (int)(Main.rand.Next(500) * NPC.scale);

        NPC.scale *= Main.rand.NextFloat(0.8f, 1.15f);
        NPC.lifeMax = (int)(NPC.lifeMax * NPC.scale);
        NPC.life = NPC.lifeMax;

        DashTime = Main.rand.Next(100, 200);
    }

    public override void AI()
    {
        if (Mode == 0)
        {
            if (Time > 50)
            {
                Time = 0;
                Mode = 1;
            }

            NPC.velocity.Y += 0.01f;
            NPC.velocity *= 0.96f;

            NPC.direction = 0;
            HeadDirection = 0;

            NPC.dontTakeDamage = true;
        }
        else
        {
            NPC.dontTakeDamage = false;

            NPC.TargetClosestUpgraded(faceTarget: false);

            var target = NPC.GetTargetData();
            if (!target.Invalid)
            {
                HeadDirection = Math.Abs(target.Center.X - NPC.Center.X) < 80 ? 0 : Math.Sign(target.Center.X - NPC.Center.X);

                if (Time % 10 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.velocity += Main.rand.NextVector2Circular(2, 2) * Main.rand.NextFloat();
                    NPC.netUpdate = true;
                }

                Vector2 direction = NPC.DirectionTo(target.Center).SafeNormalize(Vector2.Zero);
                NPC.velocity += direction * 0.07f;
            }

            NPC.velocity *= 0.98f;
        }

        if (NPC.soundDelay <= 0)
        {
            NPC.soundDelay = (int)(500 * NPC.scale);
            SoundEngine.PlaySound(SoundID.BloodZombie with { Pitch = -0.5f * NPC.scale }, NPC.Center);
        }

        Time++;
    }

    public override void OnKill()
    {

    }

    public ref float HeadFrame => ref NPC.localAI[0];
    public ref float HeadDirection => ref NPC.localAI[1];

    private Vector2 drawOffset;
    public ref Vector2 DrawOffset => ref drawOffset;

    public override void FindFrame(int frameHeight)
    {
        if (Mode != 0)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter > 8)
            {
                NPC.frameCounter = 0;
                NPC.direction = Math.Clamp(NPC.direction + Math.Sign(NPC.velocity.X), -1, 1);
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
        {
            HeadFrame = 0;
            NPC.direction = 0;
            HeadDirection = 0;
        }

        Texture2D texture = TextureAssets.Npc[Type].Value;

        Rectangle bodyFrame = texture.Frame(3, 3, (int)NPC.direction + 1, 2);
        Rectangle headFrame = texture.Frame(3, 3, (int)HeadDirection + 1, (int)HeadFrame);

        spriteBatch.Draw(texture, NPC.Center - screenPos, bodyFrame, drawColor, NPC.rotation, bodyFrame.Size() / 2, NPC.scale, 0, 0);

        Vector2 headOrigin = new Vector2(headFrame.Width / 2, headFrame.Height / 2 + 8);
        spriteBatch.Draw(texture, NPC.Center - screenPos, headFrame, drawColor, NPC.rotation, headOrigin, NPC.scale, 0, 0);

        return false;
    }
}