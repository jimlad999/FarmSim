using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Utils;

namespace UI;

[DataContract]
public abstract class UIElement
{
    [DataMember]
    public string Id;
    // texture key lookup in sprite sheet
    [DataMember]
    public string Texture;
    [DataMember]
    public string Top;
    [DataMember]
    public string Left;
    [DataMember]
    public string Bottom;
    [DataMember]
    public string Right;
    [DataMember]
    public string Width;
    [DataMember]
    public string Height;
    [DataMember]
    public Alignment? HorizontalAlignment;
    [DataMember]
    public Alignment? VerticalAlignment;
    [DataMember]
    public bool Hidden;
    [DataMember]
    public UIElement[] Children = Array.Empty<UIElement>();

    [IgnoreDataMember]
    protected bool TextureStale = true;
    [IgnoreDataMember]
    protected UISpriteSheet.ProcessedData? SpriteSheetData;

    [IgnoreDataMember]
    protected Rectangle DestinationCache = Rectangle.Empty;

    public void Resize()
    {
        DestinationCache = Rectangle.Empty;
        foreach (var uiElement in Children)
        {
            uiElement.Resize();
        }
    }

    public virtual void Update(
        GameTime gameTime,
        UISpriteSheet uiSpriteSheet,
        ControllerManager controllerManager)
    {
        if (TextureStale && Texture != null)
        {
            SpriteSheetData = uiSpriteSheet[Texture];
            TextureStale = false;
        }
        foreach (var child in Children)
        {
            child.Update(gameTime, uiSpriteSheet, controllerManager);
        }
    }

    public virtual void Draw(SpriteBatch spriteBatch, Rectangle drawArea)
    {
        // Empty, transparent panels can be built without textures. Must specify Height and Width at least.
        // Empty destination is a no-op. Texture probably hasn't loaded or been set or not a transparent panel.
        if (!Hidden && !(SpriteSheetData == null && (Height == null || Width == null)))
        {
            if (DestinationCache == Rectangle.Empty)
            {
                DestinationCache = ComputeScreenDestination(drawArea);
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
            foreach (var child in Children)
            {
                child.Draw(spriteBatch, DestinationCache);
            }
        }
    }

    private Rectangle ComputeScreenDestination(Rectangle drawArea)
    {
        var height = Height != null ? ToPixels(Height, drawArea.Height) : SpriteSheetData.Value.SourceRectangle.Height;
        var width = Width != null ? ToPixels(Width, drawArea.Width) : SpriteSheetData.Value.SourceRectangle.Width;
        var y = ComputePosition(VerticalAlignment, startValue: Top, endValue: Bottom, thisDimensionSize: height, parentDimensionSize: drawArea.Height);
        var x = ComputePosition(HorizontalAlignment, startValue: Left, endValue: Right, thisDimensionSize: width, parentDimensionSize: drawArea.Width);

        return new Rectangle(x: drawArea.X + x, y: drawArea.Y + y, width: width, height: height);
    }

    private static int ComputePosition(
        Alignment? alignment,
        string startValue,
        string endValue,
        int thisDimensionSize,
        int parentDimensionSize)
    {
        int value = 0;
        if (alignment != null)
        {
            value = ToPixels(alignment.Value, thisDimensionSize: thisDimensionSize, parentDimensionSize: parentDimensionSize);
        }
        if (startValue != null)
        {
            value += ToPixels(startValue, parentDimensionSize);
        }
        else if (endValue != null)
        {
            if (alignment != null)
            {
                value -= ToPixels(endValue, parentDimensionSize);
            }
            else
            {
                value += parentDimensionSize - thisDimensionSize - ToPixels(endValue, parentDimensionSize);
            }
        }
        return value;
    }

    private static int ToPixels(string positionValue, int parentDimensionSize)
    {
        if (positionValue.EndsWith('%'))
        {
            var fraction = float.Parse(positionValue.Substring(0, positionValue.Length - 1)) / 100f;
            return (int)(parentDimensionSize * fraction);
        }
        return int.Parse(positionValue);
    }

    private static int ToPixels(Alignment alignment, int thisDimensionSize, int parentDimensionSize)
    {
        switch (alignment)
        {
            case Alignment.Top:
            case Alignment.Left:
                return 0;
            case Alignment.Bottom:
            case Alignment.Right:
                return parentDimensionSize - thisDimensionSize;
            case Alignment.Center:
                return (parentDimensionSize - thisDimensionSize) / 2;
            default:
                // Just draw something
                return 0;
        }
    }
}