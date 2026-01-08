using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;
using static BlueWombMod.Content.BlueWomb.HushBoss.LittleHush;

namespace BlueWombMod.Content.BlueWomb.HushBoss;

public sealed partial class Hush : ModNPC
{
    public static readonly NPCUtils.SearchFilter<Player> TargetOnlyPlayersInWomb = player => player.ZoneBlueWomb;

    public bool CheckForTarget()
    {
        var result = NPCUtils.SearchForTarget(NPC, NPCUtils.TargetSearchFlag.Players, Hush.TargetOnlyPlayersInWomb);
        NPC.target = result.NearestTargetIndex;

        if (NPC.GetTargetData().Invalid)
        {
            State = (int)BossState.Despawning;
            Time = 0;
            MiscTime = 0;

            return false;
        }

        return true;
    }
}