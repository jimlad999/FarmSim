using FarmSim.Rendering;
using Microsoft.Xna.Framework;
using System;

namespace FarmSim.Entities;

abstract class Entity
{
    public FacingDirection FacingDirection = FacingDirection.Down;

    public int HitRadiusPow2;

    // world position
    public double X;
    public int XInt;
    public int TileX;
    // world position
    public double Y;
    public int YInt;
    public int TileY;

    public string EntitySpriteKey;
    public Color Color = Color.White;
    public float Scale = 1f;
    public int HitboxXOffset = 0;
    public int HitboxYOffset = 0;

    public void UpdateTilePosition()
    {
        TileX = XInt / Renderer.TileSize;
        if (XInt < 0) --TileX;
        TileY = YInt / Renderer.TileSize;
        if (YInt < 0) --TileY;
    }

    protected void UpdateFacingDirection(double directionX, double directionY)
    {
        FacingDirection = Math.Abs(directionX) > Math.Abs(directionY)
            ? directionX < 0 ? FacingDirection.Left : FacingDirection.Right
            : directionY < 0 ? FacingDirection.Up : FacingDirection.Down;
    }
}
