using FarmSim.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.Rendering;

namespace FarmSim.Utils;

class EntitySpriteSheet
{
    private readonly Dictionary<string, Metadata> _metadata = new();
    private readonly RenderTarget2D _renderTarget;

    public EntitySpriteSheet(
        SpriteBatch spriteBatch,
        EntitiesData tilesetData)
    {
        using (var disposeScope = new DeferredDisposeScope())
        {
            var totalWidth = 0;
            var totalHeight = 0;
            var sprites = new Dictionary<string, (Texture2D, Rectangle)>();
            foreach (var data in tilesetData.Entities)
            {
                var texture = Texture2D.FromFile(spriteBatch.GraphicsDevice, $"{tilesetData.BaseFolder}/{data.Value.Source}");
                // destinationRectangle to render to _renderTarget.
                // Will then be used as the base for each frames sourceRectangles from the _renderTarget.
                var destinationRectangle = new Rectangle(
                    x: 0,
                    y: totalHeight,
                    width: texture.Width,
                    height: texture.Height);
                var sourceRectangles = data.Value.DirectionFrames.ToDictionary(
                    a => a.Key,
                    a => new Rectangle(
                        x: destinationRectangle.X + a.Value.X,
                        y: destinationRectangle.Y + a.Value.Y,
                        width: data.Value.FrameWidth,
                        height: data.Value.FrameHeight));
                _metadata[data.Key] = new Metadata
                {
                    SourceRectangles = sourceRectangles,
                    Origin = data.Value.Origin?.Convert() ?? Vector2.Zero,
                };
                sprites[data.Key] = (texture, destinationRectangle);

                disposeScope.Disposables.Add(texture);

                // +1 so that there is a small gap between sprites so that you don't get bleeding around the edges
                totalHeight += texture.Height + 1;
                if (texture.Width > totalWidth)
                {
                    totalWidth = texture.Width;
                }
            }
            _renderTarget = new RenderTarget2D(
                spriteBatch.GraphicsDevice,
                width: totalWidth,
                height: totalHeight);
            using (RenderTargetScope.Create(spriteBatch, _renderTarget))
            {
                foreach (var sprite in sprites.Values)
                {
                    spriteBatch.Draw(sprite.Item1, destinationRectangle: sprite.Item2, Color.White);
                }
            }
        }
    }

    public ProcessedEntityData this[string entity, FacingDirection facingDirection]
    {
        get
        {
            var metadata = _metadata[entity];
            return new ProcessedEntityData(
                _renderTarget,
                metadata.SourceRectangles[facingDirection],
                metadata.Origin);
        }
    }

    public struct Metadata
    {
        public Vector2 Origin;
        public Dictionary<FacingDirection, Rectangle> SourceRectangles;
    }

    public struct ProcessedEntityData
    {
        public Texture2D Texture;
        public Rectangle SourceRectangle;
        public Vector2 Origin;

        public ProcessedEntityData(
            Texture2D texture,
            Rectangle sourceRectangle,
            Vector2 origin)
        {
            Texture = texture;
            SourceRectangle = sourceRectangle;
            Origin = origin;
        }
    }
}
