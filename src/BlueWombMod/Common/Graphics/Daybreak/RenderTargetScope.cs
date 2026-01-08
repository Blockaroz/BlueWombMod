using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace BlueWombMod.Common.Graphics;

// Adapted from Daybreak's RenderTargetScope
public struct RenderTargetScope : IDisposable
{
    private RenderTargetBinding[] bindings;

    public RenderTargetScope(RenderTarget2D target, RenderTargetUsage usage = RenderTargetUsage.PreserveContents, bool clear = false, Color? clearColor = null)
    {
        GetAndPreserveBindings(usage);

        Main.instance.GraphicsDevice.SetRenderTarget(target);
        if (clear)
            Main.instance.GraphicsDevice.Clear(clearColor ?? Color.Transparent);
    }

    private RenderTargetBinding[] GetAndPreserveBindings(RenderTargetUsage usage)
    {
        bindings = Main.instance.GraphicsDevice.GetRenderTargets();
        foreach (var binding in bindings)
        {
            if (binding.RenderTarget is not RenderTarget2D rt)
                continue;

            rt.RenderTargetUsage = usage;
        }

        return bindings;
    }

    public void Dispose()
    {
        Main.instance.GraphicsDevice.SetRenderTargets(bindings);
    }
}
