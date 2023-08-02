using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace FarmSim.Utils;

static class TextureUtils
{
    public static Texture2D LoadFromFile(this GraphicsDeviceManager graphics, string filename)
    {
        using (var stream = File.OpenRead(filename))
        {
            return Texture2D.FromStream(graphics.GraphicsDevice, stream);
        }
    }
}
