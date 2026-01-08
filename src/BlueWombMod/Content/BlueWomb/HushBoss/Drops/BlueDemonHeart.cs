using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss.Drops;

public sealed class BlueDemonHeart : ModItem
{
    public override LocalizedText DisplayName => Lang.GetItemName(ItemID.DemonHeart);

    public override LocalizedText Tooltip => Lang.GetTooltip(ItemID.DemonHeart)._text;

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.DemonHeart);
    }

    public override bool CanUseItem(Player player)
    {
        return !player.extraAccessory && Main.expertMode;
    }

    public override bool? UseItem(Player player)
    {
        player.extraAccessory = true;
        NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);
        return true;
    }
}