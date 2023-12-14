using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using Utils;
using Utils.Rendering;

namespace UI;

// only vertical
public class ScrollableContainer : SpriteUIElement
{
    [DataMember]
    public string ScrollboxTexture;
    [DataMember]
    public string ScrollbarBackgroundTexture;
    [DataMember]
    public int ScrollTick = 50;

    [IgnoreDataMember]
    protected bool ScrollboxTextureStale = true;
    [IgnoreDataMember]
    protected UISpriteSheet.ProcessedData? ScrollboxSpriteSheetData;
    [IgnoreDataMember]
    protected bool ScrollbarBackgroundTextureStale = true;
    [IgnoreDataMember]
    protected UISpriteSheet.ProcessedData? ScrollbarBackgroundSpriteSheetData;
    [IgnoreDataMember]
    protected Rectangle ScrollbarDestinationCache = Rectangle.Empty;
    [IgnoreDataMember]
    protected Rectangle TotalAreaCache = Rectangle.Empty;
    [IgnoreDataMember]
    private int ScrollOffset;
    [IgnoreDataMember]
    private int MaxScrollOffset;
    [IgnoreDataMember]
    private int ScrollboxDrawOffset;
    [IgnoreDataMember]
    private int ScrollboxDrawHeight;
    [IgnoreDataMember]
    private bool DraggingScrollbox = false;

    public override void Update(
        GameTime gameTime,
        UIState state,
        UISpriteSheet uiSpriteSheet,
        ControllerManager controllerManager)
    {
        var updateDrawOffset = false;
        var mousePosition = controllerManager.CurrentMouseState.Position;
        if (MaxScrollOffset > 0 && TotalAreaCache.Contains(mousePosition))
        {
            if (controllerManager.IsMouseScrollingDown())
            {
                ScrollOffset += ScrollTick;
                if (ScrollOffset > MaxScrollOffset)
                {
                    ScrollOffset = MaxScrollOffset;
                }
                updateDrawOffset = true;
            }
            else if (controllerManager.IsMouseScrollingUp() && ScrollOffset > 0)
            {
                ScrollOffset -= ScrollTick;
                if (ScrollOffset < 0)
                {
                    ScrollOffset = 0;
                }
                updateDrawOffset = true;
            }
            else if (controllerManager.IsLeftMouseInitialPressed() && ScrollbarDestinationCache.Contains(mousePosition))
            {
                DraggingScrollbox = true;
            }
        }
        if (DraggingScrollbox)
        {
            if (controllerManager.IsLeftMouseUp())
            {
                DraggingScrollbox = false;
            }
            else
            {
                ScrollOffset = (int)(MaxScrollOffset
                    * (mousePosition.Y - ScrollbarDestinationCache.Y - (ScrollboxDrawHeight / 2))
                    / (float)(ScrollbarDestinationCache.Height - ScrollboxDrawHeight));
                if (ScrollOffset < 0)
                {
                    ScrollOffset = 0;
                }
                else if (ScrollOffset > MaxScrollOffset)
                {
                    ScrollOffset = MaxScrollOffset;
                }
                updateDrawOffset = true;
            }
        }
        if (updateDrawOffset)
        {
            ScrollboxDrawOffset = (int)((ScrollbarDestinationCache.Height - ScrollboxDrawHeight) * (ScrollOffset / (float)MaxScrollOffset));
        }
        if (ScrollbarBackgroundTextureStale && ScrollbarBackgroundTexture != null)
        {
            ScrollbarBackgroundSpriteSheetData = uiSpriteSheet[ScrollbarBackgroundTexture];
            ScrollbarBackgroundTextureStale = false;
        }
        if (ScrollboxTextureStale && ScrollboxTexture != null)
        {
            ScrollboxSpriteSheetData = uiSpriteSheet[ScrollboxTexture];
            ScrollboxTextureStale = false;
        }
        base.Update(gameTime, state, uiSpriteSheet, controllerManager);
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle drawArea, Point offset)
    {
        if (!Hidden && !(SpriteSheetData == null && (Height == null || Width == null)))
        {
            if (DestinationCache == Rectangle.Empty || CachedDrawArea != drawArea || CachedOffset != offset)
            {
                CachedDrawArea = drawArea;
                CachedOffset = offset;
                MaxScrollOffset = int.MinValue;
                ScrollbarDestinationCache = Rectangle.Empty;
                DestinationCache = PreComputeDestinationCache(drawArea, offset);
                TotalAreaCache = DestinationCache;
            }
            if (ScrollbarDestinationCache == Rectangle.Empty && (ScrollbarBackgroundSpriteSheetData != null || ScrollboxSpriteSheetData != null))
            {
                ScrollbarDestinationCache = new Rectangle(
                    x: DestinationCache.X + DestinationCache.Width,
                    y: DestinationCache.Y,
                    width: ScrollbarBackgroundSpriteSheetData?.SourceRectangle.Width ?? ScrollboxSpriteSheetData.Value.SourceRectangle.Width,
                    height: DestinationCache.Height);
                TotalAreaCache = new Rectangle(
                    x: DestinationCache.X,
                    y: DestinationCache.Y,
                    width: DestinationCache.Width + ScrollbarDestinationCache.Width,
                    height: DestinationCache.Height);
            }
            if (!drawArea.Intersects(DestinationCache))
            {
                return;
            }
            if (SpriteSheetData != null)
            {
                var data = SpriteSheetData.Value;
                spriteBatch.Draw(
                    data.Texture,
                    destinationRectangle: DestinationCache,
                    sourceRectangle: data.SourceRectangle,
                    Color.White,
                    rotation: 0f,
                    data.Origin,
                    SpriteEffects.None,
                    layerDepth: 0f);
            }
            if (ScrollbarBackgroundSpriteSheetData != null)
            {
                var data = ScrollbarBackgroundSpriteSheetData.Value;
                spriteBatch.Draw(
                    data.Texture,
                    destinationRectangle: ScrollbarDestinationCache,
                    sourceRectangle: data.SourceRectangle,
                    Color.White,
                    rotation: 0f,
                    data.Origin,
                    SpriteEffects.None,
                    layerDepth: 0f);
            }
            if (ScrollboxSpriteSheetData != null && MaxScrollOffset != int.MinValue)
            {
                var data = ScrollboxSpriteSheetData.Value;
                var scrollboxDestinationCache = new Rectangle(
                    x: ScrollbarDestinationCache.X,
                    y: ScrollbarDestinationCache.Y + ScrollboxDrawOffset,
                    width: ScrollbarDestinationCache.Width,
                    height: ScrollboxDrawHeight);
                spriteBatch.Draw(
                    data.Texture,
                    destinationRectangle: scrollboxDestinationCache,
                    sourceRectangle: data.SourceRectangle,
                    ScrollboxDrawHeight == DestinationCache.Height
                        ? Color.Gray
                        : Color.White,
                    rotation: 0f,
                    data.Origin,
                    SpriteEffects.None,
                    layerDepth: 0f);
            }
            spriteBatch.End();
            using (ScissorRectangleScope.Create(spriteBatch, DestinationCache))
            {
                if (MaxScrollOffset == int.MinValue)
                {
                    var maxY = int.MinValue;
                    var maxYHeight = 0;
                    foreach (var child in Children)
                    {
                        // No scroll for initial draw so child x and heights are calculated correctly for MaxScrollOffset
                        child.Draw(spriteBatch, DestinationCache, Point.Zero);
                        var y = child.DestinationCache.Y;
                        var height = child.DestinationCache.Height;
                        if (y > maxY)
                        {
                            maxY = y;
                            maxYHeight = height;
                        }
                        else if (y == maxY && height > maxYHeight)
                        {
                            maxYHeight = height;
                        }
                    }
                    var totalChildHeight = maxY - DestinationCache.Y + maxYHeight;
                    MaxScrollOffset = totalChildHeight - DestinationCache.Height;
                    if (MaxScrollOffset > 0)
                    {
                        ScrollboxDrawHeight = (int)(DestinationCache.Height * (DestinationCache.Height / (float)totalChildHeight));
                    }
                    else
                    {
                        ScrollboxDrawHeight = DestinationCache.Height;
                    }
                }
                else
                {
                    var childOffset = new Point(x: 0, y: -ScrollOffset);
                    foreach (var child in Children)
                    {
                        child.Draw(spriteBatch, DestinationCache, childOffset);
                    }
                }
            }
            spriteBatch.Begin();
        }
    }
}
