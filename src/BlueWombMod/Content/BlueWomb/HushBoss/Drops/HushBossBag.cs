using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss.Drops;

public sealed class HushBossBag : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.BossBag[Type] = true;
        ItemID.Sets.PreHardmodeLikeBossBag[Type] = true;
        ItemID.Sets.OpenableBag[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.WallOfFleshBossBag);
    }

    public override void ModifyItemLoot(ItemLoot itemLoot)
    {
        itemLoot.Add(ItemDropRule.ByCondition(new Conditions.NotUsedDemonHeart(), ModContent.ItemType<BlueDemonHeart>()));
        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<HushMask>(), chanceDenominator: 7));
        itemLoot.Add(ItemDropRule.Common(ItemID.Pwnhammer));

        itemLoot.Add(ItemDropRule.OneFromOptions(1, 
            ItemID.WarriorEmblem, 
            ItemID.RangerEmblem, 
            ItemID.SorcererEmblem, 
            ItemID.SummonerEmblem
            ));

        itemLoot.Add(ItemDropRule.OneFromOptions(1, 
            ItemID.BreakerBlade, 
            ItemID.ClockworkAssaultRifle, 
            ItemID.LaserRifle, 
            ItemID.FireWhip
            ));

        itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<Hush>()));
    }
}
