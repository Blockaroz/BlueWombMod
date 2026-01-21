using BlueWombMod.Content.BlueWomb.HushBoss;
using BlueWombMod.Content.BlueWomb.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace BlueWombMod.Content.BlueWomb;

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

    public override void Load()
    {
        On_TrackGenerator.IsLocationInvalid += PreventTracksFromCuttingThrough;
    }

    private bool PreventTracksFromCuttingThrough(On_TrackGenerator.orig_IsLocationInvalid orig, int x, int y)
    {
        if (Main.tile[x, y].TileType == ModContent.TileType<DeadTissueBlockUnsafe>() || Main.tile[x, y].WallType == ModContent.WallType<DeadTissueWallUnsafe>())
        {
            return true;
        }

        return orig(x, y);
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
                    if (!WorldGen.SolidTile(tile) || !TileID.Sets.Stone[tile.TileType])
                    {
                        tile.ClearEverything();
                        tile.ResetToType(TileID.Stone);
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
        HushSystem.WombInWorld = true;
    }
}