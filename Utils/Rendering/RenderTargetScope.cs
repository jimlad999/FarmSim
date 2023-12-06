using Microsoft.Xna.Framework.Graphics;
using System;

namespace Utils.Rendering;

public sealed class RenderTargetScope : IDisposable
{
    private readonly SpriteBatch _spriteBatch;

    private RenderTargetScope(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
    }

    public void Dispose()
    {
        _spriteBatch.GraphicsDevice.SetRenderTarget(null);
    }

    public static RenderTargetScope Create(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
    {
        var scope = new RenderTargetScope(spriteBatch);
        spriteBatch.GraphicsDevice.SetRenderTarget(renderTarget);
        return scope;
    }
}
