using BlueWombMod.Common.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace BlueWombMod.Content.Particles;

public class ExplosionSmearParticle : BaseParticle<ExplosionSmearParticle>
{
    public static LazyAsset<Texture2D> ParticleTexture { get; } = new LazyAsset<Texture2D>($"{nameof(BlueWombMod)}/Assets/Textures/Extras/GlowBig");
}
