using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Utils.Rendering;

public sealed class RenderTargetScope : IDisposable
{
    private readonly SpriteBatch _spriteBatch;
    private readonly bool _began;

    private RenderTargetScope(SpriteBatch spriteBatch, bool began)
    {
        _spriteBatch = spriteBatch;
        _began = began;
    }

    public void Dispose()
    {
        if (_began)
        {
            _spriteBatch.End();
        }
        _spriteBatch.GraphicsDevice.SetRenderTarget(null);
    }

    public static RenderTargetScope Create(SpriteBatch spriteBatch, RenderTarget2D renderTarget, bool begin = true)
    {
        var scope = new RenderTargetScope(spriteBatch, began: begin);
        spriteBatch.GraphicsDevice.SetRenderTarget(renderTarget);
        spriteBatch.GraphicsDevice.Clear(Color.Transparent);
        if (begin)
        {
            spriteBatch.Begin(blendState: BlendState.AlphaBlend);
        }
        return scope;
    }
}
