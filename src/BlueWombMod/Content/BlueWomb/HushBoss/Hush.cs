using BlueWombMod.Common.Graphics;
using BlueWombMod.Content.BlueWomb.HushBoss.Drops;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
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
        NPC.defense = 30;
        NPC.knockBackResist = 0f;
        NPC.npcSlots = 10f;

        NPC.noTileCollide = true;
        NPC.noGravity = true;

        NPC.HitSound = SoundID.NPCHit8 with { MaxInstances = 0, Volume = 0.7f, Pitch = 0.1f };
        NPC.DeathSound = null;

        Music = MusicID.Boss2;

        Renderer = new HushRenderer(NPC);
        NPC.hide = true;
    }

    public ref float Phase => ref NPC.ai[0];
    public ref float State => ref NPC.ai[1];
    public ref float Time => ref NPC.ai[2];
    public ref float MiscTime => ref NPC.ai[3];

    public ref float VisualTime => ref NPC.localAI[0];

    public override void OnSpawn(IEntitySource source)
    {
        SetHome(NPC.Center);

        NPC.dontTakeDamage = true;
    }

    public override void AI()
    {
        CreativeShockPlayers();

        Renderer.Reset();

        DoSpawn();

        Renderer.Update();
    }

    public void CreativeShockPlayers()
    {
        foreach (Player player in Main.ActivePlayers)
        {
            if (player.ZoneBlueWomb)
                player.AddBuff(BuffID.NoBuilding, 30, true);
        }
    }

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        base.ModifyIncomingHit(ref modifiers);
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
        boundingBox = NPC.Hitbox;
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