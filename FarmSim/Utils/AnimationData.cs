using FarmSim.Entities;
using System.Collections.Generic;
using Utils.Data;

namespace FarmSim.Utils;

class AnimationData
{
    // currently expect all frames to be same size
    public int FrameWidth;
    public int FrameHeight;
    public Dictionary<FacingDirection, FrameData[]> DirectionFrames;
    // Usage (based on how image is rendering)
    // - TileData: "origin" is from top left corner of the tile
    // - EntityData: "origin" is from point the entity "stands"
    public OriginData Origin { get; set; }
}
