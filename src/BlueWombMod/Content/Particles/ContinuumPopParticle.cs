using BlueWombMod.Common.Graphics;
using BlueWombMod.Content.BlueWomb.HushBoss.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Renderers;

namespace BlueWombMod.Content.Particles;

public class ContinuumPopParticle : BaseParticle<ContinuumPopParticle>
{
    public static LazyAsset<Texture2D> TextureAsset { get; } = new LazyAsset<Texture2D>($"{nameof(BlueWombMod)}/Assets/Textures/Particles/{nameof(ContinuumPopParticle)}");

    public Vector2 Position;

    public int TimeLeft;
    public int MaxTime;
    public float Scale;
    private bool Flip;
    private float ColorProgress;

    public static ContinuumPopParticle RequestNew(Vector2 position, float colorProgress, int timeLeft = 20, float scale = 1f)
    {
        var pop = Pool.RequestParticle();
        pop.Position = position;
        pop.TimeLeft = 0;
        pop.MaxTime = timeLeft;
        pop.Scale = scale;
        pop.ColorProgress = colorProgress;
        pop.Flip = Main.rand.NextBool();
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
        Rectangle frame = texture.Frame(2, 7, 0, (int)((float)TimeLeft / MaxTime * 7));
        Rectangle glowFrame = texture.Frame(2, 7, 1, (int)((float)TimeLeft / MaxTime * 7));

        SpriteEffects flip = Flip ? SpriteEffects.FlipHorizontally : 0;

        Texture2D glow = Assets.Textures.GlowBig.Value;

        float fadeOut = Utils.GetLerpValue(MaxTime, 0, TimeLeft, true);
        spritebatch.Draw(glow, Position + settings.AnchorPosition, glow.Frame(), Color.White with { A = 0 } * 0.15f * fadeOut, 0, glow.Size() / 2, Scale * 0.12f, 0, 0);

        Color color = ContinuumTear.GetContinuumColor(ColorProgress, 1f);
        spritebatch.Draw(texture, Position + settings.AnchorPosition, frame, color, 0f, frame.Size() / 2, Scale, flip, 0);

        Color glowColor = Color.Lerp(color, Color.White, 0.5f) * 0.66f;
        spritebatch.Draw(texture, Position + settings.AnchorPosition, glowFrame, glowColor with { A = 0 }, 0f, glowFrame.Size() / 2, Scale, flip, 0);
    }
}