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
        NPC.width = 30;
        NPC.height = 38;

        NPC.boss = true;
        NPC.lifeMax = 6000;
        NPC.defense = 30;
        NPC.knockBackResist = 0;

        NPC.noTileCollide = false;
        NPC.noGravity = true;
        NPC.behindTiles = true;

        NPC.BossBar = new NeverValidProgressBar();
        Music = 0;
    }

    public override void OnSpawn(IEntitySource source)
    {
        HushSystem.SetHome(NPC.Center);
        NPC.homeTileX = HushSystem.HomeTile.X;
        NPC.homeTileY = HushSystem.HomeTile.Y;

        NPC.dontTakeDamage = true;
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        scale *= 1.1f;
        position.Y += 10;
        return true;
    }

    public ref float State => ref NPC.ai[0];
    public ref float Attack => ref NPC.ai[1];
    public ref float Time => ref NPC.ai[2];

    public override void AI()
    {
        NPC.direction = 0;

        switch (State)
        {
            case (int)BossState.Spawning:
                DoSpawn();
                break;

            default: 
            case (int)BossState.Despawning:
                break;

            case (int)BossState.BigHushTime:
                break;

            case (int)BossState.PhaseShakingCrying:
                Phase_Scared();
                break;

            case (int)BossState.PhaseBrave:
                Phase_Scared();
                break;

            case (int)BossState.PhaseAngelic:
                Phase_Scared();
                break;
        }
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        boundingBox = new Rectangle((int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height);
    }

    public override Color? GetAlpha(Color drawColor)
    {
        return drawColor;
    }

    public static Asset<Texture2D> WingsTexture { get; private set; }

    public override void Load()
    {
        WingsTexture = ModContent.Request<Texture2D>(Texture + "Wings");
    }

    public int AnimationFrame { get; private set; }

    private Vector2 drawOffset;
    public ref Vector2 DrawOffset => ref drawOffset;

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        drawColor = GetAlpha(drawColor) ?? drawColor;

        Texture2D texture = TextureAssets.Npc[Type].Value;

        Rectangle frame = texture.Frame(3, 4, NPC.direction + 1, AnimationFrame);

        spriteBatch.Draw(texture, NPC.Center + DrawOffset - screenPos, frame, drawColor, NPC.rotation, frame.Size() / 2, NPC.scale, 0, 0);

        return false;
    }
}
