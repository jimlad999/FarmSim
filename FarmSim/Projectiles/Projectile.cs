using FarmSim.Effects;
using FarmSim.Entities;
using Microsoft.Xna.Framework;

namespace FarmSim.Projectiles;

abstract class Projectile : Entity, IDespawnble
{
    public Entity Owner;//either Mob or Player
    public bool FlagForDespawning { get; set; } = false;

    public Vector2 NormalizedDirection;
    public double Speed;
    public Effect Effect;
    public string DespawnAnimationKey;

    public virtual void Update(GameTime gameTime)
    {
        var distancePerFrame = gameTime.ElapsedGameTime.TotalSeconds * Speed;
        var newX = X + NormalizedDirection.X * distancePerFrame;
        var newY = Y + NormalizedDirection.Y * distancePerFrame;
        UpdateForces(gameTime, x: ref newX, y: ref newY);
        var newXInt = (int)newX;
        var newYInt = (int)newY;
        if (GlobalState.TerrainManager.ValidateMovement(oldX: XInt, oldY: YInt, newX: newXInt, newY: newYInt))
        {
            X = newX;
            Y = newY;
            XInt = newXInt;
            YInt = newYInt;
            this.UpdateTileIndex();
            UpdateFacingDirection(directionX: NormalizedDirection.X, directionY: NormalizedDirection.Y);
        }
        else
        {
            FlagForDespawning = true;
        }
    }

    public bool DetectCollision(Entity entity)
    {
        var xDiff = X - entity.X - entity.HitboxXOffset;
        var yDiff = Y - entity.Y - entity.HitboxYOffset;
        var sumHitRadii = HitRadiusPow2 + entity.HitRadiusPow2;
        var collision = xDiff * xDiff + yDiff * yDiff < sumHitRadii;
        return collision;
    }
}
