using FarmSim.Entities;
using FarmSim.Terrain;
using Microsoft.Xna.Framework;

namespace FarmSim.Rendering;

class ResourceAnimation : Animation, IEntityAnimation
{
    public Entity Entity { get; set; }

    public ResourceAnimation(Resource resource, double animationOffsetMilliseconds)
    {
        Entity = resource;
        SpriteSheetKey = resource.EntitySpriteKey;
        AnimationKey = resource.DefaultAnimationKey;
        Scale = new Vector2(resource.Scale, resource.Scale);
        Color = resource.Color;
        X = Entity.X;
        Y = Entity.Y;
        XInt = Entity.XInt;
        YInt = Entity.YInt;
        TileX = Entity.TileX;
        TileY = Entity.TileY;
        InitAnimationOffset(animationOffsetMilliseconds);
    }

    public override DelayedAction Update(GameTime gameTime)
    {
        FlagForDespawning = Entity is IDespawnble despawnable && despawnable.FlagForDespawning;
        UpdateAnimation(gameTime, GetFrames(), out var _);
        return DelayedAction.None;
    }
}
