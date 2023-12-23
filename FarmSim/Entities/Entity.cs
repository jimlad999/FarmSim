using Microsoft.Xna.Framework;

namespace FarmSim.Entities;

abstract class Entity
{
    public FacingDirection FacingDirection = FacingDirection.Down;

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
}
