using FarmSim.Entities;
using FarmSim.Player;
using FarmSim.Terrain;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Rendering;

class AnimationManager
{
    public List<Animation> MovingAnimations = new();
    public ChunkPartitionedCollection<Animation> StationaryAnimations = new();
    public Dictionary<string, GlobalRepeatingAnimation> TilesetAnimations = new();
    private readonly ViewportManager _viewportManager;

    public AnimationManager(ViewportManager viewportManager)
    {
        _viewportManager = viewportManager;
    }

    public IEnumerable<Animation> GetAnimationsInRange(int xTileStart, int xTileEnd, int yTileStart, int yTileEnd)
    {
        return MovingAnimations
            .Where(mob =>
                xTileStart <= mob.TileX && mob.TileX <= xTileEnd
                && yTileStart <= mob.TileY && mob.TileY <= yTileEnd)
            .Concat(StationaryAnimations
                .GetInRange(xTileStart: xTileStart, xTileEnd: xTileEnd, yTileStart: yTileStart, yTileEnd: yTileEnd));
    }

    public Animation PlayOnce(Entity entity, string animationKey)
    {
        // Actions can be interrupted if a new animation takes affect before the animation is complete
        // e.g. preventing an attack from executing by aiming for middle of animation frames.
        foreach (var animation in MovingAnimations.Where(animation => animation is IEntityAnimation entityAnimation && entityAnimation.Entity == entity))
        {
            animation.FlagForDespawning = true;
            animation.Clear();
        }
        var newAnimation = new EntityAnimation(entity, animationKey);
        // We can assume here we don't need to clear any other entity animations because they will be cleared before "After" is called.
        newAnimation.After(() => PlayDefaultInternal(entity, 0));
        MovingAnimations.Add(newAnimation);
        return newAnimation;
    }

    // Should only be called on entity creation. Can assume there was no animations for this entity.
    public Animation InitDefault(Entity entity, double animationOffset)
    {
        return PlayDefaultInternal(entity, animationOffset);
    }

    private Animation PlayDefaultInternal(Entity entity, double animationOffset)
    {
        var newAnimation = new EntityDefaultAnimation(entity, animationOffset);
        MovingAnimations.Add(newAnimation);
        return newAnimation;
    }

    public Animation InitDefault(Resource resource, double animationOffset)
    {
        var newAnimation = new ResourceAnimation(resource, animationOffset);
        StationaryAnimations.Add(newAnimation);
        return newAnimation;
    }

    public Animation Generate(int x, int y, string animationKey, float scale)
    {
        var newAnimation = new EffectAnimation(animationKey)
        {
            X = x,
            XInt = x,
            Y = y,
            YInt = y,
            Scale = new Vector2(scale, scale),
        };
        newAnimation.UpdateTileIndex();
        MovingAnimations.Add(newAnimation);
        return newAnimation;
    }

    public Animation Generate(Entity entity, string animationKey, Vector2 direction)
    {
        var newAnimation = new EntityEffectAnimation(entity, animationKey, direction);
        return newAnimation;
    }

    public Animation GenerateTilesetAnimation(string tileDataKey, TileData tileData)
    {
        if (TilesetAnimations.TryGetValue(tileDataKey, out var existingAnimation))
        {
            return existingAnimation;
        }
        var newAnimation = TilesetAnimations[tileDataKey] = new GlobalRepeatingAnimation(tileDataKey, tileData.DefaultAnimationKey);
        // Tileset animations are not registered in the Animations collection because these will be repeated based on the tile data of the terrain
        // Animations collections is for individual instances of animations representing the other game objects in the world
        return newAnimation;
    }

    public void Clear()
    {
        foreach (var animation in MovingAnimations)
        {
            animation.Clear();
        }
        MovingAnimations.Clear();
        foreach (var animation in StationaryAnimations)
        {
            animation.Clear();
        }
        StationaryAnimations.Clear();
    }

    public void Update(GameTime gameTime)
    {
        const int AnimationUpdateBuffer = PlayerManager.DespawnRadius + 1;
        var (_, _, xTileStart, yTileStart, xTileEnd, yTileEnd) = _viewportManager.GetTileDimensions(minBuffer: AnimationUpdateBuffer, maxBuffer: AnimationUpdateBuffer);
        var delayedActions = new List<Action>();
        // only update animations that the player can see
        var animationsToUpdate = GetAnimationsInRange(xTileStart: xTileStart, xTileEnd: xTileEnd, yTileStart: yTileStart, yTileEnd: yTileEnd)
            .Where(animation => !animation.FlagForDespawning)
            .Concat(TilesetAnimations.Values)
            .ToList();
        foreach (var animation in animationsToUpdate)
        {
            switch (animation.Update(gameTime))
            {
                case Animation.DelayedAction.KeyFrame:
                    delayedActions.AddRange(animation.KeyFrameActions);
                    animation.ClearKeyFrame();
                    break;
                case Animation.DelayedAction.After:
                    delayedActions.AddRange(animation.AfterActions);
                    animation.ClearAfter();
                    break;
                default:
                    //case Animation.DelayedAction.None:
                    break;
            }
            if (animation.FlagForDespawning)
            {
                animation.Clear();
            }
        }
        var removed = MovingAnimations.RemoveAll(animation => animation.FlagForDespawning);
        if (removed > 0)
        {
        }
        // Defer actions so that Animations can don't modify Animations mid iteration.
        foreach (var action in delayedActions)
        {
            action();
        }
    }
}
