using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FarmSim.Entities;

abstract class Entity
{
    public List<Tags> Tags { get; init; } = new();
    public Vector2 Position { get; private set; }
    public FacingDirection FacingDirection { get; private set; }
    public Color? Color { get; private set; }

}
