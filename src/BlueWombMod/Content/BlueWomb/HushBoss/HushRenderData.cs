using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace BlueWombMod.Content.BlueWomb.HushBoss;

public struct HushRenderData
{
    public HushRenderData(NPC nPC)
    {
        NPC = nPC;
    }

    public NPC NPC { get; }

    public bool ShowFace { get; set; }

    public bool InGround { get; set; }

    public bool HideMound { get; set; }

    public void Reset()
    {
        ShowFace = true;
        InGround = false;
        HideMound = false;
    }

    public static Asset<Texture2D> Texture => TextureAssets.Npc[ModContent.NPCType<Hush>()];
    public static LazyAsset<Texture2D> EyesTexture { get; } = new LazyAsset<Texture2D>($"{nameof(BlueWombMod)}/Assets/Textures/BlueWomb/HushBoss/HushEyes");
    public static LazyAsset<Texture2D> MouthTexture { get; } = new LazyAsset<Texture2D>($"{nameof(BlueWombMod)}/Assets/Textures/BlueWomb/HushBoss/HushMouth");

    public void Draw(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        if (NPC.IsABestiaryIconDummy)
        {

        }
    }
}