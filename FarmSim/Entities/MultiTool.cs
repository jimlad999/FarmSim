using FarmSim.Rendering;
using FarmSim.Terrain;
using FarmSim.Utils;
using Microsoft.Xna.Framework;
using System;

namespace FarmSim.Entities;

class MultiTool
{
    public int HarvestMultiplier = 1;
    public int BaseDamage = 10;
    public int Damage
    {
        get
        {
            return BaseDamage;
        }
    }
    private int ToolReach = 1;//tiles
    private int WeaponReachPow2 = 110 * 110;
    private double ArcHalfRadians = 0.872665;//+/- 40 degrees of facing direction

    public bool IsTileWithinRange(Entity entity, Tile targetTile)
    {
        var xDiff = targetTile.X - entity.TileX;
        var yDiff = targetTile.Y - entity.TileY;
        var xMod = entity.XInt.Mod(Renderer.TileSize);
        var yMod = entity.YInt.Mod(Renderer.TileSize);
        const int XLowerTileBound = 12;
        const int XUppterTileBound = Renderer.TileSize - XLowerTileBound;
        const int YLowerTileBound = 6;
        const int YUppterTileBound = Renderer.TileSize - YLowerTileBound;
        var xOffset = xDiff + (xMod <= XLowerTileBound && xDiff < 0 ? 1 : xMod > XUppterTileBound && xDiff > 0 ? -1 : 0);
        var yOffset = yDiff + (yMod <= YLowerTileBound && yDiff < 0 ? 1 : yMod >= YUppterTileBound && yDiff > 0 ? -1 : 0);
        return Math.Abs(xOffset) <= ToolReach
            && Math.Abs(yOffset) <= ToolReach;
    }

    public ArcRange WeaponRange(Entity entity, int xOffset, int yOffset, Vector2 facingDirection)
    {
        return new ArcRange(
            x: entity.XInt + xOffset - (int)(facingDirection.X * 30),
            y: entity.YInt + yOffset - (int)(facingDirection.Y * 30),
            facingDirection: facingDirection,
            arcHalfRadians: ArcHalfRadians,
            reachPow2: WeaponReachPow2);
    }
}
