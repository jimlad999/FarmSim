using FarmSim.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace FarmSim.Utils;

class EntitySpriteSheet : SpriteSheet<EntityData, EntitySpriteSheet.Metadata>
{
    public EntitySpriteSheet(SpriteBatch spriteBatch, EntitiesData tilesetData)
    {
        ProcessData(spriteBatch, tilesetData);
    }

    public ProcessedSpriteData this[string entity, FacingDirection facingDirection]
    {
        get
        {
            var metadata = _metadata[entity];
            return CreateProcessedSpriteData(_renderTarget, metadata, facingDirection);
        }
    }

    protected override Metadata GetMetadata(EntityData data, Rectangle destinationRectangle)
    {
        return new Metadata
        {
            Origin = data.Origin?.Convert() ?? Vector2.Zero,
            SourceRectangles = data.DirectionFrames.ToDictionary(
                a => a.Key,
                a => new Rectangle(
                    x: destinationRectangle.X + a.Value.X,
                    y: destinationRectangle.Y + a.Value.Y,
                    width: data.FrameWidth,
                    height: data.FrameHeight)),
            Buildable = data.Buildable,
        };
    }

    protected override ProcessedSpriteData CreateProcessedSpriteData(RenderTarget2D renderTarget, Metadata metadata)
    {
        return CreateProcessedSpriteData(renderTarget, metadata, FacingDirection.Down);
    }

    private static ProcessedSpriteData CreateProcessedSpriteData(RenderTarget2D renderTarget, Metadata metadata, FacingDirection facingDirection)
    {
        return new ProcessedSpriteData(
            renderTarget,
            metadata.SourceRectangles[facingDirection],
            metadata.Origin,
            metadata.Buildable);
    }

    public struct Metadata
    {
        public Vector2 Origin;
        public Dictionary<FacingDirection, Rectangle> SourceRectangles;
        public Zoning[] Buildable;
    }
}
