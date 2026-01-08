using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.ModLoader;

namespace BlueWombMod;

public readonly record struct LazyAsset<T>(string Path) where T : class
{
    public Asset<T> Asset => lazy.Value;

    public T Value => Asset.Value;

    private readonly Lazy<Asset<T>> lazy = new Lazy<Asset<T>>(() => ModContent.Request<T>(Path, AssetRequestMode.ImmediateLoad));

    public static implicit operator Asset<T>(LazyAsset<T> asset) => asset.Asset;
}

public static class Assets
{
    public static class Textures
    {
        public static LazyAsset<Texture2D> GlowBig { get; } = new LazyAsset<Texture2D>($"{nameof(BlueWombMod)}/Assets/Textures/Extras/GlowBig");
    }

    public static class Music
    {

    }
}