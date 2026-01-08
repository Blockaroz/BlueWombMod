using BlueWombMod.Content.BlueWomb.HushBoss.Drops;
using Microsoft.Xna.Framework;
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
    }

    public override void SetDefaults()
    {
        NPC.width = 230;
        NPC.height = 230;

        NPC.boss = true;
        NPC.lifeMax = 10000;
        NPC.defense = 30;
        NPC.knockBackResist = 0f;
        NPC.npcSlots = 10f;

        NPC.noTileCollide = true;
        NPC.noGravity = true;

        NPC.HitSound = SoundID.NPCHit9;
        NPC.DeathSound = null;

        Music = 0;
    }

    public override bool? CanFallThroughPlatforms() => true;

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        return false;
    }

    public ref float Phase => ref NPC.ai[0];
    public ref float State => ref NPC.ai[1];
    public ref float Time => ref NPC.ai[2];
    public ref float MiscTime => ref NPC.ai[3];

    public ref float VisualTime => ref NPC.localAI[0];

    public override void OnSpawn(IEntitySource source)
    {
        
    }

    public override void AI()
    {

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

        NPC.SetEventFlagCleared(ref inHardmode, 19);
    }
}