using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace BlueWombMod.Common.Graphics;

public class ParticleEngine : ModSystem
{
    public static ParticleRenderer Particles { get; } = new ParticleRenderer();

    public static ParticleRenderer WallLayer { get; } = new ParticleRenderer();

    public override void Load()
    {
        On_Main.UpdateParticleSystems += UpdateParticles;
        On_Main.DrawDust += DrawParticles_Default;
        On_Main.DoDraw_WallsAndBlacks += DrawParticles_WallLayer;
    }

    private void UpdateParticles(On_Main.orig_UpdateParticleSystems orig, Main self)
    {
        orig(self);
        Particles.Update();
        WallLayer.Update();
    }

    private void DrawParticles_Default(On_Main.orig_DrawDust orig, Main self)
    {
        orig(self);

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        Particles.Draw(Main.spriteBatch);
        Main.spriteBatch.End();
    }

    private void DrawParticles_WallLayer(On_Main.orig_DoDraw_WallsAndBlacks orig, Main self)
    {
        orig(self);

        //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        WallLayer.Draw(Main.spriteBatch);
        //Main.spriteBatch.End();
    }
}