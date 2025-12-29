using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.HushBoss.Drops;

[AutoloadEquip(EquipType.Head)]
public sealed class HushMask : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 24;
        Item.vanity = true;
        Item.rare = ItemRarityID.Blue;
        Item.value = Terraria.Item.sellPrice(silver: 75);
    }
}
