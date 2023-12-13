using Microsoft.Xna.Framework;
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
        _spriteBatch.End();
        _spriteBatch.GraphicsDevice.SetRenderTarget(null);
    }

    public static RenderTargetScope Create(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
    {
        var scope = new RenderTargetScope(spriteBatch);
        spriteBatch.GraphicsDevice.SetRenderTarget(renderTarget);
        spriteBatch.GraphicsDevice.Clear(Color.Transparent);
        spriteBatch.Begin(blendState: BlendState.AlphaBlend);
        return scope;
    }
}
