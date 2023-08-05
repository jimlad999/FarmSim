using FarmSim.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FarmSim.Utils;

class Tileset
{
    private readonly Dictionary<string, Rectangle> _sourceRectangle = new();
    private readonly RenderTarget2D _renderTarget;

    public Tileset(
        SpriteBatch spriteBatch,
        TilesetData tilesetData)
    {
        using (var disposeScope = new DeferredDisposeScope())
        {
            var totalWidth = 0;
            var totalHeight = 0;
            var sprites = new Dictionary<string, (Texture2D, Rectangle)>();
            foreach (var data in tilesetData.Tilesets)
            {
                var texture = Texture2D.FromFile(spriteBatch.GraphicsDevice, $"{tilesetData.BaseFolder}/{data.Value.Source}");
                // destinationRectangle to render to _renderTarget.
                // Will then be used as the sourceRectangle from the _renderTarget.
                var sourceRectangle = new Rectangle(
                    x: totalWidth,
                    y: 0,
                    width: texture.Width,
                    height: texture.Height);
                _sourceRectangle[data.Key] = sourceRectangle;
                sprites[data.Key] = (texture, sourceRectangle);

                disposeScope.Disposables.Add(texture);

                totalWidth += texture.Width;
                if (texture.Height > totalHeight)
                {
                    totalHeight = texture.Height;
                }
            }
            _renderTarget = new RenderTarget2D(
                spriteBatch.GraphicsDevice,
                width: totalWidth,
                height: totalHeight);
            using (RenderTargetScope.Create(spriteBatch, _renderTarget))
            {
                spriteBatch.Begin();

                foreach (var sprite in sprites.Values)
                {
                    spriteBatch.Draw(sprite.Item1, destinationRectangle: sprite.Item2, Color.White);
                }

                spriteBatch.End();
            }
        }
    }

    public (Texture2D, Rectangle) this[string tileset]
    {
        get => (_renderTarget, _sourceRectangle[tileset]);
    }
}
