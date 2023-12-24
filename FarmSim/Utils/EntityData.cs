using FarmSim.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Utils.Data;

namespace FarmSim.Utils;

class EntityData : ISpriteData, IBuildableData
{
    public string Source { get; set; }
    // "origin" is from point the entity "stands"
    public OriginData Origin { get; set; }
    // Some entities are deployable as buildings (e.g. plant monsters as crops)
    public Zoning[] Buildable { get; set; } = Array.Empty<Zoning>();
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }
    public Dictionary<FacingDirection, Point> DirectionFrames { get; set; }
}
