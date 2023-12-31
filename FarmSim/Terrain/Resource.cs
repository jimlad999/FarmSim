using FarmSim.Entities;
using FarmSim.Rendering;

namespace FarmSim.Terrain;

class Resource : Entity, IDespawnble
{
    public string ItemId;
    public Tags PrimaryTag;

    public bool FlagForDespawning { get; set; }

    public Resource(
        string itemId,
        Tags primaryTag,
        string tilesetKey,
        int tileX,
        int tileY)
    {
        ItemId = itemId;
        PrimaryTag = primaryTag;
        EntitySpriteKey = tilesetKey;
        // TODO: Set up correctly from metadata files
        DefaultAnimationKey = "idle";
        TileX = tileX;
        XInt = (tileX * Renderer.TileSize) + Renderer.TileSizeHalf;
        X = XInt;
        TileY = tileY;
        YInt = (tileY * Renderer.TileSize) + Renderer.TileSizeHalf;
        Y = YInt;
        InitDefaultAnimation();
    }

    public override void InitDefaultAnimation(double animationOffsetMilliseconds = 0)
    {
        // calls the type overload.
        GlobalState.AnimationManager.InitDefault(this, animationOffsetMilliseconds);
    }
}
