using Microsoft.Xna.Framework;

namespace Utils.Data;

public class OriginData
{
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2 Convert()
    {
        return new Vector2(x: X, y: Y);
    }
}
