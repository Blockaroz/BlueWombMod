using BlueWombMod.Content.DeadWomb.HushBoss;
using BlueWombMod.Content.DeadWomb.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb;

public sealed class SuspiciousChild : ModNPC
{
    public override void SetStaticDefaults()
    {
        NPCID.Sets.SavesAndLoads[Type] = true;
        NPCID.Sets.CantTakeLunchMoney[Type] = true;
        NPCID.Sets.ReflectStarShotsInForTheWorthy[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.width = 34;
        NPC.height = 40;

        NPC.lifeMax = 500;
        NPC.defense = 5;

        NPC.noGravity = true;
        NPC.noTileCollide = false;
        NPC.knockBackResist = 0.2f;

        NPC.rarity = 1;
    }

    public ref float Time => ref NPC.ai[0];

    public override void AI()
    {
        float homeX = HushSystem.WombPosition.X * 16 + 8;

        NPC.velocity.X += Math.Sign(homeX - NPC.Center.X) * 0.08f * Utils.GetLerpValue(0, 20, Math.Abs(homeX - NPC.Center.X), true);
        NPC.velocity.X *= 0.97f;
        if (Math.Abs(NPC.velocity.X) < 0.01f)
        {
            NPC.velocity.X = 0;
        }

        int distanceToFloor = DistanceToFloor();
        NPC.velocity.Y = (distanceToFloor - 15) * 0.1f + MathF.Sin(Time / 120f * MathHelper.TwoPi) * 0.5f;

        DrawOffset.X = MathF.Sin(Time / 6f * MathHelper.TwoPi);

        NPC.TargetClosest(false);

        NPCAimedTarget target = NPC.GetTargetData();

        if (NPC.Distance(target.Center) < 500)
        {
            FaceTargetSpecial();
        }

        if (Time % 15 == 0 && NPC.life < NPC.lifeMax)
        {
            NPC.life = Math.Min(NPC.life + 1, NPC.lifeMax);
        }

        Time++;

        if (Time >= 240)
        {
            Time = 0;
        }

        Lighting.AddLight(NPC.Center, Color.SlateGray.ToVector3() * NPC.Opacity * 0.5f);
    }

    public int DistanceToFloor()
    {
        int x = (int)(NPC.Center.X / 16);
        int y = (int)(NPC.Center.Y / 16);
        for (int i = 0; i < 100; i++)
        {
            if (WorldGen.SolidOrSlopedTile(x, y + i))
            {
                return i;
            }
        }

        return -1;
    }

    public void FaceTargetSpecial()
    {
        NPCAimedTarget target = NPC.GetTargetData();
        if (target.Invalid)
        {
            NPC.direction = 0;
            return;
        }

        if (Math.Abs(target.Center.X - NPC.Center.X) < 80)
        {
            NPC.direction = 0;
        }
        else
        {
            NPC.direction = Math.Sign(target.Center.X - NPC.Center.X);
        }
    }

    public override bool PreKill()
    {
        NPC.active = false;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            // SuspiciousChildPlacementSystem.Notify();
            NPC hushy = NPC.NewNPCDirect(NPC.GetSpawnSourceForNaturalSpawn(), NPC.Bottom, ModContent.NPCType<LittleHush>());
            hushy.velocity = NPC.velocity;
            hushy.netUpdate = true;
        }

        return false;
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        boundingBox = new Rectangle((int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height);
    }

    private Vector2 drawOffset;
    public ref Vector2 DrawOffset => ref drawOffset;

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        drawColor = GetAlpha(drawColor) ?? drawColor;

        Texture2D fade = Assets.Textures.GlowBig.Value;
        spriteBatch.Draw(fade, NPC.Center + DrawOffset - screenPos, fade.Frame(), Color.Black * 0.25f, NPC.rotation, fade.Size() / 2, NPC.scale * 0.25f, 0, 0);

        Texture2D texture = TextureAssets.Npc[Type].Value;
        Rectangle frame = texture.Frame(3, 1, NPC.direction + 1, 0);

        spriteBatch.Draw(texture, NPC.Center + DrawOffset - screenPos, frame, drawColor, NPC.rotation, frame.Size() / 2, NPC.scale, 0, 0);

        return false;
    }
}

public sealed class SuspiciousChildPlacementSystem : ModSystem
{
    private static double delay;
    private static double recheck;

    public override void PostUpdateWorld()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (!HushSystem.WombInWorld)
            return;

        delay -= Main.desiredWorldEventsUpdateRate;
        if (delay < 0)
        {
            delay = 0;
        }

        recheck -= Main.desiredWorldEventsUpdateRate;
        if (recheck < 0)
        {
            recheck = 0;
        }

        if (HushSystem.ActiveFight() || NPC.AnyNPCs(ModContent.NPCType<SuspiciousChild>()))
        {
            recheck = 60;
            return;
        }

        if (delay == 0 && recheck == 0)
        {
            recheck = 240;
            if (NPC.AnyDanger())
            {
                recheck *= 2;
            }
            else
            {
                TryPlacingChild(HushSystem.WombPosition.X, HushSystem.WombPosition.Y);
            }
        }
    }

    public static void Notify()
    {
        delay = 40000;
    }

    public static void TryPlacingChild(int x, int y)
    {
        bool playerCannotSee = !WorldGen.PlayerLOS(x, y - 20) && !WorldGen.PlayerLOS(x - 20, y + 10) && !WorldGen.PlayerLOS(x + 20, y + 10);

        if (true)
        {
            if (WorldGen.SolidOrSlopedTile(x, y))
                return;

            bool notEnoughArea = false;
            var wallType = ModContent.WallType<DeadTissueWallUnsafe>();
            for (int j = -10; j < 10; j++)
            {
                for (int i = -10; i < 10; i++)
                {
                    double distance = Math.Sqrt(i * i + j * j);
                    Tile tile = Framing.GetTileSafely(x + i, y + j);
                    if (distance < 10 && (tile.WallType != wallType || WorldGen.SolidOrSlopedTile(tile)))
                    {
                        notEnoughArea = true;
                        break;
                    }
                }
            }

            if (!notEnoughArea)
            {
                for (int i = 0; i < 50; i++)
                {
                    if (WorldGen.SolidOrSlopedTile(x, y + i))
                    {
                        y += i - 10;
                        break;
                    }
                }

                NPC.NewNPCDirect(Entity.GetSource_NaturalSpawn(), new Vector2(x, y).ToWorldCoordinates(), ModContent.NPCType<SuspiciousChild>());
            }
        }
    }
}