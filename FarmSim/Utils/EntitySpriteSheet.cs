using FarmSim.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FarmSim.Utils;

class EntitySpriteSheet
{
    private readonly Dictionary<string, Rectangle> _sourceRectangle = new();
    private readonly Dictionary<string, Vector2> _origin = new();
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
                _origin[data.Key] = data.Value.Origin?.Convert() ?? Vector2.Zero;
                // destinationRectangle to render to _renderTarget.
                // Will then be used as the sourceRectangle from the _renderTarget.
                var sourceRectangle = new Rectangle(
                    x: 0,
                    y: totalHeight,
                    width: texture.Width,
                    height: texture.Height);
                _sourceRectangle[data.Key] = sourceRectangle;
                sprites[data.Key] = (texture, sourceRectangle);

                disposeScope.Disposables.Add(texture);

                totalHeight += texture.Height;
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
                spriteBatch.GraphicsDevice.Clear(Color.Transparent);
                spriteBatch.Begin(blendState: BlendState.AlphaBlend);

                foreach (var sprite in sprites.Values)
                {
                    spriteBatch.Draw(sprite.Item1, destinationRectangle: sprite.Item2, Color.White);
                }

                spriteBatch.End();
            }
        }
    }

    public ProcessedEntityData this[string entity]
    {
        get => new ProcessedEntityData(
            _renderTarget,
            _sourceRectangle[entity],
            _origin[entity]);
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
