using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BlueWombMod.Content.BlueWomb.HushBoss;

public sealed class HushSystem : ModSystem
{
    public static bool ActiveFight() => NPC.AnyNPCs(ModContent.NPCType<LittleHush>()) || NPC.AnyNPCs(ModContent.NPCType<Hush>()); // or hush

    public static bool WombInWorld => wombPosition.Y > 100;

    public const int WOMB_RADIUS = 50;

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