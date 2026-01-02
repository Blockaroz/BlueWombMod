using BlueWombMod.Content.DeadWomb.HushBoss;
using BlueWombMod.Content.DeadWomb.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace BlueWombMod.Content.DeadWomb;

public sealed class BlueWombGeneration : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int index = tasks.FindIndex(n => n.Name == "Hives");
        if (index != -1)
        {
            tasks.Insert(index + 1, new PassLegacy("Blue Womb", GenerateBlueWomb));
        }

        int smoothIndex = tasks.FindIndex(n => n.Name == "Smooth World");
        if (smoothIndex != -1)
        {
            tasks.Insert(smoothIndex + 1, new PassLegacy("Blue Womb Non Solid", SetWombTilesNonsolid));
        }

        tasks.Add(new PassLegacy("Blue Womb Solid", SetWombTilesSolid));
    }

    private void GenerateBlueWomb(GenerationProgress progress, GameConfiguration configuration)
    {
        int distance = WorldGen.genRand.Next(350, 500);
        int x = GenVars.dungeonSide > 0 ? distance : Main.maxTilesX - distance;
        int y = Main.UnderworldLayer - WorldGen.genRand.Next(140, 160);

        progress.Message = "Ripping apart Mom's Heart";
        progress.Value = 1.0;

        var biome = GenVars.configuration.CreateBiome<BlueWombGenBiome>();
        biome.Place(new Point(x, y), GenVars.structures);
    }

    private void SetWombTilesNonsolid(GenerationProgress progress, GameConfiguration configuration)
    {
        Main.tileSolid[ModContent.TileType<DeadTissueBlockUnsafe>()] = false;
    }

    private void SetWombTilesSolid(GenerationProgress progress, GameConfiguration configuration)
    {
        Main.tileSolid[ModContent.TileType<DeadTissueBlockUnsafe>()] = true;
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

        var biome = new BlueWombGenBiome();

        biome.Place(new Point(x, y), new StructureMap());
    }
}

public sealed class BlueWombGenBiome : MicroBiome
{
    public record struct WombDescription(Point Center, int Radius, List<WombLootCellDescription> Loot);

    public record struct WombLootCellDescription(Point Center, int Radius);

    public override bool Place(Point origin, StructureMap structures)
    {
        int radius = HushSystem.WOMB_RADIUS;
        var description = new WombDescription(Center: origin, Radius: radius, []);

        PlaceMainRoom(description);

        structures.AddProtectedStructure(new Rectangle(description.Center.X - radius, description.Center.Y - radius, radius * 2, radius * 2), 9);

        return true;
    }

    private void PlaceMainRoom(WombDescription description)
    {
        ushort tileType = (ushort)ModContent.TileType<DeadTissueBlockUnsafe>();
        ushort wallType = (ushort)ModContent.WallType<DeadTissueWallUnsafe>();

        const int dirtPadding = 40;
        for (int j = -description.Radius - dirtPadding; j < description.Radius + dirtPadding; j++)
        {
            for (int i = -description.Radius - dirtPadding; i < description.Radius + dirtPadding; i++)
            {
                double distance = Math.Sqrt(i * i + j * j);

                int tileX = description.Center.X + i;
                int tileY = description.Center.Y + j;
                if (!WorldGen.InWorld(tileX, tileY, 2))
                    continue;

                Tile tile = Main.tile[tileX, tileY];

                if (distance < description.Radius + dirtPadding / 2)
                {
                    if (!tile.HasTile || !TileID.Sets.Stone[tile.TileType])
                    {
                        tile.ClearEverything();
                        tile.ResetToType(TileID.Dirt);
                    }
                }

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

        HushSystem.WombPosition = description.Center;
    }
}