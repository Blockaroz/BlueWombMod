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

public sealed class DeadWombWorld : ModSystem
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
        var description = new DeadWombDescription(Center: origin, Radius: 50, []);

        structures.AddProtectedStructure(new Rectangle(0, 0, 0, 0));

        return true;
    }

    private void PlaceMainRoom(DeadWombDescription description)
    {
        for (int j = -description.Radius; j < description.Radius; j++)
        {
            for (int i = -description.Radius; i < description.Radius; i++)
            {
                double distance = Math.Sqrt(i * i + j * j);

                int tileX = description.Center.X + i;
                int tileY = description.Center.X + j;
                Tile tile = Main.tile[tileX, tileY];
            }
        }
    }
}