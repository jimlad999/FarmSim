using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Utils;
using Utils.Rendering;

namespace FarmSim.Utils;

class Tileset
{
    private readonly Dictionary<string, Rectangle> _sourceRectangle = new();
    private readonly Dictionary<string, Vector2> _origin = new();
    private readonly Dictionary<string, BuildingType[]> _buildable = new();
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
                _buildable[data.Key] = data.Value.Buildable ?? Array.Empty<BuildingType>();
                var texture = Texture2D.FromFile(spriteBatch.GraphicsDevice, $"{tilesetData.BaseFolder}/{data.Value.Source}");
                _origin[data.Key] = data.Value.Origin?.Convert() ?? Vector2.Zero;
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

    public ProcessedTileData this[string tileset]
    {
        get => new ProcessedTileData(
            _renderTarget,
            _sourceRectangle[tileset],
            _origin[tileset],
            _buildable[tileset]);
    }

    public struct ProcessedTileData
    {
        public Texture2D Texture;
        public Rectangle SourceRectangle;
        public Vector2 Origin;
        public ICollection<BuildingType> Buildable;

        public ProcessedTileData(
            Texture2D texture,
            Rectangle sourceRectangle,
            Vector2 origin,
            BuildingType[] buildable)
        {
            Texture = texture;
            SourceRectangle = sourceRectangle;
            Origin = origin;
            Buildable = buildable;
        }
    }
}
