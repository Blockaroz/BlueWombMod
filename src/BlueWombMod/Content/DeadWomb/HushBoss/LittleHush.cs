using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BlueWombMod.Content.DeadWomb.HushBoss;

public sealed class LittleHush : ModNPC
{
    public override void SetStaticDefaults()
    {
        NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
        NPCID.Sets.TeleportationImmune[Type] = true;

        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true });
    }

    public override void SetDefaults()
    {
        NPC.width = 30;
        NPC.height = 38;

        NPC.boss = true;
        NPC.lifeMax = 6000;
        NPC.defense = 30;

        NPC.behindTiles = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        HushSystem.SetHome(NPC.Center);
    }

    public override void AI()
    {
        
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        return false;
    }
}
