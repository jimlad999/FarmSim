using FarmSim.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FarmSim.Mobs;

abstract class Mob : Entity, IDespawnble
{
    public MobData Metadata;
    public Tags[] Tags;
    public int HP;
    public bool FlagForDespawning { get; set; } = false;
    public bool Hit;

    protected Behaviour[] _behaviours;

    // called after construction
    public abstract void InitBehaviours();

    public void Update(GameTime gameTime)
    {
        if (Hit)
        {
            if (UpdateForces(gameTime))
            {
                XInt = (int)X;
                YInt = (int)Y;
                this.UpdateTileIndex();
            }
            return;
        }
        foreach (var behaviour in _behaviours)
        {
            if (behaviour.TryExecute(this, gameTime))
                break;
        }
    }

    public bool TryMove(GameTime gameTime, Vector2 normalizedDirection, int targetX, int targetY)
    {
        var movementPerFrame = gameTime.ElapsedGameTime.TotalSeconds * Metadata.Speed;
        var xDirectionPositive = normalizedDirection.X > 0;
        var yDirectionPositive = normalizedDirection.Y > 0;
        var newX = X + normalizedDirection.X * movementPerFrame;
        var newY = Y + normalizedDirection.Y * movementPerFrame;
        // TODO: detect collision or unpassable terrain and return false
        X = newX;
        Y = newY;
        UpdateForces(gameTime);
        XInt = (int)X;
        YInt = (int)Y;
        this.UpdateTileIndex();
        UpdateFacingDirection(directionX: normalizedDirection.X, directionY: normalizedDirection.Y);
        return (xDirectionPositive ? XInt < targetX : XInt > targetX)
            && (yDirectionPositive ? YInt < targetY : YInt > targetY);
    }

    public IEnumerable<string> GetDrops()
    {
        return Metadata.Drops.PickItems();
    }
}
