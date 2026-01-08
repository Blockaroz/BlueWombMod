using Terraria;
using Terraria.ModLoader;

namespace BlueWombMod.Common.Utilities;

public class MusicSystem : ModSystem
{
    public override void Load()
    {
        On_Main.UpdateAudio += EditAudio;
    }

    private void EditAudio(On_Main.orig_UpdateAudio orig, Main self)
    {
        orig(self);

    }
}