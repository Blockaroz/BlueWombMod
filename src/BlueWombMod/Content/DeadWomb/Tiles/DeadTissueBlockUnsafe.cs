using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.Tiles;

public sealed class DeadTissueBlockUnsafeItem : ModItem
{
    public override string Texture => ModContent.GetInstance<DeadTissueBlockItem>().Texture;

    public override void SetStaticDefaults()
    {
        ItemID.Sets.DrawUnsafeIndicator[Type] = true;
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<DeadTissueBlockItem>();
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<DeadTissueBlockUnsafe>());
    }
}

public sealed class DeadTissueBlockUnsafe : ModTile
{
    public override string Texture => ModContent.GetInstance<DeadTissueBlock>().Texture;

    public override void SetStaticDefaults()
    {
        TileID.Sets.Suffocate[Type] = true;

        Main.tileBrick[Type] = true;
        Main.tileMergeDirt[Type] = true;

        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        MineResist = 2.5f;
        HitSound = null;
		DustType = ModContent.DustType<DeadTissueDust>();
        AddMapEntry(new Color(81, 115, 173));
	}

	public override bool CanDrop(int i, int j)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Projectile.NewProjectileDirect(Entity.GetSource_NaturalSpawn(), new Vector2(i * 16 + 8, j * 16 + 8), Vector2.Zero, ModContent.ProjectileType<DeadTissueBlockGrowth>(), 0, 0);
        }

        return false;
    }
}

public sealed class DeadTissueBlockGrowth : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;

        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 240;
    }

    public int Variant { get => (int)Projectile.localAI[0]; set => Projectile.localAI[0] = value; }

    private Vector2 visualOffset;

    public override void OnSpawn(IEntitySource source)
    {
        Variant = Main.rand.Next(3);
        visualOffset = Main.rand.NextVector2Circular(12, 12);
        Projectile.rotation = Main.rand.NextFloat(-0.5f, 0.5f);
        Projectile.timeLeft = Main.rand.Next(300, 350);
    }

    public override void AI()
    {
        Projectile.velocity = Vector2.Zero;

        Point tilePosition = Projectile.Center.ToTileCoordinates();
        if (tilePosition.X < 2 || tilePosition.X > Main.maxTilesX - 2 || tilePosition.X < 2 || tilePosition.X > Main.maxTilesX - 2)
        {
            Projectile.Kill();
            return;
        }

        Tile tileUp = Main.tile[tilePosition.X, tilePosition.Y - 1];
        Tile tileDown = Main.tile[tilePosition.X, tilePosition.Y + 1];
        Tile tileLeft = Main.tile[tilePosition.X - 1, tilePosition.Y];
        Tile tileRight = Main.tile[tilePosition.X + 1, tilePosition.Y];
        var tileType = ModContent.TileType<DeadTissueBlockUnsafe>();

        bool seedUp = tileUp.HasTile && tileUp.TileType == tileType;
        bool seedDown = tileDown.HasTile && tileDown.TileType == tileType;
        bool seedLeft = tileLeft.HasTile && tileLeft.TileType == tileType;
        bool seedRight = tileRight.HasTile && tileRight.TileType == tileType;

        if (seedUp || seedDown || seedLeft || seedRight)
        {
            Projectile.timeLeft -= 2;
        }

        Projectile.scale = Utils.GetLerpValue(90, 0, Projectile.timeLeft, true) * (1 + Utils.GetLerpValue(5, 30, Projectile.timeLeft, true) * 0.5f);

        Vector2 offset = Main.rand.NextVector2Circular(24, 24);
        if (seedUp)
        {
            offset += Vector2.UnitY * 16f;
        }
        if (seedDown)
        {
            offset += Vector2.UnitY * -16f;
        }
        if (seedLeft)
        {
            offset += Vector2.UnitX * 16f;
        }
        if (seedRight)
        {
            offset += Vector2.UnitX * -16f;
        }

        if (Projectile.timeLeft < 60)
        {
            visualOffset *= 0.93f;
        }

        Dust dust = Dust.NewDustPerfect(Projectile.Center + visualOffset + offset, ModContent.DustType<DeadTissueDust>(), -offset * 0.05f, Scale: Projectile.scale * 0.8f);
        dust.noGravity = true;
        dust.fadeIn = Projectile.scale;

        Projectile.soundDelay--;
        if (Projectile.soundDelay < 0)
        {
            Projectile.soundDelay = 15;
            SoundEngine.PlaySound(SoundID.NPCHit9 with { MaxInstances = 0, Pitch = Projectile.scale - 1f, Volume = Projectile.scale * 0.15f }, Projectile.Center);
        }
    }

    public override void OnKill(int timeLeft)
    {
        Point tilePosition = Projectile.Center.ToTileCoordinates();
        if (tilePosition.X < 2 || tilePosition.X > Main.maxTilesX - 2 || tilePosition.X < 2 || tilePosition.X > Main.maxTilesX - 2)
        {
            return;
        }

        var tileType = ModContent.TileType<DeadTissueBlockUnsafe>();
        if (Main.tile[tilePosition.X, tilePosition.Y].TileType == tileType)
        {
            return;
        }

        WorldGen.KillTile(tilePosition.X, tilePosition.Y);
        WorldGen.PlaceTile(tilePosition.X, tilePosition.Y, tileType, mute: true, forced: true);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;

        Rectangle frame = texture.Frame(1, 3, 0, Variant);

        var flip = Projectile.direction > 0 ? 0 : SpriteEffects.FlipHorizontally;
        Main.EntitySpriteDraw(texture, Projectile.Center + visualOffset - Main.screenPosition, frame, Lighting.GetColor(Projectile.Center.ToTileCoordinates()), Projectile.rotation, frame.Size() / 2, Projectile.scale, flip, 0);

        return false;
    }
}