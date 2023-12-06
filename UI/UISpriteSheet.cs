using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using UI.Data;
using Utils;
using Utils.Rendering;

namespace UI;

public class UISpriteSheet
{
    private readonly Dictionary<string, Metadata> _metadata = new();
    private readonly RenderTarget2D _renderTarget;

    public UISpriteSheet(
        SpriteBatch spriteBatch,
        UISpriteData uiData)
    {
        using (var disposeScope = new DeferredDisposeScope())
        {
            var totalWidth = 0;
            var totalHeight = 0;
            var sprites = new Dictionary<string, (Texture2D, Rectangle)>();
            foreach (var data in uiData.Elements)
            {
                var texture = Texture2D.FromFile(spriteBatch.GraphicsDevice, $"{uiData.BaseFolder}/{data.Value.Source}");
                // destinationRectangle to render to _renderTarget.
                // Will then be used as the sourceRectangle from the _renderTarget.
                var destinationRectangle = new Rectangle(
                    x: 0,
                    y: totalHeight,
                    width: texture.Width,
                    height: texture.Height);
                _metadata[data.Key] = new Metadata
                {
                    SourceRectangle = destinationRectangle,
                    Origin = data.Value.Origin?.Convert() ?? Vector2.Zero,
                };
                sprites[data.Key] = (texture, destinationRectangle);

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

    public ProcessedData this[string key]
    {
        get
        {
            var metadata = _metadata[key];
            return new ProcessedData(
                _renderTarget,
                metadata.SourceRectangle,
                metadata.Origin);
        }
    }

    public struct Metadata
    {
        public Vector2 Origin;
        public Rectangle SourceRectangle;
    }

    public struct ProcessedData
    {
        public Texture2D Texture;
        public Rectangle SourceRectangle;
        public Vector2 Origin;

        public ProcessedData(
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
