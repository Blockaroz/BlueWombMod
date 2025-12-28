using BlueWombMod.Core;
using ReLogic.Content.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace BlueWombMod;

public sealed class BlueWombMod : Mod
{
	public override IContentSource CreateDefaultContentSource() => new AssetDirectorySource(base.CreateDefaultContentSource());
}
