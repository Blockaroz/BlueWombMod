using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace BlueWombMod.Common;

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
