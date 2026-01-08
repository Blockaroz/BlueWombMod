using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss.Drops;

public sealed class SkinScissors : ModItem
{
    public sealed class SkinScissorsPlayer : ModPlayer
    {
        public bool Enabled { get; set; }

        public override void ResetEffects()
        {
            Enabled = false;
        }

        public override void Load()
        {
            On_Player.ClearMiningCacheAt += On_Player_ClearMiningCacheAt;
        }

        private void On_Player_ClearMiningCacheAt(On_Player.orig_ClearMiningCacheAt orig, Player self, int x, int y, int hitTileCacheType)
        {
            orig(self, x, y, hitTileCacheType);
        }
    }

    public override void SetDefaults()
    {
        Item.DefaultToAccessory(newwidth: 30, newheight: 28);
        Item.rare = ItemRarityID.Green;
    }
}