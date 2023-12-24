using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmSim.Utils;

class Tileset : SpriteSheet<TileData, Tileset.Metadata>
{
    public Tileset(SpriteBatch spriteBatch, TilesetData tilesetData)
    {
        ProcessData(spriteBatch, tilesetData);
    }

    protected override Metadata GetMetadata(TileData data, Rectangle destinationRectangle)
    {
        return new Metadata
        {
            Origin = data.Origin?.Convert() ?? Vector2.Zero,
            SourceRectangle = destinationRectangle,
            Buildable = data.Buildable,
        };
    }

    protected override ProcessedSpriteData CreateProcessedSpriteData(RenderTarget2D renderTarget, Metadata metadata)
    {
        return new ProcessedSpriteData(
            renderTarget,
            metadata.SourceRectangle,
            metadata.Origin,
            metadata.Buildable);
    }

    public struct Metadata
    {
        public Vector2 Origin;
        public Rectangle SourceRectangle;
        public Zoning[] Buildable;
    }
}
