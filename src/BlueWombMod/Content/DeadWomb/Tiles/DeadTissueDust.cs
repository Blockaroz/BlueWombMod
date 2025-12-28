using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.Tiles;

public sealed class DeadTissueDust : ModDust
{
    public override void SetStaticDefaults()
    {
        UpdateType = DustID.Poop;
    }

    public override void OnSpawn(Dust dust)
    {
        dust.frame = new Rectangle(0, 12 * Main.rand.Next(3), 10, 10);
        dust.scale *= Main.rand.NextFloat(0.95f, 1.15f);
    }

    public override bool PreDraw(Dust dust)
    {
        Texture2D texture = this.Texture2D.Value;

        Main.EntitySpriteDraw(texture, dust.position - Main.screenPosition, dust.frame, dust.GetAlpha(Lighting.GetColor(dust.position.ToTileCoordinates())), dust.rotation, dust.frame.Size() / 2, dust.scale, 0, 0);

        return false;
    }
}