using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlueWombMod.Common.Graphics;

// Adapted Daybreak Concept
public readonly record struct SpriteBatchSnapshot(
    SpriteSortMode SortMode,
    BlendState BlendState,
    SamplerState SamplerState,
    DepthStencilState DepthStencilState,
    RasterizerState Rasterizer,
    Effect Effect,
    Matrix TransformMatrix
    );

public static class SpritebatchSnapshotExtension
{
    extension(SpriteBatch spriteBatch)
    {
        public void End(out SpriteBatchSnapshot snapshot)
        {
            snapshot = new SpriteBatchSnapshot(
                spriteBatch.sortMode,
                spriteBatch.blendState,
                spriteBatch.samplerState,
                spriteBatch.depthStencilState,
                spriteBatch.rasterizerState,
                spriteBatch.customEffect,
                spriteBatch.transformMatrix);
            spriteBatch.End();
        }

        public void Begin(SpriteBatchSnapshot snapshot)
        {
            spriteBatch.Begin(
                snapshot.SortMode,
                snapshot.BlendState,
                snapshot.SamplerState,
                snapshot.DepthStencilState,
                snapshot.Rasterizer,
                snapshot.Effect,
                snapshot.TransformMatrix);
        }
    }
}