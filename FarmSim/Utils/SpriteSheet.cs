using FarmSim.Entities;
using FarmSim.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.Rendering;

namespace FarmSim.Utils;

interface ISpriteSheet
{
    ProcessedSpriteData this[Animation animation] { get; }
}

abstract class SpriteSheet<TSpriteData> : ISpriteSheet
    where TSpriteData : ISpriteData, IBuildableData
{
    protected readonly Dictionary<string, Metadata> _metadata = new();
    protected RenderTarget2D _renderTarget;

    public ProcessedSpriteData this[Animation animation]
    {
        get
        {
            var metadata = _metadata[animation.SpriteSheetKey];
            return CreateProcessedSpriteData(_renderTarget, metadata, animation.AnimationKey, animation.FacingDirection, animation.ActiveFrameIndex);
        }
    }

    public bool TryGetValue(Animation animation, out ProcessedSpriteData value)
    {
        if (_metadata.TryGetValue(animation.SpriteSheetKey, out var metadata))
        {
            value = CreateProcessedSpriteData(_renderTarget, metadata, animation.AnimationKey, animation.FacingDirection, animation.ActiveFrameIndex);
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

    private static ProcessedSpriteData CreateProcessedSpriteData(RenderTarget2D renderTarget, Metadata metadata, string animationKey, FacingDirection facingDirection, int frame)
    {
        var animationMetadata = metadata.Animations[animationKey];
        return new ProcessedSpriteData(
            renderTarget,
            animationMetadata.Frames[facingDirection][frame],
            animationMetadata.Origin,
            metadata.Buildable);
    }

    private static Metadata GetMetadata(TSpriteData data, Rectangle destinationRectangle)
    {
        return new Metadata
        {
            Animations = data.Animations.ToDictionary(
                a => a.Key,
                a => new AnimationMetadata
                {
                    Origin = a.Value.Origin?.Convert() ?? Vector2.Zero,
                    Frames = a.Value.DirectionFrames.ToDictionary(
                        f => f.Key,
                        f => f.Value.Select(d =>
                            new Rectangle(
                                x: destinationRectangle.X + d.X,
                                y: destinationRectangle.Y + d.Y,
                                width: a.Value.FrameWidth,
                                height: a.Value.FrameHeight)
                            ).ToArray()
                        ),
                }),
            Buildable = data.Buildable
        };
    }

    public class Metadata
    {
        public Dictionary<string, AnimationMetadata> Animations;
        // SPEED HACK: to avoid double lookups when rendering building placement preview
        public Zoning[] Buildable;
    }

    public class AnimationMetadata
    {
        public Vector2 Origin;
        public Dictionary<FacingDirection, Rectangle[]> Frames;
    }
}
