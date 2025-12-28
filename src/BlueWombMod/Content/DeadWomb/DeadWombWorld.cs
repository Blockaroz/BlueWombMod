using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace BlueWombMod.Content.DeadWomb;

public sealed class DeadWombWorld : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int index = tasks.FindIndex(n => n.Name == "Shimmer");
        if (index != -1)
        {
            tasks.Insert(index, new PassLegacy("Dead Womb", GenerateDeadWomb));
        }
    }

    private void GenerateDeadWomb(GenerationProgress progress, GameConfiguration configuration)
    {
        DeadWombGenBiome biome = 
    }
}

public sealed class DeadWombGenBiome : MicroBiome
{
    public override bool Place(Point origin, StructureMap structures)
    {
        //structures.AddProtectedStructure(new Rectangle(0, 0, 0, 0));
        return true;
    }
}