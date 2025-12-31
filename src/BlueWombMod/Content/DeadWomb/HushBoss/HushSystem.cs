using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BlueWombMod.Content.DeadWomb.HushBoss;

public sealed class HushSystem : ModSystem
{
    private static Vector2 homePosition;

    public static Vector2 HomePosition => homePosition;
    public static Point HomeTile => homePosition.ToTileCoordinates();

    public static bool ActiveFight() => NPC.AnyNPCs(ModContent.NPCType<LittleHush>()); // or hush

    public static void SetHome(Vector2 position)
    {
        homePosition = position;
    }

    public static bool WombInWorld => (Main.expertMode || Main.LocalPlayer.difficulty == 3) && wombPosition.Y > 100;

    private static Point wombPosition;
    public static ref Point WombPosition => ref wombPosition;

    public static bool DownedTheHush { get; set; }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["WombPositionX"] = WombPosition.X;
        tag["WombPositionY"] = WombPosition.Y;
        tag["DownedHush"] = DownedTheHush;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        WombPosition = new Point(tag.Get<int>("WombPositionX"), tag.Get<int>("WombPositionY"));
        DownedTheHush = tag.Get<bool>("DownedHush");
    }
}
