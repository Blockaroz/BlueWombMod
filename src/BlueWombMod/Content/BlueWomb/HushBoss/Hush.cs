using BlueWombMod.Common.Graphics;
using BlueWombMod.Content.BlueWomb.HushBoss.Drops;
using BlueWombMod.Content.BlueWomb.HushBoss.Minions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace BlueWombMod.Content.BlueWomb.HushBoss;

[AutoloadBossHead]
public sealed partial class Hush : ModNPC
{
    public override void SetStaticDefaults()
    {
        NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
        NPCID.Sets.TeleportationImmune[Type] = true;
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers() { Scale = 0.7f, Position = Vector2.UnitY * -8f });
    }

    public override void SetDefaults()
    {
        NPC.width = 250;
        NPC.height = 250;

        NPC.boss = true;
        NPC.lifeMax = 10000;
        NPC.defense = 50;
        NPC.knockBackResist = 0f;
        NPC.npcSlots = 10f;

        NPC.value = Item.sellPrice(gold: 12);

        NPC.noTileCollide = true;
        NPC.noGravity = true;

        NPC.HitSound = SoundID.NPCHit8 with { MaxInstances = 0, Volume = 0.7f, Pitch = 0.1f };
        NPC.DeathSound = null;

        Music = MusicID.Boss2;

        Renderer = new HushRenderer(NPC);
        NPC.hide = true;

        SpawnModBiomes = [ModContent.GetInstance<BlueWombBiome>().Type];

        AttackPool = new Common.Utilities.WeightedAttackPool<BossState>();
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.AddTags(new FlavorTextBestiaryInfoElement(Mod.GetLocalization($"NPCs.{nameof(Hush)}.FlavorText").Key));
    }

    public ref float State => ref NPC.ai[0];
    public ref float Time => ref NPC.ai[1];
    public ref float MiscTime => ref NPC.ai[2];
    public ref float Phase => ref NPC.ai[3];

    public ref float VisualTime => ref NPC.localAI[0];

    public override void OnSpawn(IEntitySource source)
    {
        SetHome(NPC.Center);

        NPC.dontTakeDamage = true;
        NPC.netUpdate = true;
    }

    public override void AI()
    {
        SetStats();

        Renderer.Reset();
        float wobble = Math.Abs(MathF.Sin(MiscTime * 0.15f));
        Renderer.DrawScale = new Vector2(1f - wobble * 0.02f, 1f + wobble * 0.02f);

        DoCurrentState();

        if (CheckPhaseChangeNeeded())
        {
            NPC.dontTakeDamage = true;
            EndAttack();
        }

        MiscTime++;

        if (HitTime > 0)
        {
            HitTime--;
            float hitWobble = MathF.Sin(HitTime / 2f);
            Renderer.DrawScale += new Vector2(-hitWobble * 0.02f, hitWobble * 0.02f);
        }

        Renderer.Update();
    }

    public override bool CheckActive()
    {
        return false;
    }

    public override bool CheckDead()
    {
        if (NPC.life <= 0 && State != (int)BossState.Death)
        {
            NPC.life = 1;
            NPC.dontTakeDamage = true;
            State = (int)BossState.Death;

            Time = 0;
            MiscTime = 0;

            NPC.BossBar = new NeverValidProgressBar();
            return false;
        }

        return true;
    }

    public void SetStats()
    {
        NPC.defense = 50;
        NPC.takenDamageMultiplier = 0.67f;
    }

    public void BuildLootBox()
    {
        const int radius = 6;
        for (int j = -radius; j < radius; j++)
        {
            for (int i = -radius; i < radius; i++)
            {
                float dist = MathF.Sqrt(i * i + j * j);
                if (dist < radius && dist >= radius - 1.5)
                {
                    Point tilePos = NPC.Center.ToTileCoordinates() + new Point(i, j);
                    WorldGen.PlaceTile(tilePos.X, tilePos.Y, TileID.Obsidian, mute: true);
                }
            }
        }
    }

    public void BreakRadius()
    {
        const int radius = 12;
        for (int j = -radius; j < radius; j++)
        {
            for (int i = -radius; i < radius; i++)
            {
                float dist = MathF.Sqrt(i * i + j * j);
                if (dist < radius)
                {
                    Point tilePos = NPC.Center.ToTileCoordinates() + new Point(i, j);
                    WorldGen.KillTile(tilePos.X, tilePos.Y);
                }
            }
        }
    }

    public int HitTime { get; set; }

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {

    }

    public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        if (HitTime == 0)
            HitTime = 10;
    }

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        if (HitTime == 0)
            HitTime = 10;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(NPC.homeTileX);
        writer.Write(NPC.homeTileY);

        writer.Write(FlyLeaderIndex);
        writer.Write(ContinuumWaves);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        NPC.homeTileX = reader.ReadInt32();
        NPC.homeTileY = reader.ReadInt32();

        FlyLeaderIndex = reader.ReadInt32();
        ContinuumWaves = reader.ReadByte();
    }

    public override void BossLoot(ref int potionType)
    {
        potionType = ItemID.GreaterHealingPotion;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(new DropBasedOnExpertMode(ItemDropRule.Common(ModContent.ItemType<HushMask>(), chanceDenominator: 7), ItemDropRule.DropNothing()));
        npcLoot.Add(new DropBasedOnExpertMode(ItemDropRule.Common(ItemID.Pwnhammer), ItemDropRule.DropNothing()));
        npcLoot.Add(new DropBasedOnExpertMode(ItemDropRule.OneFromOptions(1, 
            ItemID.WarriorEmblem, 
            ItemID.RangerEmblem, 
            ItemID.SorcererEmblem, 
            ItemID.SummonerEmblem
            ), ItemDropRule.DropNothing()));
        npcLoot.Add(new DropBasedOnExpertMode(ItemDropRule.OneFromOptions(1, 
            ItemID.BreakerBlade, 
            ItemID.ClockworkAssaultRifle, 
            ItemID.LaserRifle, 
            ItemID.FireWhip
            ), ItemDropRule.DropNothing()));

        npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<HushBossBag>()));
    }

    public override void OnKill()
    {
        HushSystem.DownedTheHush = true;

        return;

        bool inHardmode = Main.hardMode;
        WorldGen.StartHardmode();

        // Do jungle status message if necessary
        if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3 && !inHardmode)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
                Main.NewText(Lang.misc[32].Value, 50, byte.MaxValue, 130);
            else if (Main.dedServ)
                ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Lang.misc[32].Key), new Color(50, 255, 130));
        }

        NPC.SetEventFlagCleared(ref inHardmode, 19); // This doesn't do anything as of yet but WoF sets it
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        return false;
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        if (NPC.Distance(Main.MouseWorld) < NPC.width / 2)
            boundingBox = new Rectangle(NPC.Hitbox.X - 25, NPC.Hitbox.Y - 25, NPC.Hitbox.Width + 50, NPC.Hitbox.Height + 50);
        else
            boundingBox = Rectangle.Empty;
    }

    public HushRenderer Renderer { get; private set; }

    public override void FindFrame(int frameHeight)
    {
        if (NPC.IsABestiaryIconDummy)
        {
            Renderer.Reset();
            Renderer.UpdateForDummy();
        }
    }

    public override void DrawBehind(int index)
    {
        Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
    }

    public static RenderTarget2D DrawTarget { get; private set; }

    public override void Load()
    {
        Main.QueueMainThreadAction(() => DrawTarget = new RenderTarget2D(Main.instance.GraphicsDevice, 800, 800));
    }

    public override void Unload()
    {
        DrawTarget.Dispose();
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Renderer?.Draw(spriteBatch, screenPos);

        return false;
    }
}