using ReLogic.Content;
using ReLogic.Content.Sources;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BlueWombMod.Core;

public class AssetDirectorySource : IContentSource
{
    public IContentValidator ContentValidator { get; set; }

    public RejectedAssetCollection Rejections => _source.Rejections;

    private readonly IContentSource _source;

    public AssetDirectorySource(IContentSource source)
    {
        _source = source;
    }

    private string RewritePath(string path)
    {
        if (path.StartsWith("Content"))
        {
            return path.Replace("Content", "Assets/Textures");
        }

        if (path.StartsWith("Common"))
        {
            return path.Replace("Common", "Assets/Textures");
        }

        return path;
    }

    IEnumerable<string> IContentSource.EnumerateAssets() => _source.EnumerateAssets().Select(RewritePath);

    string IContentSource.GetExtension(string assetName) => _source.GetExtension(RewritePath(assetName));

    Stream IContentSource.OpenStream(string fullAssetName) => _source.OpenStream(RewritePath(fullAssetName));
}