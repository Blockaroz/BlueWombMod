using BlueWombMod.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.ID;

namespace BlueWombMod.Content.Particles;

public sealed class LittleAngryBugParticle : BaseParticle<LittleAngryBugParticle>
{
    public Vector2 Position;
    public Vector2 Velocity;
    public int TimeLeft;
    public int MaxTime;
    public float Scale;

    public static LittleAngryBugParticle RequestNew(Vector2 position, Vector2 velocity, int timeLeft, float scale)
    {
        var bug = LittleAngryBugParticle.Pool.RequestParticle();
        bug.Position = position;
        bug.Velocity = velocity;
        bug.TimeLeft = 0;
        bug.MaxTime = timeLeft;
        bug.Scale = scale;
        return bug;
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        MaxTime = 1;
        TimeLeft = 0;
        Scale = 1f;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        Position += Velocity;

        Velocity += Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat(0.5f);
        Velocity *= 0.97f;

        TimeLeft++;
        if (TimeLeft >= MaxTime)
        {
            ShouldBeRemovedFromRenderer = true;
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        Texture2D texture = TextureAssets.Extra[ExtrasID.RoamingFly].Value;
        int frameCount = (TimeLeft / 3) % 2;
        Rectangle frame = texture.Frame(1, 2, 0, frameCount);
        float colorFade = Utils.Remap(TimeLeft, 0, 20, 0f, 1f) * Utils.Remap(TimeLeft, MaxTime, MaxTime - 90, 0f, 1f);
        Color color = Lighting.GetColor(Position.ToTileCoordinates()) * colorFade;
        SpriteEffects flip = Velocity.X < 0 ? SpriteEffects.FlipHorizontally : 0;
        spritebatch.Draw(texture, Position + settings.AnchorPosition, frame, color, 0f, frame.Size() / 2, Scale, flip, 0);
    }
}
