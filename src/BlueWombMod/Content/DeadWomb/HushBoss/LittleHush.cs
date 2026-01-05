using BlueWombMod.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.HushBoss;

[AutoloadBossHead]
public sealed partial class LittleHush : ModNPC
{
    public override void SetStaticDefaults()
    {
        NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
        NPCID.Sets.TeleportationImmune[Type] = true;

        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true });
    }

    public override void SetDefaults()
    {
        NPC.width = 34;
        NPC.height = 40;

        NPC.boss = true;
        NPC.lifeMax = 4000;
        NPC.defense = 30;
        NPC.knockBackResist = 0f;

        NPC.noTileCollide = false;
        NPC.noGravity = true;
        NPC.behindTiles = true;

        NPC.BossBar = new NeverValidProgressBar();
        Music = 0;

        AttackPool = new WeightedAttackPool<BossState>();
    }

    public override bool? CanFallThroughPlatforms() => true;

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        scale *= 1.1f;
        position.Y += 10;
        return true;
    }

    public ref float Phase => ref NPC.ai[0];
    public ref float State => ref NPC.ai[1];
    public ref float Time => ref NPC.ai[2];
    public ref float MiscTime => ref NPC.ai[3];

    public ref float VisualTime => ref NPC.localAI[0];

    public int AddBossArmorFactor { get; private set; }

    public override void OnSpawn(IEntitySource source)
    {
        SetHome(HushSystem.WombPosition.ToWorldCoordinates());

        NPC.dontTakeDamage = true;
    }

    public override void AI()
    {
        DrawScale = Vector2.One;
        DrawOffset = Vector2.Zero;
        WingFrame = 0;

        switch (Phase)
        {
            case (int)BossPhase.Scared:
                Phase_Scared();
                break;

            case (int)BossPhase.Standing:
                Phase_Standing();
                break;

            case (int)BossPhase.Angel:
                Phase_Angel();
                break;
        }

        DoCurrentState();

        NPC.scale = 1.5f;
        Lighting.AddLight(NPC.Center, Color.SlateGray.ToVector3() * NPC.Opacity * 0.5f);

        if (AddBossArmorFactor > 0)
        {
            AddBossArmorFactor--;
        }

        VisualTime++;
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        boundingBox = new Rectangle((int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height);
    }

    public override Color? GetAlpha(Color drawColor)
    {
        return drawColor * NPC.Opacity;
    }

    public static LazyAsset<Texture2D> WingsTexture { get; } = new LazyAsset<Texture2D>($"{nameof(BlueWombMod)}/Assets/Textures/DeadWomb/HushBoss/LittleHushWings");

    public ref float AnimationFrame => ref NPC.localAI[1];

    public enum HushyPose
    {
        Crouched,
        SpitCrouched,
        Standing,
        SpitStanding,
        RaiseArmsStanding
    }

    public int GetSpittingFrame()
    {
        switch (AnimationFrame)
        {
            default:
            case (int)HushyPose.Crouched:
            case (int)HushyPose.SpitCrouched:
                return (int)HushyPose.SpitCrouched;
            case (int)HushyPose.Standing:
            case (int)HushyPose.SpitStanding:
            case (int)HushyPose.RaiseArmsStanding:
                return (int)HushyPose.SpitStanding;
        }
    }

    public int GetArmRaiseFrame()
    {
        switch (AnimationFrame)
        {
            default:
                return (int)HushyPose.Crouched;
            case (int)HushyPose.Standing:
            case (int)HushyPose.SpitStanding:
            case (int)HushyPose.RaiseArmsStanding:
                return (int)HushyPose.RaiseArmsStanding;
        }
    }

    public ref float WingFrame => ref NPC.localAI[2];
    public ref float WingFlapFrame => ref NPC.localAI[3];

    public enum HushyWingPose
    {
        Flapping,
        Splayed,
        Closed
    }

    public override void FindFrame(int frameHeight)
    {
        switch (WingFrame)
        {
            case (int)HushyWingPose.Flapping:

                NPC.frameCounter++;

                if (NPC.frameCounter > 5)
                {
                    NPC.frameCounter = 0;

                    WingFlapFrame++;
                    if (WingFlapFrame >= 6)
                    {
                        WingFlapFrame = 0;
                    }
                }

                break;

            case (int)HushyWingPose.Splayed:

                NPC.frameCounter = 0;
                WingFlapFrame = 0;

                break;

            case (int)HushyWingPose.Closed:

                NPC.frameCounter = 0;
                WingFlapFrame = 6;

                break;
        }
    }

    private Vector2 drawOffset;
    public ref Vector2 DrawOffset => ref drawOffset;

    private Vector2 drawScale;
    public ref Vector2 DrawScale => ref drawScale;

    public float FightModeStrength { get; set; }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
        {
            DrawScale = Vector2.One;
        }

        drawColor = GetAlpha(drawColor) ?? drawColor;

        Vector2 center = (NPC.Center + DrawOffset).Floor();

        Texture2D fade = Assets.Textures.GlowBig.Value;
        spriteBatch.Draw(fade, center - screenPos, fade.Frame(), Color.Black * 0.25f * NPC.Opacity, NPC.rotation, fade.Size() / 2, NPC.scale * 0.25f, 0, 0);

        Texture2D wings = WingsTexture.Value;
        Rectangle wingFrame = wings.Frame(1, 7, 0, (int)WingFlapFrame);
        Color wingColor = (drawColor * 1.2f) with { A = 150 };

        if (Phase == (int)BossPhase.Angel && WingFrame != (int)HushyWingPose.Closed)
        {
            spriteBatch.Draw(wings, center - screenPos, wingFrame, wingColor, NPC.rotation, wingFrame.Size() / 2, NPC.scale * DrawScale, 0, 0);
        }

        Texture2D texture = TextureAssets.Npc[Type].Value;
        Rectangle frame = texture.Frame(3, 5, NPC.direction + 1, (int)AnimationFrame);

        spriteBatch.Draw(texture, center - screenPos, frame, drawColor, NPC.rotation, frame.Size() / 2, NPC.scale * DrawScale, 0, 0);

        if (Phase == (int)BossPhase.Angel && WingFrame == (int)HushyWingPose.Closed)
        {
            spriteBatch.Draw(wings, center - screenPos, wingFrame, wingColor, NPC.rotation, wingFrame.Size() / 2, NPC.scale * DrawScale, 0, 0);
        }

        // Debug
        /*
        StringBuilder text = new StringBuilder();
        text.AppendLine($"{(BossState)State}");
        text.AppendLine($"{(BossAttack)Attack}");
        Utils.DrawBorderString(spriteBatch, text.ToString(), NPC.Center - new Vector2(0, 20) * DrawScale - screenPos, Color.White, 1f, 0.5f, 1f);
        */
        return false;
    }
}
