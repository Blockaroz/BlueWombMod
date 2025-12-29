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

public sealed class DeadTissueWallUnsafeItem : ModItem
{
    public override string Texture => ModContent.GetInstance<DeadTissueWallItem>().Texture;

    public override void SetStaticDefaults()
    {
        ItemID.Sets.DrawUnsafeIndicator[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<DeadTissueWallUnsafe>());
    }
}

public sealed class DeadTissueWallUnsafe : ModWall
{
    public override string Texture => ModContent.GetInstance<DeadTissueWall>().Texture;

    public override void SetStaticDefaults()
    {
        Main.wallHouse[Type] = false;
        Main.wallBlend[Type] = ModContent.WallType<DeadTissueWall>();

        HitSound = null;
        DustType = ModContent.DustType<DeadTissueDust>();
        AddMapEntry(new Color(44, 63, 104));
    }

    public override bool Drop(int i, int j, ref int type)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Projectile.NewProjectileDirect(Entity.GetSource_NaturalSpawn(), new Vector2(i * 16 + 8, j * 16 + 8), Vector2.Zero, ModContent.ProjectileType<DeadTissueWallGrowth>(), 0, 0);
        }

        return false;
    }
}
public sealed class DeadTissueWallGrowth : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;

        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 120;
        Projectile.hide = true;
    }

    public int Variant { get => (int)Projectile.localAI[0]; set => Projectile.localAI[0] = value; }

    public bool Safe { get => Projectile.ai[0] == 1; set => Projectile.ai[0] = value ? 1 : 0; }

    private Vector2 visualOffset;

    public override void OnSpawn(IEntitySource source)
    {
        Variant = Main.rand.Next(3);
        visualOffset = Main.rand.NextVector2Circular(12, 12);
        Projectile.rotation = Main.rand.NextFloat(-0.5f, 0.5f);
        Projectile.timeLeft = Main.rand.Next(100, 140);
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
        var tileType = Safe ? ModContent.WallType<DeadTissueWall>() : ModContent.WallType<DeadTissueWallUnsafe>();

        bool seedUp = tileUp.WallType == tileType;
        bool seedDown = tileDown.WallType == tileType;
        bool seedLeft = tileLeft.WallType == tileType;
        bool seedRight = tileRight.WallType == tileType;

        if (seedUp || seedDown || seedLeft || seedRight)
        {
            Projectile.timeLeft -= 2;
        }

        Projectile.scale = Utils.GetLerpValue(90, 0, Projectile.timeLeft, true) * (1 + Utils.GetLerpValue(5, 30, Projectile.timeLeft, true) * 0.5f);

        Vector2 offset = Main.rand.NextVector2Circular(30, 30);
        if (seedUp)
        {
            offset += Vector2.UnitY * -16f;
        }
        if (seedDown)
        {
            offset += Vector2.UnitY * 16f;
        }
        if (seedLeft)
        {
            offset += Vector2.UnitX * -16f;
        }
        if (seedRight)
        {
            offset += Vector2.UnitX * 16f;
        }

        if (Projectile.timeLeft < 60)
        {
            visualOffset *= 0.93f;
        }

        if (Main.rand.NextBool())
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + visualOffset + offset, ModContent.DustType<DeadTissueDust>(), -offset * 0.05f, Scale: Projectile.scale * 0.75f);
            dust.noGravity = true;
        }

        Projectile.soundDelay--;
        if (Projectile.soundDelay < 0)
        {
            Projectile.soundDelay = 11;
            SoundEngine.PlaySound(SoundID.NPCHit13 with { MaxInstances = 0, Pitch = Projectile.scale, Volume = Projectile.scale * 0.05f }, Projectile.Center);
        }
    }

    public override void OnKill(int timeLeft)
    {
        Point tilePosition = Projectile.Center.ToTileCoordinates();
        if (tilePosition.X < 2 || tilePosition.X > Main.maxTilesX - 2 || tilePosition.X < 2 || tilePosition.X > Main.maxTilesX - 2)
        {
            return;
        }

        var tileType = Safe ? ModContent.WallType<DeadTissueWall>() : ModContent.WallType<DeadTissueWallUnsafe>();
        if (Main.tile[tilePosition.X, tilePosition.Y].WallType == tileType)
        {
            return;
        }

        WorldGen.KillWall(tilePosition.X, tilePosition.Y);
        WorldGen.PlaceWall(tilePosition.X, tilePosition.Y, tileType, mute: true);
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCsAndTiles.Add(index);
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