using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarmSim.Utils;

struct ProcessedSpriteData
{
    public Texture2D Texture;
    public Rectangle SourceRectangle;
    public Vector2 Origin;
    public ICollection<Zoning> Buildable;

    public ProcessedSpriteData(
        Texture2D texture,
        Rectangle sourceRectangle,
        Vector2 origin,
        Zoning[] buildable)
    {
        Texture = texture;
        SourceRectangle = sourceRectangle;
        Origin = origin;
        Buildable = buildable;
    }
}
