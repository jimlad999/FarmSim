using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Utils.Rendering;

public sealed class ScissorRectangleScope : IDisposable
{
    private static readonly RasterizerState ScissorRasterizerState = new RasterizerState
    {
        ScissorTestEnable = true
    };
    private readonly SpriteBatch _spriteBatch;
    private readonly Rectangle _originalScissorRectangle;

    private ScissorRectangleScope(SpriteBatch spriteBatch, Rectangle originalScissorRectangle)
    {
        _spriteBatch = spriteBatch;
        _originalScissorRectangle = originalScissorRectangle;
    }

    public void Dispose()
    {
        _spriteBatch.End();
        _spriteBatch.GraphicsDevice.ScissorRectangle = _originalScissorRectangle;
    }

    public static ScissorRectangleScope Create(SpriteBatch spriteBatch, Rectangle scissorRectangle)
    {
        var scope = new ScissorRectangleScope(spriteBatch, spriteBatch.GraphicsDevice.ScissorRectangle);
        spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
        spriteBatch.Begin(SpriteSortMode.Deferred, rasterizerState: ScissorRasterizerState);
        return scope;
    }
}
