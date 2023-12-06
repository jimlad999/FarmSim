using FarmSim.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Utils.Data;

namespace FarmSim.Utils;

class EntityData
{
    public string Source { get; set; }
    // "origin" is from point the entity "stands"
    public OriginData Origin { get; set; }
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }
    public Dictionary<FacingDirection, Point> DirectionFrames { get; set; }
}
