using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.HushBoss;

public sealed class HushSystem : ModSystem
{
    private static Vector2 homePosition;

    public static Vector2 HomePosition => homePosition;

    public static bool Active => Main.npc.Any(n => n.active && (n.type == ModContent.NPCType<LittleHush>() /* || n.type == ModContent.NPCType<Hush>() */));

    public static void SetHome(Vector2 position)
    {
        homePosition = position;
    }
}
