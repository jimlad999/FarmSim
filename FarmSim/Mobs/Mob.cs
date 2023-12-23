using FarmSim.Entities;
using FarmSim.Rendering;
using Microsoft.Xna.Framework;

namespace FarmSim.Mobs;

abstract class Mob : Entity
{
    public MobData Metadata;
    public Tags[] Tags;
    public int HP;
    public bool FlagForDespawning = false;

    protected Behaviour[] _behaviours;

    // called after construction
    public abstract void Init();

    public void Update(GameTime gameTime)
    {
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
        FacingDirection = normalizedDirection.X > normalizedDirection.Y
            ? (xDirectionPositive ? FacingDirection.Right : FacingDirection.Left)
            : (yDirectionPositive ? FacingDirection.Down : FacingDirection.Up);
        X = newX;
        XInt = (int)X;
        Y = newY;
        YInt = (int)Y;
        TileX = XInt / Renderer.TileSize;
        if (XInt < 0) --TileX;
        TileY = YInt / Renderer.TileSize;
        if (YInt < 0) --TileY;
        return (xDirectionPositive ? XInt < targetX : XInt > targetX)
            && (yDirectionPositive ? YInt < targetY : YInt > targetY);
    }
}
