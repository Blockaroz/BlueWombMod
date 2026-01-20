using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.BackupIO;

namespace BlueWombMod.Content.BlueWomb.HushBoss;

public sealed class HushSystem : ModSystem
{
    public static bool ActiveFight() => NPC.AnyNPCs(ModContent.NPCType<LittleHush>()) || NPC.AnyNPCs(ModContent.NPCType<Hush>()); // or hush

    public const int WOMB_RADIUS = 50;

    public static bool WombInWorld { get; internal set; }

    private static Point wombPosition;
    public static ref Point WombPosition => ref wombPosition;

    public static bool DownedTheHush { get; set; }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["WombInWorld"] = WombInWorld;
        tag["WombPositionX"] = WombPosition.X;
        tag["WombPositionY"] = WombPosition.Y;
        tag["DownedHush"] = DownedTheHush;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        WombInWorld = tag.Get<bool>("WombInWorld");
        WombPosition = new Point(tag.Get<int>("WombPositionX"), tag.Get<int>("WombPositionY"));
        DownedTheHush = tag.Get<bool>("DownedHush");
    }
}

public sealed class HushCreativeShockPlayer : ModPlayer
{
    public override void PreUpdateBuffs()
    {
        if (Player.ZoneBlueWomb && HushSystem.ActiveFight())
        {
            Player.AddBuff(BuffID.NoBuilding, 60, true);
            Player.noBuilding = true;
        }
    }
}