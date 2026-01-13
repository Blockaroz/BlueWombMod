using BlueWombMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Renderers;

namespace BlueWombMod.Content.Particles;

public class HomingTearPopParticle : BaseParticle<HomingTearPopParticle>
{
    public static LazyAsset<Texture2D> TextureAsset { get; } = new LazyAsset<Texture2D>($"{nameof(BlueWombMod)}/Assets/Textures/Particles/{nameof(HomingTearPopParticle)}");

    public Vector2 Position;

    public int TimeLeft;
    public int MaxTime;
    public float Scale;
    private bool Flip;
    public float Rotation;

    public static HomingTearPopParticle RequestNew(Vector2 position, int timeLeft = 20, float scale = 1f, float rotation = 0f)
    {
        var pop = Pool.RequestParticle();
        pop.Position = position;
        pop.TimeLeft = 0;
        pop.MaxTime = timeLeft;
        pop.Scale = scale;
        pop.Flip = Main.rand.NextBool();
        pop.Rotation = rotation;
        return pop;
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        TimeLeft = 0;
        MaxTime = 20;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        base.Update(ref settings);

        TimeLeft++;
        if (TimeLeft >= MaxTime)
        {
            ShouldBeRemovedFromRenderer = true;
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        Texture2D texture = TextureAsset.Value;
        Rectangle frame = texture.Frame(1, 7, 0, (int)((float)TimeLeft / MaxTime * 7));
        Rectangle glowFrame = texture.Frame(1, 7, 0, (int)((float)TimeLeft / MaxTime * 7));
        Color color = Lighting.GetColor(Position.ToTileCoordinates()) * 1.2f;

        SpriteEffects flip = Flip ? SpriteEffects.FlipHorizontally : 0;

        Texture2D glow = Assets.Textures.GlowBig.Value;

        float fadeOut = Utils.GetLerpValue(MaxTime, 0, TimeLeft, true);
        spritebatch.Draw(glow, Position + settings.AnchorPosition, glow.Frame(), Color.White with { A = 0 } * 0.15f * fadeOut, Rotation, glow.Size() / 2, Scale * 0.12f, 0, 0);

        spritebatch.Draw(texture, Position + settings.AnchorPosition, frame, color, Rotation, frame.Size() / 2, Scale, flip, 0);
    }
}