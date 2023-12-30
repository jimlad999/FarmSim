using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Utils;

namespace UI;

[DataContract]
public abstract class UIElement
{
    [DataMember]
    public string Id;
    [DataMember]
    public string Top;
    [DataMember]
    public string Left;
    [DataMember]
    public string Bottom;
    [DataMember]
    public string Right;
    [DataMember]
    public Alignment? HorizontalAlignment;
    [DataMember]
    public Alignment? VerticalAlignment;
    [DataMember]
    public bool Hidden;
    [DataMember]
    public string Margin;
    [DataMember]
    public List<UIElement> Children = new();

    [IgnoreDataMember]
    public Point? MarginComputed;
    [IgnoreDataMember]
    public Rectangle DestinationCache = Rectangle.Empty;
    [IgnoreDataMember]
    protected Rectangle CachedDrawArea;
    [IgnoreDataMember]
    protected Point CachedOffset;

    public void Resize()
    {
        DestinationCache = Rectangle.Empty;
        foreach (var uiElement in Children)
        {
            uiElement.Resize();
        }
    }

    // Calculate DestinationCache ahead of time but don't set it.
    // Used for parent elements to decide if to change drawArea or offset based on child size.
    public virtual Rectangle PreComputeDestinationCache(Rectangle drawArea, Point offset)
    {
        if (MarginComputed == null)
        {
            MarginComputed = Utils.ComputePaddingOrMargin(Margin, drawArea);
        }
        var margin = MarginComputed.Value;
        return new Rectangle(
            x: drawArea.X + offset.X + margin.X,
            y: drawArea.Y + offset.Y + margin.Y,
            width: drawArea.Width - offset.X - margin.X * 2,
            height: drawArea.Height - offset.Y - margin.Y * 2);
    }

    public virtual bool TryGetById<T>(string id, out T result) where T : UIElement
    {
        if (id == Id)
        {
            result = this as T;
            return this is T;
        }
        foreach (var child in Children)
        {
            if (child.TryGetById(id, out result))
            {
                return true;
            }
        }

        result = null;
        return false;
    }

    public virtual void Update(
        GameTime gameTime,
        UIState state,
        UISpriteSheet uiSpriteSheet,
        ControllerManager controllerManager)
    {
        if (Hidden)
        {
            return;
        }
        UpdateUIState(state, controllerManager);
        foreach (var child in Children)
        {
            child.Update(gameTime, state, uiSpriteSheet, controllerManager);
        }
    }

    protected virtual void UpdateUIState(UIState state, ControllerManager controllerManager)
    {
        if (!state.IsMouseOverElement && DestinationCache.Contains(controllerManager.CurrentMouseState.Position))
        {
            state.IsMouseOverElement = true;
        }
    }

    public virtual void Draw(SpriteBatch spriteBatch, Rectangle drawArea, Point offset)
    {
        if (Hidden)
        {
            return;
        }
        if (DestinationCache == Rectangle.Empty || CachedDrawArea != drawArea || CachedOffset != offset)
        {
            CachedDrawArea = drawArea;
            CachedOffset = offset;
            DestinationCache = PreComputeDestinationCache(drawArea, offset);
        }
        if (!drawArea.Intersects(DestinationCache))
        {
            return;
        }
        DrawChildren(spriteBatch);
    }

    protected virtual void DrawChildren(SpriteBatch spriteBatch)
    {
        foreach (var child in Children)
        {
            // In general, offset from the Draw method will have already been considered in DestinationCache
            child.Draw(spriteBatch, DestinationCache, offset: Point.Zero);
        }
    }
}
