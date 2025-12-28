using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.Tiles;

public sealed class DeadTissueDust : ModDust
{
    public override void SetStaticDefaults()
    {
        UpdateType = DustID.FoodPiece;
    }

    public override void OnSpawn(Dust dust)
    {
        dust.frame = new Rectangle(0, 6 * Main.rand.Next(3), 6, 6);
    }

    public override bool PreDraw(Dust dust)
    {
        Texture2D texture = this.Texture2D.Value;

        Main.EntitySpriteDraw(texture, dust.position, dust.frame, dust.GetColor(dust.color), dust.rotation, dust.frame.Size() / 2, dust.scale, 0, 0);

        return false;
    }
}
