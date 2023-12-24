using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Utils;
using Utils.Rendering;

namespace FarmSim.Utils;

class SpriteSheet
{
    public Tileset Tileset;
    public EntitySpriteSheet Entities;

    public SpriteSheet(Tileset tileset, EntitySpriteSheet entities)
    {
        Tileset = tileset;
        Entities = entities;
    }

    public ProcessedSpriteData this[string spriteSheetKey]
    {
        get
        {
            if (Tileset.TryGetValue(spriteSheetKey, out var value))
            {
                return value;
            }
            return Entities[spriteSheetKey];
        }
    }
}

abstract class SpriteSheet<TSpriteData, TMetadata>
    where TSpriteData : ISpriteData
{
    protected readonly Dictionary<string, TMetadata> _metadata = new();
    protected RenderTarget2D _renderTarget;

    public ProcessedSpriteData this[string spriteSheetKey]
    {
        get
        {
            var metadata = _metadata[spriteSheetKey];
            return CreateProcessedSpriteData(_renderTarget, metadata);
        }
    }

    public bool TryGetValue(string spriteSheetKey, out ProcessedSpriteData value)
    {
        if (_metadata.TryGetValue(spriteSheetKey, out var metadata))
        {
            value = CreateProcessedSpriteData(_renderTarget, metadata);
            return true;
        }
        value = default;
        return false;
    }

    protected void ProcessData(SpriteBatch spriteBatch, ISpriteSheetData<TSpriteData> spriteSheetData)
    {
        using (var disposeScope = new DeferredDisposeScope())
        {
            var totalWidth = 0;
            var totalHeight = 0;
            var sprites = new Dictionary<string, (Texture2D, Rectangle)>();
            foreach (var data in spriteSheetData.Data)
            {
                var texture = Texture2D.FromFile(spriteBatch.GraphicsDevice, $"{spriteSheetData.BaseFolder}/{data.Value.Source}");
                // destinationRectangle to render to _renderTarget.
                // Will then be used as the base for each frames sourceRectangles from the _renderTarget.
                var destinationRectangle = new Rectangle(
                    x: 0,
                    y: totalHeight,
                    width: texture.Width,
                    height: texture.Height);
                _metadata[data.Key] = GetMetadata(data.Value, destinationRectangle);
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

    protected abstract TMetadata GetMetadata(TSpriteData data, Rectangle destinationRectangle);

    protected abstract ProcessedSpriteData CreateProcessedSpriteData(RenderTarget2D renderTarget, TMetadata metadata);
}
