using Microsoft.Xna.Framework;

namespace FarmSim.Utils;

class FrameData
{
    // top left of frame (frame size is expected to be same for all frames)
    public int X;
    public int Y;
    // milliseconds
    public double Duration;
    // Which frame to execute animation actions on.
    // Should only be 1 key frame per animation.
    // Optional.
    public bool KeyFrame;
}
