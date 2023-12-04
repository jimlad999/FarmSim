using FarmSim.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FarmSim.Utils;

class EntityData
{
    public string Source { get; set; }
    // "origin" is from point the entity "stands"
    public TilesetOrigin Origin { get; set; }
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }
    public Dictionary<FacingDirection, Point> DirectionFrames { get; set; }
}
