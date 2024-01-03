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
    public Dictionary<string, GlobalRepeatingAnimation> EntityAnimations = new();

    public IEnumerable<Animation> GetAnimationsInRange(int xTileStart, int xTileEnd, int yTileStart, int yTileEnd)
    {
        return GetMovingAnimationsInRange(xTileStart: xTileStart, xTileEnd: xTileEnd, yTileStart: yTileStart, yTileEnd: yTileEnd)
            .Concat(StationaryAnimations
                .GetInRange(xTileStart: xTileStart, xTileEnd: xTileEnd, yTileStart: yTileStart, yTileEnd: yTileEnd));
    }

    private IEnumerable<Animation> GetMovingAnimationsInRange(int xTileStart, int xTileEnd, int yTileStart, int yTileEnd)
    {
        return MovingAnimations.Where(mob =>
            xTileStart <= mob.TileX && mob.TileX <= xTileEnd
            && yTileStart <= mob.TileY && mob.TileY <= yTileEnd);
    }

    public Animation PlayOnce(Entity entity, string animationKey)
    {
        return SwapEntityAnimations(entity, new EntityAnimation(entity, animationKey));
    }

    public Animation PlayForDuration(Entity entity, string animationKey, double durationMilliseconds)
    {
        return SwapEntityAnimations(entity, new EntityDurableAnimation(entity, animationKey, durationMilliseconds, playOnceOnly: false));
    }

    private Animation SwapEntityAnimations(Entity entity, Animation newAnimation)
    {
        // Actions can be interrupted if a new animation takes affect before the animation is complete
        // e.g. preventing an attack from executing by aiming for middle of animation frames.
        foreach (var animation in MovingAnimations.Where(animation => animation is IEntityAnimation entityAnimation && entityAnimation.Entity == entity))
        {
            animation.FlagForDespawning = true;
            animation.Clear();
        }
        // We can assume here we don't need to clear any other entity animations because they will be cleared before "After" is called.
        newAnimation.After(() => PlayDefaultInternal(entity, 0));
        MovingAnimations.Add(newAnimation);
        return newAnimation;
    }

    // Should only be called on entity creation. Can assume there was no animations for this entity.
    public Animation InitDefault(Entity entity, double animationOffsetMilliseconds)
    {
        return PlayDefaultInternal(entity, animationOffsetMilliseconds);
    }

    private Animation PlayDefaultInternal(Entity entity, double animationOffsetMilliseconds)
    {
        var newAnimation = new EntityDefaultAnimation(entity, animationOffsetMilliseconds);
        MovingAnimations.Add(newAnimation);
        return newAnimation;
    }

    public Animation InitDefault(Resource resource, double animationOffsetMilliseconds)
    {
        var newAnimation = new ResourceAnimation(resource, animationOffsetMilliseconds);
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

    public Animation Generate(Entity entity, string animationKey, double durationMilliseconds, bool playOnceOnly, Vector2 direction, int xOffset = 0, int yOffset = 0)
    {
        var newAnimation = new EntityEffectAnimation(entity, animationKey, durationMilliseconds, playOnceOnly, direction)
        {
            XOffset = xOffset,
            YOffset = yOffset
        };
        MovingAnimations.Add(newAnimation);
        return newAnimation;
    }

    public Animation GenerateTilesetAnimation(string tileDataKey, TileData tileData)
    {
        if (TilesetAnimations.TryGetValue(tileDataKey, out var existingAnimation))
        {
            return existingAnimation;
        }
        // Tileset animations are not registered in the Animations collection because these will be repeated based on the tile data of the terrain
        // Animations collections is for individual instances of animations representing the other game objects in the world
        var newAnimation = TilesetAnimations[tileDataKey] = new GlobalRepeatingAnimation(tileDataKey, tileData.DefaultAnimationKey);
        return newAnimation;
    }

    public Animation GenerateGlobalEntityAnimation(string spriteSheetKey, EntityData entityData)
    {
        if (EntityAnimations.TryGetValue(spriteSheetKey, out var existingAnimation))
        {
            return existingAnimation;
        }
        // Global entity animations are used only for rendering when presenting the inventory
        var newAnimation = EntityAnimations[spriteSheetKey] = new GlobalRepeatingAnimation(spriteSheetKey, entityData.DefaultAnimationKey);
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
        // don't clear global animations
    }

    public void Update(GameTime gameTime)
    {
        const int AnimationUpdateBuffer = PlayerManager.DespawnRadius + 1;
        var (_, _, xTileStart, yTileStart, xTileEnd, yTileEnd) = GlobalState.ViewportManager.GetTileDimensions(minBuffer: AnimationUpdateBuffer, maxBuffer: AnimationUpdateBuffer);
        Update(
            gameTime,
            GetMovingAnimationsInRange(xTileStart: xTileStart, xTileEnd: xTileEnd, yTileStart: yTileStart, yTileEnd: yTileEnd)
                .Where(animation => !animation.FlagForDespawning)
                .Concat(TilesetAnimations.Values),
            MovingAnimations.RemoveAll);
        Update(
            gameTime,
            StationaryAnimations.GetInRange(xTileStart: xTileStart, xTileEnd: xTileEnd, yTileStart: yTileStart, yTileEnd: yTileEnd),
            StationaryAnimations.RemoveAllInRange(xTileStart: xTileStart, xTileEnd: xTileEnd, yTileStart: yTileStart, yTileEnd: yTileEnd));
    }

    private void Update(
        GameTime gameTime,
        IEnumerable<Animation> animationsToUpdate,
        Func<Predicate<Animation>, int> removeAll)
    {
        var delayedActions = new List<Action>();
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
        removeAll(animation => animation.FlagForDespawning);
        // Defer actions so that Animations can don't modify Animations mid iteration.
        foreach (var action in delayedActions)
        {
            action();
        }
    }
}
