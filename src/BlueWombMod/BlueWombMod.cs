using BlueWombMod.Core;
using ReLogic.Content.Sources;
using Terraria.ModLoader;

namespace BlueWombMod;

public sealed class BlueWombMod : Mod
{
    public override IContentSource CreateDefaultContentSource() => new AssetDirectorySource(base.CreateDefaultContentSource());
}