using BlueWombMod.Content.DeadWomb.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace BlueWombMod.Content.DeadWomb;

public sealed class DeadWombGeneration : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int index = tasks.FindIndex(n => n.Name == "Underworld");
        if (index != -1)
        {
            tasks.Insert(index + 1, new PassLegacy("Dead Womb", GenerateDeadWomb));
        }
    }

    private void GenerateDeadWomb(GenerationProgress progress, GameConfiguration configuration)
    {
        int distance = WorldGen.genRand.Next(250, 400);
        int x = GenVars.dungeonSide > 0 ? distance : Main.maxTilesX - distance;
        int y = Main.UnderworldLayer - WorldGen.genRand.Next(200, 250);

        progress.Message = "Ripping apart Mom's Heart";

        int tries = 50;
        var biome = new DeadWombGenBiome();
        while (tries > 0)
        {
            if (biome.Place(new Point(x, y), GenVars.structures))
				break;

			tries--;
        }
    }
}

public sealed class GenCommand : ModCommand
{
    public override string Command => "womb";

    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        int x = (int)(Main.MouseWorld.X / 16);
        int y = (int)(Main.MouseWorld.Y / 16);

        var biome = new DeadWombGenBiome();

        biome.Place(new Point(x, y), new StructureMap());
    }
}

public sealed class DeadWombGenBiome : MicroBiome
{
    public record struct DeadWombDescription(Point Center, int Radius, List<LootCellDescription> Loot);

    public record struct LootCellDescription(Point Center, int Radius);

    public override bool Place(Point origin, StructureMap structures)
    {
        var description = new DeadWombDescription(Center: origin, Radius: 48, []);

        PlaceMainRoom(description);

        structures.AddProtectedStructure(new Rectangle(description.Center.X - description.Radius, description.Center.Y - description.Radius, description.Radius * 2, description.Radius * 2));

        return true;
    }

    private void PlaceMainRoom(DeadWombDescription description)
    {
        ushort tileType = (ushort)ModContent.TileType<DeadTissueBlockUnsafe>();
        ushort wallType = (ushort)ModContent.WallType<DeadTissueWallUnsafe>();

        const int padding = 16;
        for (int j = -description.Radius - padding; j < description.Radius + padding; j++)
        {
            for (int i = -description.Radius - padding; i < description.Radius + padding; i++)
            {
                double distance = Math.Sqrt(i * i + j * j);

                int tileX = description.Center.X + i;
                int tileY = description.Center.Y + j;
                if (!WorldGen.InWorld(tileX, tileY, 2))
                    continue;

                Tile tile = Main.tile[tileX, tileY];

                if (distance < description.Radius + WorldGen.genRand.Next(3) - 3)
                {
                    if (distance > description.Radius - 14.5)
                    {
                        tile.ResetToType(tileType);
                    }
                    else
                    {
                        tile.ClearEverything();
                    }

                    if (distance < description.Radius - 4 - WorldGen.genRand.Next(2))
                    {
                        tile.WallType = wallType;
                    }
                }
            }
        }
    }
}